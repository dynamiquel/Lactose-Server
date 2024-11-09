using Lactose.Simulation.Data.Repos;
using Lactose.Simulation.Dtos.Crops;
using Lactose.Simulation.Mapping;
using Lactose.Simulation.Models;
using LactoseWebApp;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Simulation.Controllers;

[ApiController]
[Route("[controller]")]
public class CropsController(
    ILogger<CropsController> logger,
    ICropsRepo cropsRepo) : ControllerBase, ICropsController
{
    
    [HttpGet("query", Name = "Query Crops")]
    public async Task<ActionResult<QueryCropsResponse>> QueryCrops(QueryCropsRequest request)
    {
        ISet<string> foundCrops = await cropsRepo.Query();

        return Ok(new QueryCropsResponse
        {
            CropIds =  foundCrops.ToList()
        });
    }
    
    [HttpGet(Name = "Get Crops")]
    public async Task<ActionResult<GetCropsResponse>> GetCrops(GetCropsRequest request)
    {
        var foundCrops = await cropsRepo.Get(request.CropIds.ToHashSet());
        return Ok(CropMapper.ToDto(foundCrops));
    }

    [HttpPost(Name = "Create Crop")]
    public async Task<ActionResult<GetCropResponse>> CreateCrop(CreateCropRequest request)
    {
        if (!CropTypes.IsValid(request.Type))
            return BadRequest($"Provided an invalid Crop Type: {request.Type}");
        
        if (request.CostItems.IsEmpty())
            return BadRequest($"Expected at least one Cost Item");
        
        if (request.HarvestSeconds <= 0)
            return BadRequest($"Expected Harvest Seconds to be more than 0, but received {request.HarvestSeconds}");

        if (request.HarvestItems.IsEmpty())
            return BadRequest($"Expected at least one Harvest Item");
        
        var newCrop = new Crop
        {
            Name = request.Name,
            Type = request.Type,
            CostItems = request.CostItems,
            HarvestSeconds = request.HarvestSeconds,
            HarvestItems = request.HarvestItems,
            DestroyItems = request.DestroyItems,
            FertiliserItemId = request.FertiliserItemId
        };
        
        var createdCrop = await cropsRepo.Set(newCrop);
        if (createdCrop is null)
            return StatusCode(500, $"Could not create Crop with name '{request.Name}'");
        
        return Ok(CropMapper.ToDto(createdCrop));
    }

    [HttpPatch(Name = "Update Crop")]
    public async Task<ActionResult<GetCropResponse>> UpdateCrop(UpdateCropRequest request)
    {
        if (!request.CropId.IsValidObjectId())
            return BadRequest($"CropId '{request.CropId}' is not a valid CropId");
        
        if (request.CostItems is not null && request.CostItems.IsEmpty())
            return BadRequest($"Expected at least one Cost Item");
        
        if (request.HarvestSeconds <= 0)
            return BadRequest($"Expected Harvest Seconds to be more than 0, but received {request.HarvestSeconds}");

        if (request.HarvestItems is not null && request.HarvestItems.IsEmpty())
            return BadRequest($"Expected at least one Harvest Item");
        
        var existingCrop = await cropsRepo.Get(request.CropId);
        if (existingCrop is null)
            return BadRequest($"Crop with Id '{request.CropId}' does not exist");

        if (request.Name is not null)
            existingCrop.Name = request.Name;
        if (request.CostItems is not null)
            existingCrop.CostItems = request.CostItems;
        if (request.HarvestSeconds is not null)
            existingCrop.HarvestSeconds = request.HarvestSeconds.Value;
        if (request.HarvestItems is not null)
            existingCrop.HarvestItems = request.HarvestItems;
        
        existingCrop.DestroyItems = request.DestroyItems;
        existingCrop.FertiliserItemId = request.FertiliserItemId;

        var updatedCrop = await cropsRepo.Set(existingCrop);
        if (updatedCrop is null)
            return StatusCode(500, $"Could not update Crop with Id '{request.CropId}'");

        return Ok(CropMapper.ToDto(updatedCrop));
    }

    [HttpDelete(Name = "Delete Crop")]
    public async Task<ActionResult<DeleteCropsResponse>> DeleteCrops(DeleteCropsRequest request)
    {
        if (request.CropIds is null)
        {
            bool deletedAll = await cropsRepo.Clear();
            return deletedAll ? Ok(new DeleteCropsResponse()) : BadRequest();
        }
        
        var deletedCrops = await cropsRepo.Delete(request.CropIds);
        if (deletedCrops.IsEmpty())
            return BadRequest();

        return Ok(new DeleteCropsResponse
        {
            DeletedCropIds = deletedCrops.ToList()
        });
    }
}