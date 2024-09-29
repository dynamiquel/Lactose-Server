using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Identity.Models;

public class Role
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required string RoleId { get; set; }
    public required string RoleName { get; set; }
    public ISet<string> Permissions { get; set; } = new HashSet<string>();
}