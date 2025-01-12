using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Lactose.Identity.Data.Repos;

public class MongoUsersRepo : MongoBasicKeyValueRepo<IUsersRepo, User, UsersDatabaseOptions>, IUsersRepo
{
    public MongoUsersRepo(ILogger<IUsersRepo> logger, IOptions<UsersDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) 
    { }

    public Task<User?> GetUserByEmail(string email)
    {
        Logger.LogInformation($"Finding user by email: {email}");

        var results =
            from item in Collection.AsQueryable()
            where item.Email == email
            select item;
        
        return Task.FromResult(results.FirstOrDefault());
    }
}