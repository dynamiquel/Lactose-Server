using System.Globalization;
using System.Security.Claims;
using System.Text;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Dtos.Auth;
using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp;
using LactoseWebApp.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using LoginRequest = Lactose.Identity.Dtos.Auth.LoginRequest;

namespace Lactose.Identity.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    readonly SigningCredentials _tokenSigningCredentials;
    readonly IUsersRepo _usersRepo;
    readonly IRefreshTokensRepo _refreshTokensRepo;
    readonly UsersController _usersController;
    readonly IOptions<AuthOptions> _authOptions;
    readonly IPasswordHasher<User> _passwordHasher;
    readonly JsonWebTokenHandler _tokenHandler = new();

    public AuthController(
        ILogger<AuthController> logger,
        IUsersRepo usersRepo,
        IRefreshTokensRepo refreshTokensRepo,
        IOptions<AuthOptions> authOptions,
        IPasswordHasher<User> passwordHasher,
        UsersController usersController)
    {
        _usersRepo = usersRepo;
        _refreshTokensRepo = refreshTokensRepo;
        _passwordHasher = passwordHasher;
        _usersController = usersController;
        _authOptions = authOptions;
     
        logger.LogInformation($"Using Auth Options: {_authOptions.Value.ToIndentedJson()}");
        
        if (string.IsNullOrWhiteSpace(authOptions.Value.JwtTokenKey))
            throw new SecurityTokenEncryptionKeyNotFoundException("No JwtTokenKey found");
        
        _tokenSigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Value.JwtTokenKey)), 
            SecurityAlgorithms.HmacSha256);
    }

    [HttpPost("login", Name = "Login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        User? foundUser = await _usersRepo.GetUserByEmail(request.Email);
        if (foundUser is null)
            return BadRequest();
        
        if (foundUser.PasswordHash is null)
            return BadRequest();
        
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
            foundUser, 
            foundUser.PasswordHash,
            request.Password);
        
        if (result == PasswordVerificationResult.Failed)
            return BadRequest();

        string refreshToken = await CreateJwtRefreshTokenForUser(foundUser);
        string accessToken = CreateJwtAccessTokenForUser(foundUser);
        
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

        return Ok(new LoginResponse
        {
            Id = foundUser.Id,
            Email = foundUser.Email,
            DisplayName = foundUser.DisplayName,
            Token = accessToken
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
            TimeCreated = DateTime.UtcNow
        };
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

        var createdUser = await _usersRepo.Set(newUser);
        if (createdUser is null)
            return StatusCode(500, "Could not create user");

        string refreshToken = await CreateJwtRefreshTokenForUser(createdUser);
        string accessToken = CreateJwtAccessTokenForUser(createdUser);

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
        
        return Ok(new SignupResponse
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            DisplayName = createdUser.DisplayName,
            Token = accessToken
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
            RefreshToken? refreshToken = await ParseRefreshTokenFromJwt(refreshTokenStr);
            if (refreshToken is not null)
                await _refreshTokensRepo.Delete(refreshToken.Id);
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
                    Expires = DateTimeOffset.UtcNow.AddDays(-1)
                });
        }


        return Ok(new LogoutResponse());
    }

    [HttpPost("details", Name = "Details")]
    [Authorize] // Need to add the Authorize attribute, otherwise, Claims would not get deserialised. Kinda understandable.
    public async Task<ActionResult<DetailsResponse>> Details(DetailsRequest request)
    {
        string? jwt = HttpContext.GetJwtAccessToken();

        if (string.IsNullOrEmpty(jwt))
            return Unauthorized();
        
        return Ok(new DetailsResponse
        {
            Id = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            DisplayName = User.FindFirstValue(JwtRegisteredClaimNames.Name),
            Email = User.FindFirstValue(JwtRegisteredClaimNames.Email),
            TokenExpires = User.FindFirstValue(JwtRegisteredClaimNames.Exp),
            Token = jwt
        });
    }

    [HttpPost("refresh", Name = "Refresh")]
    public async Task<ActionResult<RefreshResponse>> Refresh(RefreshRequest request)
    {
        if (request.RefreshToken is null)
        {
            string? refreshTokenStr = Request.Cookies[AuthDefaults.JwtRefreshTokenCookieName];
            if (refreshTokenStr is null)
                return BadRequest("No Refresh Token was provided");

            request.RefreshToken = refreshTokenStr;
        }

        RefreshToken? refreshToken = await ParseRefreshTokenFromJwt(request.RefreshToken);
        if (refreshToken is null)
            return Unauthorized("Invalid Refresh Token was provided");

        RefreshToken? trustedRefreshToken = await _refreshTokensRepo.Get(refreshToken.Id);
        if (trustedRefreshToken is null)
            return Unauthorized("Refresh Token does not exist on the server");
        
        if (trustedRefreshToken.ExpiresAt < DateTimeOffset.UtcNow)
            return Unauthorized($"Refresh Token has expired. Expired: ${trustedRefreshToken.ExpiresAt.ToString(CultureInfo.InvariantCulture)}");

        if (trustedRefreshToken.UserId != refreshToken.UserId)
            return Unauthorized("Refresh Token does not match");

        User? foundUser = await _usersRepo.Get(trustedRefreshToken.UserId);
        if (foundUser is null)
            return BadRequest($"Could not find user with ID: {refreshToken.UserId}");
        
        string newRefreshToken = await CreateJwtRefreshTokenForUser(foundUser);
        string newAccessToken = CreateJwtAccessTokenForUser(foundUser);
        
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
        await _refreshTokensRepo.Delete(refreshToken.Id);

        return Ok(new RefreshResponse
        {
            Id = foundUser.Id,
            Email = foundUser.Email,
            DisplayName = foundUser.DisplayName,
            Token = newAccessToken
        });
    }

    string CreateJwtAccessTokenForUser(User user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new (JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new (JwtRegisteredClaimNames.Name, user.DisplayName),
                new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            }),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(_authOptions.Value.JwtExpireMinutes),
            Issuer = _authOptions.Value.JwtIssuer,
            Audience = _authOptions.Value.JwtAudience,
            SigningCredentials = _tokenSigningCredentials,
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    async Task<string> CreateJwtRefreshTokenForUser(User user)
    {
        var randomTokenId = Guid.NewGuid();
        var issuedAt = DateTime.UtcNow;
        var expiresAt = DateTime.UtcNow.AddHours(_authOptions.Value.JwtRefreshExpireHours);

        // Adds the refresh token to the database.
        var refreshToken = await _refreshTokensRepo.Set(new RefreshToken
        {
            Id = randomTokenId.ToString(),
            UserId = user.Id ?? throw new InvalidOperationException(),
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Issuer = _authOptions.Value.JwtIssuer
        });

        if (refreshToken is null)
            throw new InvalidOperationException();
        
        // Creates a JWT representing the refresh token that clients will use.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new (JwtRegisteredClaimNames.Jti, randomTokenId.ToString()),
                new (JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty)
            }),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(_authOptions.Value.JwtRefreshExpireHours),
            Issuer = _authOptions.Value.JwtIssuer,
            SigningCredentials = _tokenSigningCredentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    async Task<RefreshToken?> ParseRefreshTokenFromJwt(string refreshTokenJwt)
    {
        var token = _tokenHandler.ReadJsonWebToken(refreshTokenJwt);
        TokenValidationResult? tokenValid = await _tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Value.JwtTokenKey)),
            ValidIssuer = _authOptions.Value.JwtIssuer,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateAudience = false
        });

        if (!tokenValid.IsValid)
            return null;

        token.TryGetValue(JwtRegisteredClaimNames.Jti, out string? tokenId);
        if (string.IsNullOrEmpty(tokenId))
            return null;
        token.TryGetValue(JwtRegisteredClaimNames.Sub, out string? userId);
        if (string.IsNullOrEmpty(userId))
            return null;
        
        var refreshToken = new RefreshToken
        {
            Id = tokenId,
            UserId = userId,
            IssuedAt = token.IssuedAt,
            ExpiresAt = token.ValidTo,
            Issuer = token.Issuer,
        };

        return refreshToken;
    }

    string GetRefreshActionRelativeUrl()
    {
        // Seriously need to find a better way to do this.
        if (!string.IsNullOrEmpty(HttpContext.Request.PathBase))
            return $"{HttpContext.Request.PathBase}/auth/refresh";
        return "/auth/refresh";
    }
}