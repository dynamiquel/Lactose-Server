using LactoseWebApp.Options;

namespace LactoseWebApp.Mongo;

public class MongoDatabaseOptions
{
    public required string Connection { get; set; }
    public required string Database { get; set; }
    public required string Collection { get; set; }
    public required string Username { get; set; } = "/run/secrets/lactose-mongodb-username";
    public required string Password { get; set; } = "/run/secrets/lactose-mongodb-pass";

    public string ConnectionWithBasicAuth
    {
        get
        {
            string? username = OptionsExtensions.GetRawOrFileString(Username);
            string? password = OptionsExtensions.GetRawOrFileString(Password);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return Connection;

            // Connection string needs to be URL-encoded and if passwords contain %, these
            // need to be addressed.
            password = password.Replace("%", "%25");

            string connectionWithoutProtocol = Connection.Replace("mongodb://", null);

            return $"mongodb://{username}:{password}@{connectionWithoutProtocol}";
        }
    }
}