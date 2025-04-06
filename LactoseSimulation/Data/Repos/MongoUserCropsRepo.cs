using Lactose.Simulation.Models;
using Lactose.Simulation.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Lactose.Simulation.Data.Repos;

public class MongoUserCropsRepo : MongoBasicKeyValueRepo<MongoUserCropsRepo, UserCropInstances, UserCropsDatabaseOptions>, IUserCropsRepo
{
    public MongoUserCropsRepo(ILogger<MongoUserCropsRepo> logger, IOptions<UserCropsDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) { }

    public Task<List<CropInstance>> GetUserCropsById(string userId, List<string> cropInstanceIds)
    {
        var foundCropInstances = Collection.AsQueryable()
            .Where(doc => doc.Id == userId)
            .SelectMany(doc => doc.CropInstances.Where(crop => cropInstanceIds.Contains(crop.Id)));

        return Task.FromResult(foundCropInstances.ToList());
    }
}