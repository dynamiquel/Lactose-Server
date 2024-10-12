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
    
    public async Task<ISet<string>> Query()
    {
        var results =
            from role in _usersCollection.AsQueryable()
            select role.UserId;

        return results.ToHashSet();
    }

    public Task<ICollection<User>> Get(ICollection<string> ids)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> Get(string userId)
    {
        var result =
            from role in _usersCollection.AsQueryable()
            where role.UserId == userId
            select role;
        
        return result.FirstOrDefault();
    }

    public async Task<User?> Set(User user)
    {
        var task = _usersCollection.InsertOneAsync(user);
        await task;
        return task.IsCompletedSuccessfully ? user : null;
    }

    public async Task<bool> Delete(string userId)
    {
        var result = await _usersCollection.DeleteOneAsync(user => user.UserId == userId);
        return result.IsAcknowledged;
    }
    
    public Task<ICollection<string>> Delete(ICollection<string> ids)
    {
        throw new NotImplementedException();
    }
    
    public Task<bool> Clear()
    {
        throw new NotImplementedException();
    }
}