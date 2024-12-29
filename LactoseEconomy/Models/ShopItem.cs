using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Economy.Models;

public class ShopItem : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]
    public string? Id { get; set; }
    
    public required string UserId { get; set; }
    public required string ItemId { get; set; }
    
    public required string TransactionType { get; set; }
    public IList<UserItem> TransactionItems { get; set; } = new List<UserItem>();
}