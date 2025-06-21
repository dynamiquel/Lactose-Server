using Lactose.Identity.Auth;
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

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSingleton<IRolesRepo, MongoRolesRepo>();
        builder.Services.AddSingleton<IUsersRepo, MongoUsersRepo>();
        builder.Services.AddSingleton<IRefreshTokensRepo, MongoRefreshTokensRepo>();
        builder.Services.AddSingleton<JwtTokenHandler>();
        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddSingleton<IPermissionsRepo, NativePermissionsRepo>();
        builder.Services.AddSingleton<IApiAuthHandler, NativeApiAuthHandler>();
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
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}