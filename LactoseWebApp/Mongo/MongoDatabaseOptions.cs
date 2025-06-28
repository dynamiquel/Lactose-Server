using System.Text.Json.Serialization;

namespace LactoseWebApp.Mongo;

public class MongoDatabaseOptions
{
    /// <summary>
    /// The Mongo URL to connect to.
    /// It adheres to the standard Mongo URL expression but also supports an optional collection name, as well
    /// as providing usernames and passwords as files.
    /// 
    /// Expected format: <code>mongodb://username:password@host:port/database?optionalArgs++optionalCollection</code>
    /// </summary>
    public required string Connection { get; set; }

    [JsonIgnore]
    public MongoUrlEx ConnectionUrl
    {
        get
        {
            if (_connectionUrl is null)
            {
                if (string.IsNullOrEmpty(Connection))
                    throw new NullReferenceException($"{GetType()} has no {nameof(Connection)} set");

                _connectionUrl = new MongoUrlEx(Connection);
            }

            return _connectionUrl;
        }
    }
    
    MongoUrlEx? _connectionUrl;
}