namespace LactoseWebApp.Service;

public class ServiceHealthChecker(
    ILogger<ServiceHealthChecker> logger,
    IServiceInfo serviceInfo) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Service '{ServiceName}' is online", serviceInfo.Name);
        logger.LogInformation("Service binaries were last built on: {Date}", serviceInfo.BuildTime.ToString("yyyy-MM-dd HH:mm"));
        
        serviceInfo.Status = OnlineStatus.Online;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Service '{ServiceName}' is shutting down", serviceInfo.Name);

        serviceInfo.Status = OnlineStatus.Ending;
        return Task.CompletedTask;
    }
}