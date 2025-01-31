using Lactose.Identity.Data.Repos;
using Lactose.Identity.Models;
using LactoseWebApp.Auth.Permissions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lactose.Identity.Auth;

public class NativePermissionsRepo(
    IUsersRepo usersRepo,
    IRolesRepo rolesRepo,
    IOptions<PermissionsOptions> options) : IPermissionsRepo
{
    public async Task<List<string>> GetPermissionsForRole(CaseSensitiveClaimsIdentity identity, string roleName)
    {
        Role? role = await rolesRepo.Get(roleName);
        if (role is null)
            return [];
        
        var permissions = role.Permissions.ToList();
        // Add the role as a claim under 'role-'.
        permissions.Add($"{options.Value.RoleClaimPrefix}{roleName}");
        
        return permissions;
    }

    public async Task<List<string>> GetUserRoles(CaseSensitiveClaimsIdentity identity, string userId)
    {
        User? user = await usersRepo.Get(userId);
        return user is not null ? user.Roles.ToList() : [];
    }
}