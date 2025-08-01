using System.Security.Cryptography.X509Certificates;
using Lactose.Identity.Options;
using LactoseWebApp.Auth;
using LactoseWebApp.Auth.Permissions;
using LactoseWebApp.Filters;
using LactoseWebApp.Http;
using LactoseWebApp.Mqtt;
using LactoseWebApp.Service;
using LactoseWebApp.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Razor;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
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
        if (_started)
        {
            Console.Error.WriteLine("The app has already started and is being requested to start again");
            return this;
        }

        _started = true;

        try
        {
            Console.WriteLine("Initialising service...");
            OnInitialise(args);
            
            var appBuilder = WebApplication.CreateBuilder(args.ToArray());
            
            Console.WriteLine("Configuring service...");
            Configure(appBuilder);

            Console.WriteLine("Building service...");
            App = appBuilder.Build();
            
            Console.WriteLine("Post-building service...");
            OnBuilt(App);

            Console.WriteLine("Pre-running service...");
            OnPreRun(App);
            
            Console.WriteLine("Running service...");
            App.Run();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Application failed to start: {e}");
        }
        finally
        {
            Console.WriteLine("Shutting down service...");
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
        builder.Services.AddLactoseService(builder.Configuration);

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
        builder.Services
            .AddSingleton<IConfiguration>(builder.Configuration)
            .AddOptions(builder.Configuration)
            .AddHttpClientFactory()
            .AddOpenTelemetry()
            .WithMetrics(ConfigureMeters)
            .ConfigureResource(r => r.AddService(builder.Configuration.GetOptions<ServiceOptions>().ServiceName));
        
        var authOptions = builder.Configuration.TryGetOptions<AuthOptions>();
        if (authOptions is { Enabled: true })
        {
            builder.Services.AddSingleton<PermissionsService>();
            builder.Services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();
            builder.Services.AddHostedService<ApiAuthService>();

            if (authOptions.UseLocalAuth)
            {
                builder.Services.AddJwtAuthentication(authOptions);
            }
            else
            {
                builder.Services.AddSingleton<IPermissionsRepo, HttpPermissionsRepo>();
                builder.Services.AddSingleton<IApiAuthHandler, HttpApiAuthHandler>();

                var permissionOptions = builder.Configuration.GetOptions<PermissionsOptions>();
                builder.Services.AddLactoseIdentityAuthentication(authOptions, permissionOptions);
            }
            
            builder.Services.AddAuthorization();
        }
        
        builder.Services.AddMqtt();

        builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add<LogActionFilter>();
        }).AddControllersAsServices();

        builder.Services.Configure<RazorViewEngineOptions>(options =>
        {
            // Add support for VSA-based views.
            options.ViewLocationFormats.AddRange([
                "/{1}/{0}.cshtml",
                "/{1}/Views/{0}.cshtml",
                "/Features/{1}/{0}.cshtml",
                "/Features/{1}/Views/{0}.cshtml"
            ]);
        });
    }

    protected virtual void ConfigureMeters(MeterProviderBuilder builder)
    {
        builder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddPrometheusExporter();
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
        
        var authOptions = app.Configuration.TryGetOptions<AuthOptions>();
        if (authOptions is { Enabled: true })
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.MapControllers();
        app.MapPrometheusScrapingEndpoint();
    }

    /// <summary>
    /// Executed just before ASP.NET Core's web app is going to start.
    /// </summary>
    /// <param name="app"></param>
    protected virtual void OnPreRun(WebApplication app)
    {
    }
    
    /// <summary>
    /// Executed when the web app has been requested to shut down or has unexpectedly terminated.
    /// </summary>
    protected virtual void OnShutdown()
    {
        
    }
}