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
    public required bool Completed { get; set; }
}

public class UserTaskUpdatedEvent : UserEvent
{
    public required string TaskId { get; set; }
    public required string UserTaskId { get; set; }
    public required float PreviousProgress { get; set; }
}