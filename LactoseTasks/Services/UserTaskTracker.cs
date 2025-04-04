using Lactose.Tasks.Data;
using Lactose.Tasks.TaskTriggerHandlers;
using MQTTnet;

namespace LactoseTasks.Services;

public class UserTaskTracker(
    ILogger<UserTaskTracker> logger,
    IMqttClient mqttClient,
    MongoTasksRepo tasksRepo,
    MongoUserTasksRepo userTasksRepo) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Go through every Task and subscribe to the necessary events based on its triggers.
        // Cache the tasks so when an event is received, it knows exactly which tasks to iterate over.
        // Or just do a batch mongo query request based on any topics? probably better.
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}