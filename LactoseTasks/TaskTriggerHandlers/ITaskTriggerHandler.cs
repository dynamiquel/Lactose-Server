using Lactose.Events;
using Lactose.Tasks.Models;

namespace Lactose.Tasks.TaskTriggerHandlers;

public interface ITaskTriggerHandler
{
    string Name { get; }
    Type ConfigType { get; }
    Type EventType { get; }
    Task<float> CalculateTaskProgress(Trigger trigger, UserEvent eventPayload);
}