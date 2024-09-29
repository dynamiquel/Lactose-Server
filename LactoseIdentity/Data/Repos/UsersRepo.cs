using Lactose.Identity.Models;
using Lactose.Identity.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Lactose.Identity.Data.Repos;

public class UsersRepo : IUsersRepo
{
    readonly IMongoCollection<User> _usersCollection;

    public UsersRepo(IOptions<UsersDatabaseOptions> usersDatabaseOptions)
    {
        var mongoClient = new MongoClient(usersDatabaseOptions.Value.Connection);
        var mongoDb = mongoClient.GetDatabase(usersDatabaseOptions.Value.Database);
        _usersCollection = mongoDb.GetCollection<User>(usersDatabaseOptions.Value.Collection);
    }
    
    public async Task<ISet<string>> QueryUsers()
    {
        var results =
            from role in _usersCollection.AsQueryable()
            select role.UserId;

        return results.ToHashSet();
    }

    public async Task<User?> GetUserById(string userId)
    {
        var result =
            from role in _usersCollection.AsQueryable()
            where role.UserId == userId
            select role;
        
        return result.FirstOrDefault();
    }

    public async Task<User?> CreateUser(User user)
    {
        var task = _usersCollection.InsertOneAsync(user);
        await task;
        return task.IsCompletedSuccessfully ? user : null;
    }

    public async Task<bool> DeleteUserById(string userId)
    {
        var result = await _usersCollection.DeleteOneAsync(user => user.UserId == userId);
        return result.IsAcknowledged;
    }
}