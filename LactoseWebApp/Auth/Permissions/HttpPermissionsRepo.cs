using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LactoseWebApp.Auth.Permissions;

public class HttpPermissionsRepo(
    IHttpClientFactory httpClientFactory,
    IOptions<PermissionsOptions> options) : IPermissionsRepo
{
    public async Task<List<string>> GetPermissionsForRole(CaseSensitiveClaimsIdentity identity, string roleName)
    {
        var httpClient = CreateHttpClient(identity);
        List<string> permissions = [];
            
        var response = await httpClient.PostAsJsonAsync(
            $"{options.Value.IdentityUrl}/roles",
            new { RoleIds = new[] { roleName } });

        response.EnsureSuccessStatusCode();
            
        var responseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        if (responseJson.RootElement.TryGetProperty("roles", out var rolesJson))
        {
            // Add the role as a claim under 'role-'.
            permissions.Add($"{options.Value.RoleClaimPrefix}{roleName}");
                
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

    public async Task<List<string>> GetUserRoles(CaseSensitiveClaimsIdentity identity, string userId)
    {
        var httpClient = CreateHttpClient(identity);
        List<string> roles = [];

        var response = await httpClient.PostAsJsonAsync(
            $"{options.Value.IdentityUrl}/users",
            new { UserId = userId });

        response.EnsureSuccessStatusCode();

        var responseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        if (responseJson.RootElement.TryGetProperty("roles", out var rolesJson))
        {
            foreach (var roleJson in rolesJson.EnumerateArray())
            {
                string? roleId = roleJson.GetString();
                if (!string.IsNullOrWhiteSpace(roleId))
                    roles.Add(roleId);
            }
        }
        
        return roles;
    }
    
    HttpClient CreateHttpClient(CaseSensitiveClaimsIdentity identity)
    {
        var httpClient = httpClientFactory.CreateClient();
        string accessToken = identity.SecurityToken.UnsafeToString();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return httpClient;
    }
}