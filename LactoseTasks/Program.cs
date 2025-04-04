using Lactose.Economy;
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
        
        builder.Services.AddSingleton<MongoTasksRepo>();
        builder.Services.AddSingleton<MongoUserTasksRepo>();
        builder.Services.AddHostedService<UserTaskTracker>();
        builder.Services.AddSingleton<DefaultTaskTriggerHandler>();
       
        {
            builder.Services.Configure<EconomyClientOptions>(builder.Configuration.GetSection("Economy"));
            var clientBuilder = builder.Services.AddHttpClient<TransactionsClient>((provider, client) =>
            {
                client.UseThisApiForAuth(provider);
            });
            
            if (builder.Environment.IsDevelopment())
                clientBuilder.DisableSslValidation();
        }
    }
}