using Lactose.Simulation.Models;
using LactoseWebApp.Types;

namespace Lactose.Simulation.Dtos.UserCrops;

public class UserRequest
{
    public required string UserId { get; init; }
}

public class GetUserCropsRequest : UserRequest;

public class GetUserCropsResponse
{
    public DateTime PreviousSimulationTime { get; set; }
    public IList<CropInstance> CropInstances { get; set; } = new List<CropInstance>();
}

public class SimulateUserCropsRequest : UserRequest;

public class SimulateUserCropsResponse
{
    public DateTime PreviousSimulationTime { get; set; }
    public DateTime NewSimulationTime { get; set; }
}

public class CreateUserCropRequest : UserRequest
{
    public required string CropId { get; init; }
    public required Vector CropLocation { get; init; }
    public required Vector CropRotation { get; init; }
}

public class CreateUserCropResponse
{
    public required string UserCropInstanceId { get; set; }
}

public class HarvestUserCropsRequest : UserRequest
{
    public required IList<string> CropInstanceIds { get; init; }
}

public class HarvestUserCropsResponse
{
    public required IList<string> HarvestedCropInstanceIds { get; set; }
}

public class DestroyUserCropsRequest : UserRequest
{
    public required IList<string> CropInstanceIds { get; init; }
}

public class DestroyUserCropsResponse
{
    public required IList<string> DestroyedCropInstanceIds { get; set; }
}

public class FertiliseUserCropsRequest : UserRequest
{
    public required IList<string> CropInstanceIds { get; init; }
}

public class FertiliseUserCropsResponse
{
    public required IList<string> FertilisedCropInstanceIds { get; set; }
}

public class SeedUserCropRequest : UserRequest
{
    public required IList<string> CropInstanceIds { get; init; }
    public required string CropId { get; init; }
}

public class SeedUserCropsResponse
{
    public required IList<string> SeededCropInstanceIds { get; set; }
}

public class UserCropInstancesEvent
{
    public required string UserId { get; init; }
    public required IList<string> CropInstanceIds { get; init; }
}