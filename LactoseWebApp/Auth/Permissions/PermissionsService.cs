using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LactoseWebApp.Auth.Permissions;

public class PermissionsService(
    IPermissionsRepo permissionsRepo,
    IOptions<PermissionsOptions> options)
{
    Dictionary<string, List<string>> rolesToPermissionsMap = new();
    private DateTime roleToPermissionsCacheExpiry = DateTime.UtcNow;
    
    public async Task<List<string>> GetPermissionsForRole(CaseSensitiveClaimsIdentity identity, string roleName)
    {
        if (roleToPermissionsCacheExpiry < DateTime.UtcNow)
            ResetCache();
        
        if (!rolesToPermissionsMap.ContainsKey(roleName))
        {
            List<string> permissions = await permissionsRepo.GetPermissionsForRole(identity, roleName);
            rolesToPermissionsMap[roleName] = permissions;
        }
        
        return rolesToPermissionsMap[roleName];
    }
    
    public Task<List<string>> GetRolesForUser(CaseSensitiveClaimsIdentity identity, string userId)
    {
        return permissionsRepo.GetUserRoles(identity, userId);
    }

    public async Task<List<string>> GetPermissionClaimsForUser(CaseSensitiveClaimsIdentity identity, string userId)
    {
        List<string> permissionClaims = [];
        List<string> userRoles = await GetRolesForUser(identity, userId);
        
        var getPermissionsForRolesTasks = new List<Task<List<string>>>();
        getPermissionsForRolesTasks.AddRange(userRoles.Select(r => GetPermissionsForRole(identity, r)));

        Task.WaitAll(getPermissionsForRolesTasks);
        foreach (var task in getPermissionsForRolesTasks)
            permissionClaims.AddRange(task.Result);
        
        return permissionClaims;
    }

    public void ResetCache()
    {
        rolesToPermissionsMap.Clear();
        roleToPermissionsCacheExpiry = DateTime.UtcNow.AddMinutes(options.Value.PermissionsCacheRefreshMinutes);
    }
}