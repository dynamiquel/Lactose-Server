using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Simulation.Models;

public class CropItem
{
    public required string ItemId { get; set; }
    public int Quantity { get; set; } = 1;
}

public static class CropTypes
{
    public readonly static string Plot = "Plot";
    public readonly static string Tree = "Tree";
    public readonly static string Animal = "Animal";

    public static bool IsValid(string cropType)
    {
        return cropType == Plot || cropType == Tree || cropType == Animal;
    }
}

public class Crop : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonIgnoreIfDefault]
    public string? Id { get; set; }
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required IList<CropItem> CostItems { get; set; }
    public required double HarvestSeconds { get; set; }
    public required IList<CropItem> HarvestItems { get; set; }
    public IList<CropItem>? DestroyItems { get; set; }
    public string? FertiliserItemId { get; set; }
}