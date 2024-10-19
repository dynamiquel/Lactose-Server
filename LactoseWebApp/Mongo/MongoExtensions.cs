namespace LactoseWebApp.Mongo;

public static class MongoExtensions
{
    public static bool IsValidObjectId(this string objectId)
    {
        return MongoDB.Bson.ObjectId.TryParse(objectId, out _);
    }
}