using System.Numerics;

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
    public required string State { get; set; }
    public required Vector3 Location { get; set; }
    public required Vector3 Rotation { get; set; }
    public required double RemainingHarvestSeconds { get; set; }
    public required double RemainingFertiliserSeconds { get; set; }
}

