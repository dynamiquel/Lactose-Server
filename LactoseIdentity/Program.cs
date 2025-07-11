using Lactose.Identity.Auth;
using Lactose.Identity.Data;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Metrics;
using Lactose.Identity.Models;
using LactoseWebApp.Auth;
using LactoseWebApp.Auth.Permissions;
using Microsoft.AspNetCore.Identity;
using OpenTelemetry.Metrics;

new IdentityApi().Start(args);

internal sealed class IdentityApi : LactoseWebApp.BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);

        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddSingleton<IRolesRepo, MongoRolesRepo>()
            .AddSingleton<IUsersRepo, MongoUsersRepo>()
            .AddSingleton<IRefreshTokensRepo, MongoRefreshTokensRepo>()
            .AddSingleton<JwtTokenHandler>()
            .AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>()
            .AddSingleton<IPermissionsRepo, NativePermissionsRepo>()
            .AddSingleton<IApiAuthHandler, NativeApiAuthHandler>()
            .AddHostedService<MigrationService>();
    }

    protected override void ConfigureMeters(MeterProviderBuilder builder)
    {
        base.ConfigureMeters(builder);
        builder.AddMeter(IdentityMetrics.MeterName);
    }

    protected override void OnBuilt(WebApplication app)
    {
        base.OnBuilt(app);

        if (app.Environment.IsDevelopment())
        {
            app
                .UseSwagger()
                .UseSwaggerUI();
        }
    }
}