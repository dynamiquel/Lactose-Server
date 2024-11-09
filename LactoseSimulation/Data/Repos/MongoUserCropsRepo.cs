using Lactose.Simulation.Models;
using LactoseSimulation.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;

namespace Lactose.Simulation.Data.Repos;

public class MongoUserCropsRepo : MongoBasicKeyValueRepo<MongoUserCropsRepo, UserCropInstances, UserCropsDatabaseOptions>, IUserCropsRepo
{
    public MongoUserCropsRepo(ILogger<MongoUserCropsRepo> logger, IOptions<UserCropsDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) { }
}