using Lactose.Economy.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;

namespace Lactose.Economy.Data.Repos;

public class MongoUserItemsRepo : MongoBasicKeyValueRepo<IUserItemsRepo, Models.UserItems, UserItemsDatabaseOptions>, IUserItemsRepo
{
    public MongoUserItemsRepo(ILogger<IUserItemsRepo> logger, IOptions<UserItemsDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) 
    { }
}
