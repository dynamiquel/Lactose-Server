using System.Security.Authentication;
using Lactose.Identity.Controllers;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Dtos.Apis;
using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp.Auth;
using LactoseWebApp.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Lactose.Identity.Auth;

public class NativeApiAuthHandler(
    IOptions<AuthOptions> authOptions,
    ApiController apiController,
    IUsersRepo usersRepo,
    IPasswordHasher<User> passwordHasher,
    JwtTokenHandler tokenHandler) : IApiAuthHandler
{
    public JsonWebToken? AccessToken { get; set; }

    public async Task<JsonWebToken?> Authenticate()
    {
        string? apiKey = OptionsExtensions.GetRawOrFileString(authOptions.Value.ApiKey);
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidCredentialException("No API key was provided");

        string[] splitApiKey = apiKey.Split(':', 2);
        if (splitApiKey.Length != 2)
            throw new InvalidCredentialException("Invalid API key");
        
        string apiId = splitApiKey[0];
        string apiSecret = splitApiKey[1];
        
        var apiUser = await usersRepo.GetUserByEmail(apiId);
        if (apiUser is null)
        {
            apiUser = await apiController.CreateApiUserInternal(new CreateApiRequest
            {
                ApiId = apiId,
                ApiPassword = apiSecret,
                DisplayName = apiId
            });

            if (apiUser is null)
                throw new InvalidOperationException("Could not create native API");
        }

        PasswordVerificationResult result;
        if (apiUser.PasswordHash is null)
        {
            result = PasswordVerificationResult.Success;
        }
        else
        {
            result = passwordHasher.VerifyHashedPassword(
                apiUser, 
                apiUser.PasswordHash,
                apiSecret);
        }
      
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialException("Invalid API Password");

        string accessTokenStr = tokenHandler.CreateJwtAccessTokenForUser(apiUser);
        if (string.IsNullOrEmpty(accessTokenStr))
            return null;
        
        AccessToken = new JsonWebToken(accessTokenStr);
        
        return AccessToken;
    }

    public void ResetAccessToken()
    {
        AccessToken = null;
    }
}