using Lactose.Identity.Services;

new IdentityApi().Start(args);

internal sealed class IdentityApi : LactoseWebApp.BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);

        /*var profanityOptions = builder.Configuration.TryGetOptions<ProfanityOptions>();
        if (profanityOptions?.Enabled == true)
        {
            builder.Services.AddSingleton<IGuildConfigRepo<ProfanityGuildConfigModel>, ProfanityGuildConfigRepo>();
            builder.Services.AddHostedService<ProfanityService>();
        }*/
    }

    protected override void OnBuilt(WebApplication app)
    {
        base.OnBuilt(app);

        app.MapGrpcService<RolesService>();
        app.MapGrpcService<UsersService>();
    }
}