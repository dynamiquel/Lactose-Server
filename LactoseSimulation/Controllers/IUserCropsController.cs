using Lactose.Simulation.Dtos.UserCrops;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Simulation.Controllers;

public interface IUserCropsController
{
    Task<ActionResult<GetUserCropsResponse>> GetCrops(GetUserCropsRequest request);
    Task<ActionResult<SimulateUserCropsResponse>> SimulateCrops(SimulateUserCropsRequest request);
    Task<ActionResult<CreateUserCropResponse>> CreateCrop(CreateUserCropRequest request);
    Task<ActionResult<HarvestUserCropsResponse>> HarvestCrops(HarvestUserCropsRequest request);
    Task<ActionResult<DestroyUserCropsResponse>> DestroyCrops(DestroyUserCropsRequest request);
    Task<ActionResult<FertiliseUserCropsResponse>> FertiliseCrops(FertiliseUserCropsRequest request);
    Task<ActionResult<SeedUserCropsResponse>> SeedCrop(SeedUserCropRequest request);
}