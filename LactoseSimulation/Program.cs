using Lactose.Economy;
using Lactose.Simulation.Data.Repos;
using Lactose.Client;
using Lactose.Economy.Transactions;
using Lactose.Simulation.Metrics;
using LactoseWebApp.Auth;
using OpenTelemetry.Metrics;

new SimulationApi().Start(args);

internal sealed class SimulationApi : LactoseWebApp.BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);
        
        builder.Services.AddSingleton<ICropsRepo, MongoCropsRepo>();
        builder.Services.AddSingleton<IUserCropsRepo, MongoUserCropsRepo>();
       
        {
            builder.Services.Configure<TransactionsClientOptions>(builder.Configuration.GetSection("Economy"));
            var clientBuilder = builder.Services.AddHttpClient<TransactionsClient>((provider, client) =>
            {
                client.UseThisApiForAuth(provider);
            });
            
            if (builder.Environment.IsDevelopment())
                clientBuilder.DisableSslValidation();
        }
    }

    protected override void ConfigureMeters(MeterProviderBuilder builder)
    {
        base.ConfigureMeters(builder);
        builder.AddMeter(SimulationMetrics.MeterName);
    }
}