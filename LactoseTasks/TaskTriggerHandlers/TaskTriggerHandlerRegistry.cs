using LactoseWebApp;

namespace Lactose.Tasks.TaskTriggerHandlers;

/// <summary>
/// Stores a collection of Task Trigger Handlers and maps them to a key.
/// i.e. DefaultTaskTriggerHandler => 'default'.
/// 
/// This key is used by the Task configuration to identify the trigger handler.
/// </summary>
public class TaskTriggerHandlerRegistry
{
    readonly Dictionary<string, ITaskTriggerHandler> _triggerHandlers = new();

    TaskTriggerHandlerRegistry(
        Logger<TaskTriggerHandlerRegistry> logger)
    {
        logger.LogInformation("Registering Task Trigger Handlers");
        
        var taskTriggerHandlers = CommonExtensions.GetTypesWithInterface<ITaskTriggerHandler>();
        foreach (var type in taskTriggerHandlers)
        {
            // Only Classes are supported.
            if (!type.IsClass)
                continue;
            
            string key = type.Name.Replace("TaskTriggerHandler", string.Empty).ToLowerInvariant();
            
            logger.LogInformation("Registering Task Trigger Handler '{Type}' under the key '{Key}", type.Name, key);

            var triggerHandler = Activator.CreateInstance(type) as ITaskTriggerHandler;
            if (triggerHandler is null)
            {
                logger.LogError("Failed to instantiate Task Trigger Handler '{Type}'", type.Name);
                continue;
            }
            
            _triggerHandlers.Add(key, triggerHandler);
            logger.LogInformation("Registered Task Trigger Handler '{Type}' under the key '{Key}", type.Name, key);
        }
    }

    public ITaskTriggerHandler? GetTriggerHandler(string key) => 
        _triggerHandlers.TryGetValue(key, out var handler) ? handler : null;
}