using Lactose.Economy.UserItems;
using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Simulation.Models;

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
    public required List<UserItem> CostItems { get; set; }
    public required double HarvestSeconds { get; set; }
    public required List<UserItem> HarvestItems { get; set; }
    public List<UserItem>? DestroyItems { get; set; }
    public string? FertiliserItemId { get; set; }
    public string? GameCrop { get; set; }
}