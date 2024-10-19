using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Identity.Models;

public class Role : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required string Id { get; set; }
    public required string RoleName { get; set; }
    public ISet<string> Permissions { get; set; } = new HashSet<string>();
}