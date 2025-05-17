using Lactose.Economy;
using Lactose.Economy.Transactions;
using Lactose.Tasks.Data;
using Lactose.Tasks.TaskTriggerHandlers;
using LactoseTasks.Services;
using LactoseWebApp;
using LactoseWebApp.Auth;

new TasksApi().Start(args);

internal sealed class TasksApi : BaseApp
{
    protected override void Configure(WebApplicationBuilder builder)
    {
        base.Configure(builder);

        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddSingleton<MongoTasksRepo>()
            .AddSingleton<MongoUserTasksRepo>()
            .AddHostedService<UserTaskTracker>()
            .AddSingleton<TaskTriggerHandlerRegistry>()
            .AddSingleton<DefaultTaskTriggerHandler>()
            .AddSingleton<CropTaskTriggerHandler>();

        {
            builder.Services
                .Configure<TransactionsClientOptions>(builder.Configuration.GetSection("Economy"))
                .Configure<SimulationClientOptions>(builder.Configuration.GetSection("Simulation"));

            var transactionsClientBuilder = builder.Services.AddHttpClient<TransactionsClient>((provider, client) =>
            {
                client.UseThisApiForAuth(provider);
            });
            
            var cropsClientBuilder = builder.Services.AddHttpClient<CropsClient>((provider, client) =>
            {
                client.UseThisApiForAuth(provider);
            });
            
            var userCropsClientBuilder = builder.Services.AddHttpClient<UserCropsClient>((provider, client) =>
            {
                client.UseThisApiForAuth(provider);
            });

            if (builder.Environment.IsDevelopment())
            {
                transactionsClientBuilder.DisableSslValidation();
                cropsClientBuilder.DisableSslValidation();
                userCropsClientBuilder.DisableSslValidation();
            }
        }
    }
}