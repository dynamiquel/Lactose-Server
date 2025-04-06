namespace Lactose.Tasks.TaskTriggerHandlers;

public class TaskTriggerHandlerRegistry
{
    private readonly Dictionary<string, ITaskTriggerHandler> handlers = [];
    
    public TaskTriggerHandlerRegistry(
        DefaultTaskTriggerHandler defaultHandler,
        CropTaskTriggerHandler cropHandler)
    {
        handlers.Add(defaultHandler.Name, defaultHandler);
        handlers.Add(cropHandler.Name, cropHandler);
    }

    public ITaskTriggerHandler? FindHandler(string handlerName)
    {
        handlers.TryGetValue(handlerName, out ITaskTriggerHandler? handler);
        return handler;
    }
}