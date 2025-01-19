using LactoseWebApp.Options;

namespace LactoseWebApp.Auth.Permissions;

[Options]
public class PermissionsOptions
{
    public string IdentityUrl { get; set; } = "https://lactose.mookrata.ovh/identity";
    public int PermissionsCacheRefreshMinutes { get; set; } = 10;
    public string RoleClaimPrefix { get; set; } = "role-";
}