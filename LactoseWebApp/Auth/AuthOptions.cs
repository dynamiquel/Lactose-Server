using LactoseWebApp.Options;

namespace Lactose.Identity.Options;

[Options]
public class AuthOptions
{
    public bool Enabled { get; set; } = false;
    public bool UseCookie { get; set; } = true;
    public string JwtTokenKey { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = "https://lactose.mookrata.ovh";
    public string JwtAudience { get; set; } = "https://lactose.mookrata.ovh";
    public int JwtExpireHours { get; set; } = 24;
}