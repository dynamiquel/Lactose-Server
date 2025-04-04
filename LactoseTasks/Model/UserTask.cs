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

/// <summary>
/// Represents any event that has context of a user.
/// </summary>
public class UserEvent
{
    public required string UserId { get; set; }
}