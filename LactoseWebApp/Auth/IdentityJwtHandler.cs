using System.Security.Claims;
using System.Text.Json;
using Lactose.Identity.Options;
using LactoseWebApp.Auth.Permissions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LactoseWebApp.Auth;

/**
 * Required to provide a remote JWT Handler for ASP NET Core JWT Authentication.
 */
public class IdentityJwtHandler : JsonWebTokenHandler
{
    public required AuthOptions AuthOptions { get; init; }
    public required PermissionsOptions PermissionsOptions { get; init; }
    
    private readonly HttpClient _httpClient = new();
    
    public override async Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
    {
        JsonWebToken? jsonWebToken;
        try
        {
            jsonWebToken = new JsonWebToken(token);
        }
        catch (Exception ex)
        {
            return new TokenValidationResult
            {
                Exception = ex,
                IsValid = false
            };
        }

        var response = await _httpClient.PostAsJsonAsync(
            $"{AuthOptions.IdentityUrl}/auth/authenticate-token",
            new { AccessToken = token, Audience = AuthOptions.JwtAudience });
        
        if (!response.IsSuccessStatusCode)
        {
            return new TokenValidationResult
            {
                IsValid = false
            };
        }

        var claimsIdentity = CreateClaimsIdentity(jsonWebToken, validationParameters);
        var responseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        if (responseJson.RootElement.TryGetProperty("userRoles", out var rolesJson))
        {
            foreach (var roleJson in rolesJson.EnumerateArray())
            {
                string? roleId = roleJson.GetString();
                if (!string.IsNullOrWhiteSpace(roleId))
                    claimsIdentity.AddClaim(new Claim($"{PermissionsOptions.RoleClaimPrefix}{roleId}", "true"));
            }
        }
        
        var validationResult = new TokenValidationResult
        {
            IsValid = true,
            SecurityToken = jsonWebToken,
            ClaimsIdentity = claimsIdentity,
        };
        
        return validationResult;
    }
}