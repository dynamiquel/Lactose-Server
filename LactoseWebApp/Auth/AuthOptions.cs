using LactoseWebApp.Options;

namespace Lactose.Identity.Options;

[Options]
public class AuthOptions
{
    public bool Enabled { get; set; } = false;
    public bool UseLocalAuth { get; set; } = false;
    public string IdentityUrl { get; set; } = "https://lactose.mookrata.ovh/identity";
    public bool UseCookieForAccessToken { get; set; } = true;
    public bool UseCookieForRefreshToken { get; set; } = true;
    // Can be the token as a value or as a path.
    public string JwtTokenKey { get; set; } = "/run/secrets/lactose-jwt-key";
    public string JwtIssuer { get; set; } = "https://lactose.mookrata.ovh";
    public string JwtAudience { get; set; } = "https://lactose.mookrata.ovh";
    public int JwtExpireMinutes { get; set; } = 10;
    public int JwtRefreshExpireHours { get; set; } = 24 * 7;
    public string ApiKey { get; set; } = "/run/secrets/lactose-api-key";
}