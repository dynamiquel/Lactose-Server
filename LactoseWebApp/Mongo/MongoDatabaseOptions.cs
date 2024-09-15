namespace LactoseWebApp.Mongo;

public class MongoDatabaseOptions
{
    public required string Connection { get; set; }
    public required string Database { get; set; }
    public required string Collection { get; set; }
}