using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Identity.Models;

public class User : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public required string DisplayName { get; set; }
    public ISet<string> Roles { get; set; } = new HashSet<string>();
    public DateTime TimeCreated { get; set; }
    public DateTime TimeLastLoggedIn { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
}