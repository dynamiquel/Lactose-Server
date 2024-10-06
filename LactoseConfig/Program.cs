using Lactose.Config.Data.Repositories;

new ConfigApi().Start(args);

internal sealed class ConfigApi : LactoseWebApp.BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);
        builder.Services.AddSingleton<IConfigRepo, ConfigRepo>();
    }
}