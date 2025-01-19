using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace LactoseWebApp.Auth.Permissions;

public class PermissionClaimsTransformation(
    PermissionsService permissionsService) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        string? userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId is not null)
        {
            List<string> permissionClaims = await permissionsService.GetPermissionClaimsForUser(userId);
            (principal.Identity as ClaimsIdentity)?.AddClaims(permissionClaims.Select(x => new Claim(x, "true")));
        }
        
        return principal;
    }
}