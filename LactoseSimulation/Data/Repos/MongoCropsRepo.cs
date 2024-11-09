using Lactose.Simulation.Models;
using Lactose.Simulation.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;

namespace Lactose.Simulation.Data.Repos;

public class MongoCropsRepo : MongoBasicKeyValueRepo<MongoCropsRepo, Crop, CropsDatabaseOptions>, ICropsRepo
{
    public MongoCropsRepo(ILogger<MongoCropsRepo> logger, IOptions<CropsDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) { }
}