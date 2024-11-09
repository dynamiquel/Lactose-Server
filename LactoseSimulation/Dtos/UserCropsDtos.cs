using System.Numerics;

namespace Lactose.Simulation.Dtos.UserCrops;

public class GetUserCropsRequest
{
    public required string UserId { get; set; }
}

public class GetUserCropsResponse;

public class SimulateUserCropsRequest
{
    public required string UserId { get; set; }
}

public class SimulateUserCropsResponse
{
    public DateTime PreviousSimulationTime { get; set; }
    public DateTime NewSimulationTime { get; set; }
}

public class CreateUserCropRequest
{
    public required string UserId { get; set; }
    public required string CropId { get; set; }
    public required Vector3 CropLocation { get; set; }
    public required Vector3 CropRotation { get; set; }
}

public class CreateUserCropResponse
{
    public required string UserCropInstanceId { get; set; }
}

public class HarvestUserCropsRequest
{
    public required string UserId { get; set; }
    public required IList<string> CropInstanceIds { get; set; }
}

public class HarvestUserCropsResponse
{
    public required IList<string> HarvestedCropInstanceIds { get; set; }
}

public class DestroyUserCropsRequest
{
    public required string UserId { get; set; }
    public required IList<string> CropInstanceIds { get; set; }
}

public class DestroyUserCropsResponse
{
    public required IList<string> DestroyedCropInstanceIds { get; set; }
}

public class FertiliseUserCropsRequest
{
    public required string UserId { get; set; }
    public required IList<string> CropInstanceIds { get; set; }
}

public class FertiliseUserCropsResponse
{
    public required IList<string> FertilisedCropInstanceIds { get; set; }
}
