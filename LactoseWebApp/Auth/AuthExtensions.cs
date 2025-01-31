using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LactoseWebApp.Auth;

public static class AuthExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user) => 
        user.FindFirstValue(JwtRegisteredClaimNames.Sub);

    public static bool MatchesId(this ClaimsPrincipal user, string userId) =>
        user.GetUserId() == userId;

    public static bool HasBoolClaim(this ClaimsPrincipal user, string claimName) => user.HasClaim(claimName, "true");
}