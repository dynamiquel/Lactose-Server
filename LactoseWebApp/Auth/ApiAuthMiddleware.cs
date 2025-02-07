namespace LactoseWebApp.Auth;

public class ApiAuthMiddleware(
    RequestDelegate next,
    IApiAuthHandler apiAuthHandler,
    ILogger<ApiAuthMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var accessToken = await apiAuthHandler.Authenticate();
        if (accessToken is null)
            throw new UnauthorizedAccessException("Could not authenticate API");

        logger.LogInformation("API has been authenticated");
        logger.LogInformation("API Access Token {0}", accessToken.UnsafeToString());
        
        await next(context);
    }
}