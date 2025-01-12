using System.Security.Claims;
using System.Text;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Dtos.Auth;
using Lactose.Identity.Dtos.Users;
using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp;
using LactoseWebApp.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using LoginRequest = Lactose.Identity.Dtos.Auth.LoginRequest;

namespace Lactose.Identity.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    readonly SigningCredentials _tokenSigningCredentials;
    readonly IUsersRepo _usersRepo;
    readonly UsersController _usersController;
    readonly IOptions<AuthOptions> _authOptions;
    readonly IPasswordHasher<User> _passwordHasher;
    readonly JsonWebTokenHandler _tokenHandler = new();

    public AuthController(
        ILogger<AuthController> logger,
        IUsersRepo usersRepo,
        IOptions<AuthOptions> authOptions,
        IPasswordHasher<User> passwordHasher,
        UsersController usersController)
    {
        _usersRepo = usersRepo;
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

        string jwt = CreateJwtForUser(foundUser);

        if (_authOptions.Value.UseCookie)
        {
            // As well as the standard JWT Bearer Token in the Authorization Header,
            // the service should also support JWTs being stored in Cookies too as it's
            // easier to manage on the client side.
            Response.Cookies.Append(
                AuthDefaults.JwtCookieName,
                jwt,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromDays(1),
                });
        }

        return Ok(new LoginResponse
        {
            Id = foundUser.Id,
            Email = foundUser.Email,
            Token = jwt
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
      
        string jwt = CreateJwtForUser(createdUser);

        if (_authOptions.Value.UseCookie)
        {
            Response.Cookies.Append(
                AuthDefaults.JwtCookieName,
                jwt,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromDays(1),
                });
        }
        
        return Ok(new SignupResponse
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            Token = jwt
        });
    }
    
    [HttpPost("logout", Name = "Logout")]
    public async Task<ActionResult<LogoutResponse>> Logout(LogoutRequest request)
    {
        string? jwt = HttpContext.GetJwt();
        
        if (string.IsNullOrEmpty(jwt))
            return Unauthorized();
        
        if (_authOptions.Value.UseCookie)
        {
            // Force reset the Jwt cookie on the client.
            Response.Cookies.Append(
                AuthDefaults.JwtCookieName,
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
    public async Task<ActionResult<DetailsResponse>> Details(DetailsRequest request)
    {
        string? jwt = HttpContext.GetJwt();

        if (string.IsNullOrEmpty(jwt))
            return Unauthorized();
        
        return Ok(new DetailsResponse
        {
            Id = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            DisplayName = User.FindFirstValue(JwtRegisteredClaimNames.Name),
            Email = User.FindFirstValue(JwtRegisteredClaimNames.Email),
            Token = jwt
        });
    }

    string CreateJwtForUser(User user)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new (JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new (JwtRegisteredClaimNames.Name, user.DisplayName),
                new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
            }),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(_authOptions.Value.JwtExpireHours),
            Issuer = _authOptions.Value.JwtIssuer,
            Audience = _authOptions.Value.JwtAudience,
            SigningCredentials = _tokenSigningCredentials,
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }
}