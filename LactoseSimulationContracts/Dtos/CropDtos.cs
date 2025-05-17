using Lactose.Economy.UserItems;

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
    public required string Type { get; init; }
    public required string Name { get; init; }
    public required List<UserItem> CostItems { get; init; }
    public required double HarvestSeconds { get; init; }
    public required List<UserItem> HarvestItems { get; init; }
    public List<UserItem>? DestroyItems { get; init; }
    public string? FertiliserItemId { get; init; }
    public string? GameCrop { get; init; }
}

public class UpdateCropRequest
{
    public required string CropId { get; init; }
    public string? Name { get; init; }
    public List<UserItem>? CostItems { get; init; }
    public double? HarvestSeconds { get; init; }
    public List<UserItem>? HarvestItems { get; init; }
    public List<UserItem>? DestroyItems { get; init; }
    public string? FertiliserItemId { get; init; }
    public string? GameCrop { get; init; }
}

public class DeleteCropsRequest
{
    public List<string>? CropIds { get; init; }
}

public class DeleteCropsResponse
{
    public List<string> DeletedCropIds { get; set; } = [];
}

public class CropEvent
{
    public required string CropId { get; init; }
}