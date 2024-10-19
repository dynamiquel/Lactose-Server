using Lactose.Economy.Models;
using Lactose.Economy.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;

namespace Lactose.Economy.Data.Repos;

public class MongoItemsRepo : MongoBasicKeyValueRepo<IItemsRepo, Item, ItemsDatabaseOptions>, IItemsRepo
{
    public MongoItemsRepo(ILogger<IItemsRepo> logger, IOptions<ItemsDatabaseOptions> databaseOptions)
        : base(logger, databaseOptions) 
    { }
}