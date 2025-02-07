using Microsoft.IdentityModel.JsonWebTokens;

namespace LactoseWebApp.Auth;

public interface IApiAuthHandler
{
    public JsonWebToken? AccessToken { get; protected set; }
    
    Task<JsonWebToken?> Authenticate();
    public void ResetAccessToken();
}