using LactoseWebApp.Repo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lactose.Identity.Models;

public class RefreshToken : IBasicKeyValueModel
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string Issuer { get; set; }
    public required DateTime IssuedAt { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public string? ClientIp { get; set; }
}