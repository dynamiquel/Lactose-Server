using Lactose.Economy.Data.Repos;

new EconomyApi().Start(args);

internal sealed class EconomyApi : LactoseWebApp.BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);
        builder.Services.AddSingleton<IItemsRepo, MongoItemsRepo>();
        builder.Services.AddSingleton<IUserItemsRepo, MongoUserItemsRepo>();
        builder.Services.AddSingleton<ITransactionsRepo, MongoTransactionsRepo>();
    }
}