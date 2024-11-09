using Lactose.Simulation.Data.Repos;

new SimulationApi().Start(args);

internal sealed class SimulationApi : LactoseWebApp.BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);
        builder.Services.AddSingleton<ICropsRepo, MongoCropsRepo>();
        builder.Services.AddSingleton<IUserCropsRepo, MongoUserCropsRepo>();
    }
}