using Lactose.Economy.Models;
using Lactose.Economy.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Lactose.Economy.Data.Repos;

public class ItemsRepo : IItemsRepo
{
    readonly IMongoCollection<Item> _itemsCollection;

    public ItemsRepo(IOptions<ItemsDatabaseOptions> usersDatabaseOptions)
    {
        var mongoClient = new MongoClient(usersDatabaseOptions.Value.Connection);
        var mongoDb = mongoClient.GetDatabase(usersDatabaseOptions.Value.Database);
        _itemsCollection = mongoDb.GetCollection<Item>(usersDatabaseOptions.Value.Collection);
    }
    
    public async Task<ISet<string>> Query()
    {
        var results =
            from item in _itemsCollection.AsQueryable()
            select item.Id;

        return results.ToHashSet();
    }

    public async Task<ICollection<Item>> Get(ICollection<string> ids)
    {
        var results =
            from item in _itemsCollection.AsQueryable()
            where ids.Contains(item.Id!)
            select item;

        return results.ToList();
    }

    public async Task<Item?> Set(Item model)
    {
        if (model.Id is null)
        {
            var task = _itemsCollection.InsertOneAsync(model);
            await task;

            return task.IsCompletedSuccessfully ? model : null;
        }

        var result = await _itemsCollection.FindOneAndReplaceAsync<Item>(
            filter => filter.Id == model.Id,
            model,
            new FindOneAndReplaceOptions<Item>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            });

        return result;
    }

    public async Task<ICollection<string>> Delete(ICollection<string> ids)
    {
        var result = await _itemsCollection.DeleteManyAsync(item => ids.Contains(item.Id!));
        if (!result.IsAcknowledged)
            return new List<string>();

        if (result.DeletedCount == ids.Count)
            return ids;
        
        // Not all the desired Roles were deleted. Figure out which Roles were not deleted.
        ISet<string> existingItems = await Query();

        return ids.Where(r => !existingItems.Contains(r)).ToList();
    }

    public Task<bool> Clear()
    {
        throw new NotImplementedException();
    }
}