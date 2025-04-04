using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Tasks.Models;

/// <summary>
/// Represents an event that must be triggered and how to handle the event
/// in order to record task progress.
/// </summary>
public class Trigger
{
    /// <summary>
    /// The MQTT topic to subscribe to.
    /// </summary>
    public required string Topic { get; set; }
    
    /// <summary>
    /// The name of the C# Handler class to use to handle the event.
    /// </summary>
    public required string Handler { get; set; }
    
    /// <summary>
    /// The config to provide to the specified C# Handler class.
    /// </summary>
    public object? Config { get; set; }
}

public class ItemReward
{
    public required string ItemId { get; set; }
    public required int Quantity { get; set; }
}

public class Task : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public required string TaskName { get; set; }
    public string? TaskDescription { get; set; }
    public required float RequiredProgress { get; set; }
    public List<Trigger> Triggers { get; set; } = [];
    public List<ItemReward> Rewards { get; set; } = [];
}