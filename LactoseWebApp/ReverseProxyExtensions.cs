using Serilog;

namespace LactoseWebApp;

public static class ReverseProxyDefaults
{
    public static string PathBase => "Kestrel:PathBase";
}

public static class ReverseProxyExtensions
{
    /// <summary>
    /// Use Reverse Proxy support if the <see cref="IConfiguration"/> has specified a Path Base.
    /// </summary>
    public static IApplicationBuilder UseReverseProxySupport(this WebApplication app)
    {
        string? pathBase = app.Configuration[ReverseProxyDefaults.PathBase];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            app.Use((httpContext, next) =>
            {
                httpContext.Request.PathBase = pathBase;
                return next();
            });
            
            Log.Information("Using Path Base: {PathBase}", pathBase);
        }

        return app;
    }
}