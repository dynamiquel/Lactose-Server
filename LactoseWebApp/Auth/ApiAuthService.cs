namespace LactoseWebApp.Auth;

public class ApiAuthService(
    IApiAuthHandler apiAuthHandler,
    ILogger<ApiAuthService> logger) : IHostedService
{
    // How to make this execute before any of the dependent HTTP clients?
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (apiAuthHandler.AccessToken is not null)
            return;
        
        var accessToken = await apiAuthHandler.Authenticate();
        if (accessToken is null)
            throw new UnauthorizedAccessException("Could not authenticate API");

        logger.LogInformation("API has been authenticated");
        logger.LogInformation("API Access Token {0}", accessToken.UnsafeToString());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}