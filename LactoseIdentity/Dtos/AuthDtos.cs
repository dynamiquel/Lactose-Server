using System.ComponentModel.DataAnnotations;

namespace Lactose.Identity.Dtos.Auth;

public class BasicClaims
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public List<string>? Roles { get; set; }
    public List<string>? Permissions { get; set; }
    public string? TokenExpires { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class LoginResponse : BasicClaims;

public class LogoutRequest;

public class LogoutResponse;

public class SignupRequest
{
    [EmailAddress]
    public required string Email { get; init; }
    
    [MinLength(8)]
    public required string Password { get; init; }
    
    [MinLength(8)]
    public required string DisplayName { get; init; }
}

public class SignupResponse : BasicClaims;

public class DetailsRequest;

public class DetailsResponse : BasicClaims;

public class RefreshRequest
{
    // Optional. Will try to find cookie with refresh token if not set.
    public string? RefreshToken { get; set; }
}

public class AuthenticateTokenRequest
{
    public required string AccessToken { get; init; }
    public string? Audience { get; init; }
}

public class AuthenticateTokenResponse
{
    public required ISet<string> UserRoles { get; set; }
}

public class RefreshResponse : BasicClaims;

public class UserLoggedInEvent
{
    public required string UserId { get; init; }
    public DateTime TimeLastLoggedIn { get; init; }
}
