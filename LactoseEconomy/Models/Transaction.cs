using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Economy.Models;

public class Transaction : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonIgnoreIfDefault]
    public string? Id { get; set; }
    public string? SourceUserId { get; set; }
    public string? DestinationUserId { get; set; }
    public required string ItemId { get; set; }
    public required int Quantity { get; set; }
    public required DateTime Time { get; set; }
}