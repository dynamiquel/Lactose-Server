namespace Lactose.Tasks.Repos;

public class QueryUserTasksRequest
{
    public required string UserId { get; set; }
}

public class QueryUserTasksResponse
{
    public required List<string> UserTaskIds { get; set; } = [];
}

public class GetUserTasksRequest
{
    public required List<string> UserTaskIds { get; set; }
}

public class GetUserTasksFromTaskIdRequest
{
    public required string UserId { get; set; }
    public required List<string> TaskIds { get; set; }
}

public class UserTaskDto
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string TaskId { get; set; }
    public required float Progress { get; set; }
    public required bool Completed { get; set; }
}

public class GetUserTasksResponse
{
   public required List<UserTaskDto> UserTasks { get; set; } = [];
}
