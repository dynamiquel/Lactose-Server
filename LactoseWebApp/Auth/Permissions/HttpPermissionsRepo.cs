using System.Net.Http.Headers;
using System.Text.Json;
using Lactose.Identity.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LactoseWebApp.Auth.Permissions;

public class HttpPermissionsRepo(
    IHttpClientFactory httpClientFactory,
    IOptions<PermissionsOptions> permissionsOptions,
    IOptions<AuthOptions> authOptions) : IPermissionsRepo
{
    public async Task<List<string>> GetPermissionsForRole(CaseSensitiveClaimsIdentity identity, string roleName)
    {
        var httpClient = CreateHttpClient(identity);
        List<string> permissions = [];
            
        var response = await httpClient.PostAsJsonAsync(
            $"{authOptions.Value.IdentityUrl}/roles",
            new { RoleIds = new[] { roleName } });

        response.EnsureSuccessStatusCode();
            
        var responseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        if (responseJson.RootElement.TryGetProperty("roles", out var rolesJson))
        {
            // Add the role as a claim under 'role-'.
            permissions.Add($"{permissionsOptions.Value.RoleClaimPrefix}{roleName}");
                
            if (rolesJson[0].TryGetProperty("permissions", out var permissionsJson))
            {
                foreach (var permissionJson in permissionsJson.EnumerateArray())
                {
                    string? permissionId = permissionJson.GetString();
                    if (!string.IsNullOrWhiteSpace(permissionId))
                    {
                        permissions.Add(permissionId);
                    }
                }
            }
        }
        
        return permissions;
    }

    public Task<List<string>> GetUserRoles(CaseSensitiveClaimsIdentity identity, string userId)
    {
        // The user claims identity will already have the role claims from a prior step
        // (IdentityJwtHandler).
        
        // Role claims are prefixed with RoleClaimPrefix.
        // Find any of these claims and strip out the prefix to find the actual role name.
        List<string> roles = identity.Claims
            .Where(c => c.Type.StartsWith(permissionsOptions.Value.RoleClaimPrefix))
            .Select(c => c.Type.Replace(permissionsOptions.Value.RoleClaimPrefix, string.Empty))
            .ToList();
        
        return Task.FromResult(roles);
    }
    
    HttpClient CreateHttpClient(CaseSensitiveClaimsIdentity identity)
    {
        var httpClient = httpClientFactory.CreateClient();
        string accessToken = identity.SecurityToken.UnsafeToString();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return httpClient;
    }
}