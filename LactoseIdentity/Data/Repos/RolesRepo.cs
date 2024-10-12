using Lactose.Identity.Models;
using Lactose.Identity.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Lactose.Identity.Data.Repos;

public class RolesRepo : IRolesRepo
{
    readonly IMongoCollection<Role> _roleCollection;

    public RolesRepo(IOptions<RolesDatabaseOptions> rolesDatabaseOptions)
    {
        var mongoClient = new MongoClient(rolesDatabaseOptions.Value.Connection);
        var mongoDb = mongoClient.GetDatabase(rolesDatabaseOptions.Value.Database);
        _roleCollection = mongoDb.GetCollection<Role>(rolesDatabaseOptions.Value.Collection);
    }
    
    public async Task<ISet<string>> Query()
    {
        var results =
            from role in _roleCollection.AsQueryable()
            select role.RoleId;

        return results.ToHashSet();
    }
    
    public async Task<ICollection<Role>> Get(ICollection<string> roleIds)
    {
        var results =
            from role in _roleCollection.AsQueryable()
            where roleIds.Contains(role.RoleId)
            select role;

        return results.ToList();
    }

    public async Task<Role?> Set(Role role)
    {
        var task = _roleCollection.InsertOneAsync(role);
        await task;
        return task.IsCompletedSuccessfully ? role : null;
    }

    public Task<ICollection<string>> Delete(IEnumerable<string> ids)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Clear()
    {
        throw new NotImplementedException();
    }

    public async Task<ICollection<string>> Delete(ICollection<string> roleIds)
    {
        var result = await _roleCollection.DeleteManyAsync(r => roleIds.Contains(r.RoleId));
        if (!result.IsAcknowledged)
            return new List<string>();

        if (result.DeletedCount == roleIds.Count)
            return roleIds;
        
        // Not all the desired Roles were deleted. Figure out which Roles were not deleted.
        var existingRoles = await Query();

        return roleIds.Where(r => !existingRoles.Contains(r)).ToList();
    }
}