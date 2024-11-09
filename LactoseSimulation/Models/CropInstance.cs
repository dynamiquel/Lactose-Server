using LactoseWebApp.Types;

namespace Lactose.Simulation.Models;

public static class CropInstanceStates
{
    public readonly static string Empty = "Empty";
    public readonly static string Growing = "Growing";
    public readonly static string Harvestable = "Harvestable";

    public static bool IsValid(string cropState)
    {
        return cropState == Empty || cropState == Growing || cropState == Harvestable;
    }
}

public class CropInstance
{
    public required string Id { get; set; }
    public required string CropId { get; set; }
    public string State { get; set; } = CropInstanceStates.Empty;
    public required Vector Location { get; set; }
    public required Vector Rotation { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    public required double RemainingHarvestSeconds { get; set; }
    public double RemainingFertiliserSeconds { get; set; }
}

