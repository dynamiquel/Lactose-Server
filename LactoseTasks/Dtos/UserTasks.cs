//
// This file was generated by Catalyst's C# compiler at 02/05/2025 17:35:49.
// It is recommended not to modify this file. Modify the source spec file instead.
//

using System.Collections.Generic;
using Lactose.Events;

namespace Lactose.Tasks;

public record QueryUserTasksRequest
{
    public required string UserId { get; set; }

    public byte[] ToBytes()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public static QueryUserTasksRequest? FromBytes(ReadOnlySpan<byte> bytes)
    {
        return System.Text.Json.JsonSerializer.Deserialize<QueryUserTasksRequest>(bytes);
    }
}

public record QueryUserTasksResponse
{
    public required List<string> UserTaskIds { get; set; }

    public byte[] ToBytes()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public static QueryUserTasksResponse? FromBytes(ReadOnlySpan<byte> bytes)
    {
        return System.Text.Json.JsonSerializer.Deserialize<QueryUserTasksResponse>(bytes);
    }
}

public record GetUserTasksRequest
{
    public required List<string> UserTaskIds { get; set; }

    public byte[] ToBytes()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public static GetUserTasksRequest? FromBytes(ReadOnlySpan<byte> bytes)
    {
        return System.Text.Json.JsonSerializer.Deserialize<GetUserTasksRequest>(bytes);
    }
}

public record GetUserTasksFromTaskIdRequest
{
    public required string UserId { get; set; }

    public required List<string> TaskIds { get; set; }

    public byte[] ToBytes()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public static GetUserTasksFromTaskIdRequest? FromBytes(ReadOnlySpan<byte> bytes)
    {
        return System.Text.Json.JsonSerializer.Deserialize<GetUserTasksFromTaskIdRequest>(bytes);
    }
}

public record UserTaskDto
{
    public required string Id { get; set; }

    public required string UserId { get; set; }

    public required string TaskId { get; set; }

    public required double Progress { get; set; }

    public required bool Completed { get; set; }
    
    public DateTime? CompleteTime { get; set; }

    public byte[] ToBytes()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public static UserTaskDto? FromBytes(ReadOnlySpan<byte> bytes)
    {
        return System.Text.Json.JsonSerializer.Deserialize<UserTaskDto>(bytes);
    }
}

public record GetUserTasksResponse
{
    public required List<Lactose.Tasks.UserTaskDto> UserTasks { get; set; }

    public byte[] ToBytes()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public static GetUserTasksResponse? FromBytes(ReadOnlySpan<byte> bytes)
    {
        return System.Text.Json.JsonSerializer.Deserialize<GetUserTasksResponse>(bytes);
    }
}

public class UserTaskUpdatedEvent : UserEvent
{
    public required string TaskId { get; set; }
    public required string UserTaskId { get; set; }
    public required double PreviousProgress { get; set; }
}