using Lactose.Economy.Models;

namespace Lactose.Simulation.Dtos.Crops;

public class QueryCropsRequest;

public class QueryCropsResponse
{
    public IList<string> CropIds { get; set; } = new List<string>();
}

public class GetCropsRequest
{
    public required IList<string> CropIds { get; init; }
}

public class GetCropResponse
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required IList<UserItem> CostItems { get; set; }
    public required double HarvestSeconds { get; set; }
    public required IList<UserItem> HarvestItems { get; set; }
    public IList<UserItem>? DestroyItems { get; set; }
    public string? FertiliserItemId { get; set; }
    public string? GameCrop { get; set; }
}

public class GetCropsResponse
{
    public IList<GetCropResponse> Crops { get; set; } = new List<GetCropResponse>();
}

public class CreateCropRequest
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required IList<UserItem> CostItems { get; set; }
    public required double HarvestSeconds { get; set; }
    public required IList<UserItem> HarvestItems { get; set; }
    public IList<UserItem>? DestroyItems { get; set; }
    public string? FertiliserItemId { get; set; }
    public string? GameCrop { get; set; }
}

public class UpdateCropRequest
{
    public required string CropId { get; init; }
    public string? Name { get; set; }
    public IList<UserItem>? CostItems { get; set; }
    public double? HarvestSeconds { get; set; }
    public IList<UserItem>? HarvestItems { get; set; }
    public IList<UserItem>? DestroyItems { get; set; }
    public string? FertiliserItemId { get; set; }
    public string? GameCrop { get; set; }
}

public class DeleteCropsRequest
{
    public IList<string>? CropIds { get; init; }
}

public class DeleteCropsResponse
{
    public IList<string> DeletedCropIds { get; set; } = new List<string>();
}