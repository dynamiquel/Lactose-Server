using Microsoft.IdentityModel.JsonWebTokens;

namespace LactoseWebApp.Auth;

public class ApiAuthService(
    IApiAuthHandler apiAuthHandler,
    ILogger<ApiAuthService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Authenticating API...");
            
            JsonWebToken? accessToken = await apiAuthHandler.Authenticate();
            if (accessToken is null)
            {
                logger.LogError("Failed to authenticate API. Retrying in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            else
            {
                logger.LogInformation("API has been authenticated");
                logger.LogInformation("API Access Token {Token}", accessToken.UnsafeToString());
                
                TimeSpan reauthTime = accessToken.ValidTo - DateTime.UtcNow - TimeSpan.FromMinutes(2);
                logger.LogInformation("Scheduled to reauthenticate at {Time} (in {TimeSpan} mins))", 
                    (DateTime.Now + reauthTime).ToString("g"), reauthTime.TotalMinutes.ToString("N0"));
                
                await Task.Delay(reauthTime, stoppingToken);
            }
        }
        
        logger.LogInformation("Authentication loop cancelled");
    }
}