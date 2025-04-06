using Lactose.Economy.Models;

namespace LactoseTasks.Simulation;

/// <summary>
/// This is basically copied over from Simulation.
/// Need to figure out a better way to deal with this.
///
/// Economy uses a 'shared' and 'client' library as an alternative,
/// but I don't like that either.
/// </summary>

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

public class CropEvent
{
    public required string CropId { get; init; }
}