using System.Security.Authentication;
using System.Text.Json;
using Lactose.Identity.Options;
using LactoseWebApp.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace LactoseWebApp.Auth;

public class HttpApiAuthHandler(
    IHttpClientFactory httpClientFactory,
    IOptions<AuthOptions> authOptions) : IApiAuthHandler
{
    public JsonWebToken? AccessToken { get; set; }

    public void ResetAccessToken()
    {
        AccessToken = null;
    }

    public async Task<JsonWebToken?> Authenticate()
    {
        string? apiKey = OptionsExtensions.GetRawOrFileString(authOptions.Value.ApiKey);
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidCredentialException("No API key was provided");

        string[] splitApiKey = apiKey.Split(':', 2);
        if (splitApiKey.Length != 2)
            throw new InvalidCredentialException("Invalid API key");
        
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            $"{authOptions.Value.IdentityUrl}/auth/login",
            new { Email = splitApiKey[0], Password = splitApiKey[1] });
        
        response.EnsureSuccessStatusCode();
        
        var responseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var accessTokenStr = responseJson.RootElement.GetProperty("accessToken").GetString();
        if (string.IsNullOrEmpty(accessTokenStr))
            throw new InvalidOperationException("No access token was provided");

        AccessToken = new JsonWebToken(accessTokenStr);
        
        return AccessToken;
    }
}