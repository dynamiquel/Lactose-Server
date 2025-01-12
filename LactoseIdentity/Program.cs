using Lactose.Identity.Data.Repos;
using Lactose.Identity.Models;
using Microsoft.AspNetCore.Identity;

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
        
        builder.Services.AddSingleton<IRolesRepo, MongoRolesRepo>();
        builder.Services.AddSingleton<IUsersRepo, MongoUsersRepo>();
        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
    }

    protected override void OnBuilt(WebApplication app)
    {
        base.OnBuilt(app);

    }
}