using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Identity.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }
    public required string DisplayName { get; set; }
    public ISet<string> Roles { get; set; } = new HashSet<string>();
    public DateTime TimeCreated { get; set; }
    public DateTime TimeLastLoggedIn { get; set; }
}