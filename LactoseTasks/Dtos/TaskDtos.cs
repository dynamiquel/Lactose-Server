namespace Lactose.Tasks.Dtos;

/// <summary>
/// DTO representation of an ItemReward for API contracts.
/// </summary>
public class ItemRewardDto
{
    public required string ItemId { get; set; }
    public required int Quantity { get; set; }
}

/// <summary>
/// DTO representation of a Trigger for API contracts.
/// </summary>
public class TriggerDto
{
    /// <summary>
    /// The MQTT topic to subscribe to.
    /// </summary>
    public required string Topic { get; set; }

    /// <summary>
    /// The name of the C# Handler class to use to handle the event.
    /// </summary>
    public string Handler { get; set; } = "default";

    /// <summary>
    /// The config object to provide to the specified C# Handler class.
    /// Can be any valid JSON structure.
    /// </summary>
    public object? Config { get; set; }
}
    
/// <summary>
/// Request to query the list of available Task IDs.
/// (Currently retrieves all - add filtering parameters if needed later)
/// </summary>
public class QueryTasksRequest {}

/// <summary>
/// Response containing a list of Task IDs.
/// </summary>
public class QueryTasksResponse
{
    public IList<string> TaskIds { get; set; } = new List<string>();
}

/// <summary>
/// Request to get detailed information for specific Tasks by their IDs.
/// </summary>
public class GetTasksRequest
{
    public required IList<string> TaskIds { get; init; }
}

/// <summary>
/// Response containing the detailed information for multiple Tasks.
/// </summary>
public class GetTasksResponse
{
    public IList<GetTaskResponse> Tasks { get; set; } = new List<GetTaskResponse>();
}

/// <summary>
/// Response containing the detailed information for a single Task.
/// This is also used within GetTasksResponse.
/// </summary>
public class GetTaskResponse
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required float RequiredProgress { get; set; }
    public IList<ItemRewardDto> Rewards { get; set; } = new List<ItemRewardDto>();
}

// --- DTOs for Task Create Operation ---

/// <summary>
/// Request to create a new Task definition.
/// </summary>
public class CreateTaskRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required float RequiredProgress { get; init; }
    public required List<TriggerDto> Triggers { get; init; }
    public List<ItemRewardDto>? Rewards { get; init; }
}

// --- DTOs for Task Update Operation ---

/// <summary>
/// Request to update an existing Task definition.
/// Only include fields that should be updatable.
/// Null fields typically mean "do not update this property".
/// </summary>
public class UpdateTaskRequest
{
    public required string TaskId { get; init; }
    public string? TaskName { get; init; }
    public string? TaskDescription { get; init; }
    public float? RequiredProgress { get; init; }
    public IList<TriggerDto>? Triggers { get; init; }
    public IList<ItemRewardDto>? Rewards { get; init; }
}

/// <summary>
/// Request to delete one or more Task definitions by their IDs.
/// </summary>
public class DeleteTasksRequest
{
    public required List<string>? TaskIds { get; init; } // Null = delete all
}

/// <summary>
/// Response confirming which Task IDs were successfully deleted.
/// </summary>
public class DeleteTasksResponse
{
    public List<string> DeletedTaskIds { get; set; } = [];
}
    
/// <summary>
/// Example DTO for representing a basic event related to a Task.
/// </summary>
public class TaskEvent
{
    public required string TaskId { get; init; }
}