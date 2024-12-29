using Lactose.Economy.Models;
using Lactose.Economy.Options;
using LactoseWebApp;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Lactose.Economy.Data.Repos;

public class MongoShopItemsRepo : MongoBasicKeyValueRepo<IShopItemsRepo, ShopItem, ShopItemsDatabaseOptions>, IShopItemsRepo
{
    public MongoShopItemsRepo(ILogger<IShopItemsRepo> logger, IOptions<ShopItemsDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) 
    { }

    public Task<ICollection<ShopItem>> GetUserShopItems(string userId)
    {
        Logger.LogInformation($"Retrieving shop items from user with ID '{userId}'");

        var results =
            from shopItem in Collection.AsQueryable()
            where shopItem.UserId == userId
            select shopItem;
        
        var foundShopItems = results.ToList();
        
        Logger.LogInformation($"Retrieved {foundShopItems.Count} items from user shop with ID '{userId}'");

        return Task.FromResult<ICollection<ShopItem>>(foundShopItems);
    }

    public async Task<bool> DeleteUserShopItems(string userId)
    {
        Logger.LogInformation($"Deleting shop items from user with ID '{userId}'");
        
        var result = await Collection.DeleteManyAsync(item => item.UserId == userId);
        if (!result.IsAcknowledged)
        {
            Logger.LogError($"Failed to deleted shop items for user with ID '{userId}'");
            return false;
        }
        
        return true;
    }
}
