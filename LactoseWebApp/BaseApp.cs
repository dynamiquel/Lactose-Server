using LactoseWebApp.Filters;
using LactoseWebApp.Service;
using LactoseWebApp.Options;
using Serilog;
using Serilog.Formatting.Display;

namespace LactoseWebApp;

/// <summary>
/// A slightly opinionated base web application that should be used in favour of
/// <see cref="Microsoft.AspNetCore.Builder.WebApplication"/>.
/// </summary>
public abstract class BaseApp
{
    protected WebApplication? App { get; private set; }
    bool _started;

    public BaseApp Start(ReadOnlySpan<string> args)
    {
        // Don't allow multiple starts.
        if (_started)
        {
            Log.Error("The app has already started and is being requested to start again");
            return this;
        }

        _started = true;

        try
        {
            OnInitialise(args);
            
            var appBuilder = WebApplication.CreateBuilder(args.ToArray() /* Copied coz .NET still using legacy shit 😞*/ );
            Configure(appBuilder);

            App = appBuilder.Build();
            OnBuilt(App);

            OnPreRun(App);
            App.Run();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Application terminated unexpectedly. Check logs for more info");
        }
        finally
        {
            OnShutdown();
        }

        return this;
    }

    /// <summary>
    /// Override to add any behaviour that should be executed when the app first starts.
    /// </summary>
    /// <param name="args">Provided app arguments</param>
    protected virtual void OnInitialise(ReadOnlySpan<string> args)
    {
        //InitialiseStaticLogger();
    }

    /// <summary>
    /// Override to configure the ASP.NET Core's web app and add any services via dependency injection.
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void Configure(WebApplicationBuilder builder)
    {
        // If using ISS, enable ISS compatability.
        if (Environment.GetEnvironmentVariable("USE_ISS")?.ToUpperInvariant() is "1" or "TRUE")
            builder.WebHost.UseIISIntegration();

        // Initialises Serilog so meaningful information can be outputted to the console and log files.
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration.Enrich.FromLogContext();
            configuration.WriteTo.Console(new MessageTemplateTextFormatter("[{Timestamp:MM-dd HH:mm:ss}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"));
            configuration.WriteTo.File(
                Path.Combine(Environment.CurrentDirectory, "Saved", "Logs", $"Log_{DateTime.UtcNow}.txt"),
                flushToDiskInterval: TimeSpan.FromSeconds(5));
        });
        
        // Lowers Kestrel's default HTTP KeepAlive timeout and allows overriding via Configuration.
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(
                int.TryParse(builder.Configuration["Kestrel:Limits:KeepAliveTimeout"], out var timeout)
                ? timeout : 30);
        });

        // Adds the Configuration as a singleton so it can be easily accessed by other services.
        // Ideally, the Options system should be used instead.
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddOptions(builder.Configuration);

        builder.Services.AddServiceInfo(builder.Configuration);
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<LogActionFilter>();
        }).AddControllersAsServices();
    }

    /// <summary>
    /// Executed when ASP.NET Core's web app has been built and is ready to start.
    /// </summary>
    /// <param name="app"></param>
    protected virtual void OnBuilt(WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseReverseProxySupport();
        app.UseHttpsRedirection();
        app.UseRouting();
        
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            //app.UseHsts();
        }

        app.MapControllers();
    }

    /// <summary>
    /// Executed just before ASP.NET Core's web app is going to start.
    /// </summary>
    /// <param name="app"></param>
    protected virtual void OnPreRun(WebApplication app)
    {
        var serviceInfo = app.Services.GetService<IServiceInfo>();
        if (serviceInfo is null)
        {
            throw new ServiceInfoNotFoundException();
        }
        
        serviceInfo.Status = OnlineStatus.Online;
    }
    
    /// <summary>
    /// Executed when the web app has been requested to shutdown or has unexpectedly terminated.
    /// </summary>
    protected virtual void OnShutdown()
    {
        Log.CloseAndFlush();
    }

    class WebAppPreRunFailedException : Exception
    {
        internal WebAppPreRunFailedException() : base("Web App has failed the PreRun stage. Check logs for more info") {}
    }
}