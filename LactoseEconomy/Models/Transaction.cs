using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Economy.Models;

public enum TransactionType
{
    Sell,
    Purchase
}

public class Transaction
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required string Id { get; set; }
    
    public required string UserId { get; set; }
    public required string ItemId { get; set; }
    public required TransactionType Type { get; set; }
    public required int Quantity { get; set; }
    public required DateTime Time { get; set; }
}