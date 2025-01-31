using Lactose.Identity.Auth;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Models;
using LactoseWebApp.Auth.Permissions;
using Microsoft.AspNetCore.Identity;

new IdentityApi().Start(args);

internal sealed class IdentityApi : LactoseWebApp.BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);

        builder.Services.AddSingleton<IRolesRepo, MongoRolesRepo>();
        builder.Services.AddSingleton<IUsersRepo, MongoUsersRepo>();
        builder.Services.AddSingleton<IRefreshTokensRepo, MongoRefreshTokensRepo>();
        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddSingleton<IPermissionsRepo, NativePermissionsRepo>();
    }

    protected override void OnBuilt(WebApplication app)
    {
        base.OnBuilt(app);

    }
}