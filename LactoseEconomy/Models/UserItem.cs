using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Economy.Models;

public class UserItem
{
    public required string ItemId { get; set; }
    public required string Quantity { get; set; }
}

public class UserItems
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required string UserId { get; set; }
    public IList<UserItem> Items { get; set; } = new List<UserItem>();
}