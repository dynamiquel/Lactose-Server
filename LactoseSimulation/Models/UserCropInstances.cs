using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Simulation.Models;

public class UserCropInstances : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string? Id { get; set; }
    
    public DateTime PreviousSimulationTime { get; set; }
    
    public IList<CropInstance> CropInstances { get; set; } = new List<CropInstance>();
}