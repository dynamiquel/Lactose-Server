using Lactose.Events;
using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Tasks.Models;

public class UserTask : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public required string UserId { get; set; }
    public required string TaskId { get; set; }
    public float Progress { get; set; }
    public DateTime? CompleteTime { get; set; }

    [BsonIgnore]
    public bool Completed
    {
        get => CompleteTime is not null;
        set => CompleteTime = value ? DateTime.UtcNow : null;
    }
}

public class UserTaskUpdatedEvent : UserEvent
{
    public required string TaskId { get; set; }
    public required string UserTaskId { get; set; }
    public required float PreviousProgress { get; set; }
}