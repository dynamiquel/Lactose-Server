using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Simulation.Models;

public class UserCropInstances : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    [BsonIgnoreIfDefault]
    public string? Id { get; set; }
    public IDictionary<string, CropInstance> CropInstances { get; set; } = new Dictionary<string, CropInstance>();
}