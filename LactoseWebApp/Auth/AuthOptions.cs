using LactoseWebApp.Options;

namespace Lactose.Identity.Options;

[Options]
public class AuthOptions
{
    public bool Enabled { get; set; } = false;
    public bool UseCookieForAccessToken { get; set; } = true;
    public bool UseCookieForRefreshToken { get; set; } = true;
    // Can be the token as a value or as a path.
    public string JwtTokenKey { get; set; } = "/run/secrets/lactose-jwt-key";
    public string JwtIssuer { get; set; } = "https://lactose.mookrata.ovh";
    public string JwtAudience { get; set; } = "https://lactose.mookrata.ovh";
    public int JwtExpireMinutes { get; set; } = 30;
    public int JwtRefreshExpireHours { get; set; } = 24 * 7;

}