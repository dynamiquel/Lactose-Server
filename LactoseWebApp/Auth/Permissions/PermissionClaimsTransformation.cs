using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace LactoseWebApp.Auth.Permissions;

public class PermissionClaimsTransformation(
    PermissionsService permissionsService) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        CaseSensitiveClaimsIdentity? identity = principal.Identity as CaseSensitiveClaimsIdentity;
        if (identity == null)
            return principal;
        
        string? userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId is not null)
        {
            List<string> permissionClaims = await permissionsService.GetPermissionClaimsForUser(identity, userId);
            identity.AddClaims(permissionClaims.Select(x => new Claim(x, "true")));
        }
        
        return principal;
    }
}