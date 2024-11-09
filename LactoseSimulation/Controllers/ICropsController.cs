using Lactose.Simulation.Dtos.Crops;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Simulation.Controllers;

public interface ICropsController
{
    Task<ActionResult<QueryCropsResponse>> QueryCrops(QueryCropsRequest request);
    Task<ActionResult<GetCropsResponse>> GetCrops(GetCropsRequest request);
    Task<ActionResult<GetCropResponse>> CreateCrop(CreateCropRequest request);
    Task<ActionResult<GetCropResponse>> UpdateCrop(UpdateCropRequest request);
    Task<ActionResult<DeleteCropsResponse>> DeleteCrops(DeleteCropsRequest request);
}