using Lactose.Simulation.Models;
using LactoseWebApp.Repo;

namespace Lactose.Simulation.Data.Repos;

public interface IUserCropsRepo : IBasicKeyValueRepo<UserCropInstances>
{
    Task<List<CropInstance>> GetUserCropsById(string userId, List<string> cropInstanceIds);
}