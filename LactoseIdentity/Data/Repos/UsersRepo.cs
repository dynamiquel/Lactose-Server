using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;

namespace Lactose.Identity.Data.Repos;

public class MongoUsersRepo : MongoBasicKeyValueRepo<IUsersRepo, User, UsersDatabaseOptions>, IUsersRepo
{
    public MongoUsersRepo(ILogger<IUsersRepo> logger, IOptions<UsersDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) 
    { }
}