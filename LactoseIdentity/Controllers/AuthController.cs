using System.Globalization;
using System.Security.Claims;
using Lactose.Identity.Auth;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Dtos.Auth;
using Lactose.Identity.Metrics;
using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp;
using LactoseWebApp.Auth;
using LactoseWebApp.Auth.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MQTTnet;
using LoginRequest = Lactose.Identity.Dtos.Auth.LoginRequest;

namespace Lactose.Identity.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    readonly ILogger<AuthController> _logger;
    readonly IUsersRepo _usersRepo;
    readonly IOptions<AuthOptions> _authOptions;
    readonly IPasswordHasher<User> _passwordHasher;
    readonly IOptions<NewUserOptions> _newUserOptions;
    readonly IOptions<PermissionsOptions> _permissionsOptions;
    readonly PermissionsService _permissionsService;
    readonly JwtTokenHandler _tokenHandler;
    readonly IMqttClient _mqttClient;

    public AuthController(
        ILogger<AuthController> logger,
        IUsersRepo usersRepo,
        IOptions<AuthOptions> authOptions,
        IPasswordHasher<User> passwordHasher,
        IOptions<NewUserOptions> newUserOptions,
        IOptions<PermissionsOptions> permissionsOptions,
        PermissionsService permissionsService,
        JwtTokenHandler tokenHandler,
        IMqttClient mqttClient)
    {
        _logger = logger;
        _usersRepo = usersRepo;
        _passwordHasher = passwordHasher;
        _authOptions = authOptions;
        _newUserOptions = newUserOptions;
        _permissionsOptions = permissionsOptions;
        _permissionsService = permissionsService;
        _tokenHandler = tokenHandler;
        _mqttClient = mqttClient;
     
        logger.LogInformation($"Using Auth Options: {_authOptions.Value.ToIndentedJson()}");
    }

    [HttpPost("login", Name = "Login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        User? foundUser = await _usersRepo.GetUserByEmail(request.Email);
        if (foundUser is null)
            return BadRequest("No user found");
        
        if (foundUser.PasswordHash is null)
            return BadRequest("No user found");
        
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
            foundUser, 
            foundUser.PasswordHash,
            request.Password);
        
        if (result == PasswordVerificationResult.Failed)
            return BadRequest("Invalid password");

        string refreshToken = await _tokenHandler.CreateJwtRefreshTokenForUser(foundUser);
        string accessToken = _tokenHandler.CreateJwtAccessTokenForUser(foundUser);
        
        if (_authOptions.Value.UseCookieForAccessToken)
        {
            // As well as the standard JWT Bearer Token in the Authorization Header,
            // the service should also support JWTs being stored in Cookies too as it's
            // easier to manage on the client side.
            Response.Cookies.Append(
                AuthDefaults.JwtAccessTokenCookieName,
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(_authOptions.Value.JwtExpireMinutes)
                });
        }

        if (_authOptions.Value.UseCookieForRefreshToken)
        {
            Response.Cookies.Append(
                AuthDefaults.JwtRefreshTokenCookieName,
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(_authOptions.Value.JwtRefreshExpireHours),
                    Path = GetRefreshActionRelativeUrl()
                });
        }
        
        await _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/identity/presence/{foundUser.Id!}/online")
            .WithPayload(new UserLoggedInEvent
            {
                UserId = foundUser.Id!, 
                TimeLastLoggedIn = foundUser.TimeLastLoggedIn
            }.ToJson())
            .Build());
        
        IdentityMetrics.LoginsCounter.Add(1);
        
        return Ok(new LoginResponse
        {
            Id = foundUser.Id,
            Email = foundUser.Email,
            DisplayName = foundUser.DisplayName,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
    
    [HttpPost("signup", Name = "Signup")]
    public async Task<ActionResult<SignupResponse>> Signup(SignupRequest request)
    {
        User? foundUser = await _usersRepo.GetUserByEmail(request.Email);
        if (foundUser is not null)
            return BadRequest("User already exists");
        
        var newUser = new User
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            TimeCreated = DateTime.UtcNow,
            Roles = _newUserOptions.Value.DefaultRoles.ToHashSet()
        };
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        var createdUser = await _usersRepo.Set(newUser);
        if (createdUser is null)
            return StatusCode(500, "Could not create user");

        string refreshToken = await _tokenHandler.CreateJwtRefreshTokenForUser(createdUser);
        string accessToken = _tokenHandler.CreateJwtAccessTokenForUser(createdUser);

        if (_authOptions.Value.UseCookieForAccessToken)
        {
            Response.Cookies.Append(
                AuthDefaults.JwtAccessTokenCookieName,
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(_authOptions.Value.JwtExpireMinutes)
                });
        }

        if (_authOptions.Value.UseCookieForRefreshToken)
        {
            Response.Cookies.Append(
                AuthDefaults.JwtRefreshTokenCookieName,
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(_authOptions.Value.JwtRefreshExpireHours),
                    Path = GetRefreshActionRelativeUrl()
                });
        }
        
        await _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/identity/presence/{createdUser.Id!}/online")
            .WithPayload(new UserLoggedInEvent
            {
                UserId = createdUser.Id!
            }.ToJson())
            .Build());
        
        IdentityMetrics.SignupsCounter.Add(1);
        
        return Ok(new SignupResponse
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            DisplayName = createdUser.DisplayName,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
    
    [HttpPost("logout", Name = "Logout")]
    public async Task<ActionResult<LogoutResponse>> Logout(LogoutRequest request)
    {
        string? jwt = HttpContext.GetJwtAccessToken();
        
        if (string.IsNullOrEmpty(jwt))
            return Unauthorized();

        // Delete the refresh token from the DB so it can not be reused again.
        string? refreshTokenStr = Request.Cookies[AuthDefaults.JwtRefreshTokenCookieName];
        if (refreshTokenStr is not null)
        {
            RefreshToken? refreshToken = await _tokenHandler.ParseRefreshTokenFromJwt(refreshTokenStr);
            if (refreshToken is not null)
                await _tokenHandler.DeleteRefreshToken(refreshToken.Id);
        }
        
        // Delete the access and refresh tokens from the client so they can not be reused again.
        if (_authOptions.Value.UseCookieForAccessToken)
        {
            // Force reset the Jwt cookie on the client.
            Response.Cookies.Append(
                AuthDefaults.JwtAccessTokenCookieName,
                string.Empty,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1)
                });
        }

        if (_authOptions.Value.UseCookieForRefreshToken)
        {
            Response.Cookies.Append(
                AuthDefaults.JwtRefreshTokenCookieName,
                string.Empty,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1),
                    Path = GetRefreshActionRelativeUrl()
                });
        }

        IdentityMetrics.LogoutsCounter.Add(1);

        return Ok(new LogoutResponse());
    }

    [HttpPost("details", Name = "Details")]
    [Authorize] // Need to add the Authorize attribute, otherwise, Claims would not get deserialised. Kinda understandable.
    public async Task<ActionResult<DetailsResponse>> Details(DetailsRequest request)
    {
        string? jwt = HttpContext.GetJwtAccessToken();

        if (string.IsNullOrEmpty(jwt))
            return Unauthorized();

        List<string>? permissions = null;
        var claimsIdentity = User.Identity as CaseSensitiveClaimsIdentity;
        string? userId = User.GetUserId();
        if (claimsIdentity is not null && !string.IsNullOrEmpty(userId))
        {
            permissions = await _permissionsService.GetPermissionClaimsForUser(claimsIdentity, userId);
        }

        return Ok(new DetailsResponse
        {
            Id = User.GetUserId(),
            DisplayName = User.FindFirstValue(JwtRegisteredClaimNames.Name),
            Email = User.FindFirstValue(JwtRegisteredClaimNames.Email),
            TokenExpires = User.FindFirstValue(JwtRegisteredClaimNames.Exp),
            Roles = User.Claims
                .Where(c => c.Type.StartsWith(_permissionsOptions.Value.RoleClaimPrefix))
                .Select(c => c.Type).ToList(),
            Permissions = permissions,
            AccessToken = jwt
        });
    }

    [HttpPost("refresh", Name = "Refresh")]
    public async Task<ActionResult<RefreshResponse>> Refresh(RefreshRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            string? refreshTokenStr = Request.Cookies[AuthDefaults.JwtRefreshTokenCookieName];
            if (refreshTokenStr is null)
                return BadRequest("No Refresh Token was provided");

            request.RefreshToken = refreshTokenStr;
        }

        RefreshToken? refreshToken = await _tokenHandler.ParseRefreshTokenFromJwt(request.RefreshToken);
        if (refreshToken is null)
            return Unauthorized("Invalid Refresh Token was provided");

        RefreshToken? trustedRefreshToken = await _tokenHandler.GetRefreshTokenById(refreshToken.Id);
        if (trustedRefreshToken is null)
            return Unauthorized("Refresh Token does not exist on the server");
        
        if (trustedRefreshToken.ExpiresAt < DateTimeOffset.UtcNow)
            return Unauthorized($"Refresh Token has expired. Expired: ${trustedRefreshToken.ExpiresAt.ToString(CultureInfo.InvariantCulture)}");

        if (trustedRefreshToken.UserId != refreshToken.UserId)
            return Unauthorized("Refresh Token does not match");

        User? foundUser = await _usersRepo.Get(trustedRefreshToken.UserId);
        if (foundUser is null)
            return BadRequest($"Could not find user with ID: {refreshToken.UserId}");
        
        string newRefreshToken = await _tokenHandler.CreateJwtRefreshTokenForUser(foundUser);
        string newAccessToken = _tokenHandler.CreateJwtAccessTokenForUser(foundUser);
        
        if (_authOptions.Value.UseCookieForAccessToken)
        {
            // As well as the standard JWT Bearer Token in the Authorization Header,
            // the service should also support JWTs being stored in Cookies too as it's
            // easier to manage on the client side.
            Response.Cookies.Append(
                AuthDefaults.JwtAccessTokenCookieName,
                newAccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(_authOptions.Value.JwtExpireMinutes)
                });
        }

        if (_authOptions.Value.UseCookieForRefreshToken)
        {
            Response.Cookies.Append(
                AuthDefaults.JwtRefreshTokenCookieName,
                newRefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(_authOptions.Value.JwtRefreshExpireHours),
                    Path = GetRefreshActionRelativeUrl()
                });
        }
        
        // Delete the old refresh token.
        await _tokenHandler.DeleteRefreshToken(refreshToken.Id);
        
        await _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/identity/presence/{foundUser.Id!}/online")
            .WithPayload(new UserLoggedInEvent
            {
                UserId = foundUser.Id!, 
                TimeLastLoggedIn = foundUser.TimeLastLoggedIn
            }.ToJson())
            .Build());
        
        IdentityMetrics.RefreshTokensCounter.Add(1);
        IdentityMetrics.LoginsCounter.Add(1);

        return Ok(new RefreshResponse
        {
            Id = foundUser.Id,
            Email = foundUser.Email,
            DisplayName = foundUser.DisplayName,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    [HttpPost("authenticate-token", Name = "Authenticate Token")]
    public async Task<ActionResult<AuthenticateTokenResponse>> AuthenticateToken(AuthenticateTokenRequest request)
    {
        TokenValidationResult? tokenValid = await _tokenHandler.ValidateAccessToken(request.AccessToken, request.Audience);
        
        if (tokenValid is null || !tokenValid.IsValid)
            return Unauthorized($"Invalid Access Token for Audience {request.Audience}");

        Claim? userIdClaim = tokenValid.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim is null)
            return Unauthorized("Invalid User Id claim");

        User? foundUser = await _usersRepo.Get(userIdClaim.Value);
        if (foundUser is null)
            return Unauthorized($"Could not find user with ID: {userIdClaim.Value}");

        ISet<string> userRoles = foundUser.Roles;
        return Ok(new AuthenticateTokenResponse
        {
            UserRoles = userRoles
        });
    }

    [HttpPost("authenticate-token-basic", Name = "Authenticate Token (Basic)")]
    public async Task<ActionResult> AuthenticateTokenBasic()
    {
        string? jwt = HttpContext.GetJwtAccessToken();
        if (string.IsNullOrEmpty(jwt))
            return Unauthorized();
        
        TokenValidationResult? tokenValid = await _tokenHandler.ValidateAccessToken(jwt, null);
        if (tokenValid is null || !tokenValid.IsValid)
            return Unauthorized();
        
        return Ok();
    }
    
    string GetRefreshActionRelativeUrl()
    {
        // Seriously need to find a better way to do this.
        if (!string.IsNullOrEmpty(HttpContext.Request.PathBase))
            return $"{HttpContext.Request.PathBase}/auth/refresh";
        return "/auth/refresh";
    }
}