using System.Diagnostics.Metrics;

namespace Lactose.Identity.Metrics;

public static class IdentityMetrics
{
    public const string MeterName = "Lactose.Identity";
    
    static readonly Meter Meter = new(MeterName, "1.0.0");
    
    public static Counter<long> SignupsCounter { get; } = 
        Meter.CreateCounter<long>("identity.signups.total", description: "Total number of successful user signups.");
    
    public static Counter<long> LoginsCounter { get; } = 
        Meter.CreateCounter<long>("identity.logins.total", description: "Total number of successful user logins.");
    
    public static Counter<long> LogoutsCounter { get; } = 
        Meter.CreateCounter<long>("identity.logouts.total", description: "Total number of successful user logouts.");
    
    public static Counter<long> RefreshTokensCounter { get; } = 
        Meter.CreateCounter<long>("identity.refresh_tokens.total", description: "Total number of refresh token requests.");
}