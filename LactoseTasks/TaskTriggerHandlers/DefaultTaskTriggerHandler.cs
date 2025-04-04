using Lactose.Tasks.Models;
using Task = System.Threading.Tasks.Task;

namespace Lactose.Tasks.TaskTriggerHandlers;

public class DefaultTaskTriggerConfig
{
    public float ProgressPerOccurence { get; set; } = 1;
}

/// <summary>
/// Basic task handler that calculates task progress based on the occurrences of an event.
/// </summary>
public class DefaultTaskTriggerHandler : ITaskTriggerHandler
{
    public Type GetConfigType() => typeof(DefaultTaskTriggerConfig);

    public Task<float> CalculateTaskProgress(Trigger trigger, UserEvent eventPayload)
    {
        var config = (DefaultTaskTriggerConfig)trigger.Config!;
        return Task.FromResult(config.ProgressPerOccurence);
    }
}