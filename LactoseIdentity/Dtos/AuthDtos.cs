namespace Lactose.Identity.Dtos.Auth;

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
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string DisplayName { get; set; }
}

public class SignupResponse : BasicClaims;

public class DetailsRequest;

public class DetailsResponse : BasicClaims;

public class BasicClaims
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
}