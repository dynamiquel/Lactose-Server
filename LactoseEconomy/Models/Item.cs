using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Economy.Models;

public class Item
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]
    public string? Id { get; set; }
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}