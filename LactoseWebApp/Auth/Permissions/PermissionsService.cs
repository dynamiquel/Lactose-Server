using System.Text.Json;
using Microsoft.Extensions.Options;

namespace LactoseWebApp.Auth.Permissions;

public class PermissionsService(
    IHttpClientFactory httpClientFactory,
    IOptions<PermissionsOptions> options)
{
    Dictionary<string, List<string>> rolesToPermissionsMap = new();
    private DateTime roleToPermissionsCacheExpiry = DateTime.UtcNow;
    
    public async Task<List<string>> GetPermissionsForRole(string roleName, HttpClient httpClient)
    {
        if (roleToPermissionsCacheExpiry < DateTime.UtcNow)
            ResetCache();
        
        if (!rolesToPermissionsMap.ContainsKey(roleName))
        {
            List<string> permissions = new List<string>();
            
            var response = await httpClient.PostAsJsonAsync(
                $"{options.Value.IdentityUrl}/roles",
                new { RoleIds = new[] { roleName } });
            
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

            rolesToPermissionsMap[roleName] = permissions;
        }
        
        return rolesToPermissionsMap[roleName];
    }
    
    public async Task<List<string>> GetRolesForUser(string userId, HttpClient httpClient)
    {
        List<string> roles = new List<string>();
        
        var response = await httpClient.PostAsJsonAsync(
            $"{options.Value.IdentityUrl}/users",
            new { UserId = userId });
        
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

    public async Task<List<string>> GetPermissionClaimsForUser(string userId)
    {
        var httpClient = httpClientFactory.CreateClient();
        
        List<string> permissionClaims = new List<string>();
        List<string> userRoles = await GetRolesForUser(userId, httpClient);
        
        var getPermissionsForRolesTasks = new List<Task<List<string>>>();
        getPermissionsForRolesTasks.AddRange(userRoles.Select(r => GetPermissionsForRole(r, httpClient)));

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