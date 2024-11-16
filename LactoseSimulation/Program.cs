using Lactose.Economy;
using Lactose.Simulation.Data.Repos;
using LactoseClient;

new SimulationApi().Start(args);

internal sealed class SimulationApi : LactoseWebApp.BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);
        builder.Services.AddSingleton<ICropsRepo, MongoCropsRepo>();
        builder.Services.AddSingleton<IUserCropsRepo, MongoUserCropsRepo>();
        builder.Services.Configure<EconomyClientOptions>(builder.Configuration.GetSection("Economy"));

        {
            var clientBuilder = builder.Services.AddHttpClient<TransactionsClient>();
            if (builder.Environment.IsDevelopment())
                clientBuilder.DisableSslValidation();
        }
    }
}