using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Economy.Models;

public class UserItem
{
    public required string ItemId { get; set; }
    public required int Quantity { get; set; }
}

public class UserItems : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonIgnoreIfDefault]
    public required string? Id { get; set; }
    public IList<UserItem> Items { get; set; } = new List<UserItem>();

    public UserItem IncreaseItemQuantity(string itemId, int quantity)
    {
        var existingItem = Items.SingleOrDefault(x => x.ItemId == itemId);
        if (existingItem is not null)
        {
            existingItem.Quantity += quantity;
            return existingItem;
        }

        Items.Add(new UserItem { ItemId = itemId, Quantity = quantity });
        return Items.Last();
    }
    
    public UserItem? DecreaseItemQuantity(string itemId, int quantity)
    {
        var existingItem = Items.SingleOrDefault(x => x.ItemId == itemId);
        if (existingItem is not null)
        {
            existingItem.Quantity -= quantity;

            if (existingItem.Quantity > 0)
                return existingItem;

            Items.Remove(existingItem);
        }

        return null;
    }
}