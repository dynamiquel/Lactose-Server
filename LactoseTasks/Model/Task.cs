using Lactose.Economy.Models;
using Lactose.Tasks.TaskTriggerHandlers;
using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Tasks.Models;

/// <summary>
/// Don't like this type, but it's effectively a workaround to ensure
/// BSON knows how to serialise and deserialise it.
/// Maybe there's a better way.
/// </summary>
[BsonDiscriminator(Required = false)]
[BsonKnownTypes(typeof(DefaultTaskTriggerConfig))]
[BsonKnownTypes(typeof(CropTaskTriggerConfig))]
public class TaskHandlerConfig;


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
    public TaskHandlerConfig? Config { get; set; }
}

public class Task : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required double RequiredProgress { get; set; }
    public List<Trigger> Triggers { get; set; } = [];
    public List<UserItem> Rewards { get; set; } = [];
}