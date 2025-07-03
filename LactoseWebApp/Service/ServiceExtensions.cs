using LactoseWebApp.Options;

namespace LactoseWebApp.Service;

public static class ServiceExtensions
{
    /// <summary>
    /// Retrieves the Lactose service info from the configuration file and adds it as a singleton service.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddLactoseService(this IServiceCollection services, IConfiguration config)
    {
        var serviceOptions = config.GetOptions<ServiceOptions>();

        ServiceInfo serviceInfo = new()
        {
            Name = serviceOptions.ServiceName,
            Description = serviceOptions.Description,
            Dependencies = serviceOptions.Dependencies,
            Version = !string.IsNullOrWhiteSpace(serviceOptions.Version)
                ? Version.Parse(serviceOptions.Version)
                : new Version(0, 1),
            Status = OnlineStatus.Starting
        };
        
        Console.WriteLine($"Initialising Lactose Service {serviceOptions.ServiceName} (v. {serviceInfo.Version} b. {serviceInfo.BuildTime})...");

        services.AddSingleton<IServiceInfo, ServiceInfo>(_ => new ServiceInfo
        {
            Name = serviceOptions.ServiceName,
            Description = serviceOptions.Description,
            Dependencies = serviceOptions.Dependencies,
            Version = !string.IsNullOrWhiteSpace(serviceOptions.Version) ? Version.Parse(serviceOptions.Version) : new Version(0, 1),
            Status = OnlineStatus.Starting
        });

        services.AddHostedService<ServiceHealthChecker>();

        return services;
    }
}
