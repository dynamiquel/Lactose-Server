using Microsoft.IdentityModel.Tokens;

namespace LactoseWebApp.Auth.Permissions;

public interface IPermissionsRepo
{
    Task<List<string>> GetPermissionsForRole(CaseSensitiveClaimsIdentity identity, string roleName);
    Task<List<string>> GetUserRoles(CaseSensitiveClaimsIdentity identity, string userId);
}