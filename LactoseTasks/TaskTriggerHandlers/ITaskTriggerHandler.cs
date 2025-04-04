using Lactose.Tasks.Models;

namespace Lactose.Tasks.TaskTriggerHandlers;

public interface ITaskTriggerHandler
{
    Type GetConfigType();
    Task<float> CalculateTaskProgress(Trigger trigger, UserEvent eventPayload);
}