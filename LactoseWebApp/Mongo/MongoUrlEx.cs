using LactoseWebApp.Options;
using MongoDB.Driver;

namespace LactoseWebApp.Mongo;

/// <summary>
/// A Mongo URL with some few extras:
/// - Ability to specify a Collection by suffixing with ++CollectionName.
/// - Ability to read credentials from files by passing in a file path.
/// </summary>
public class MongoUrlEx : MongoUrl
{
    public string? CollectionName { get; private set; }
    
    public MongoUrlEx(string url) : base(CreateMongoUrlString(url))
    {
        CollectionName = CreateCollectionString(url);
    }

    static string CreateMongoUrlString(string url)
    {
        int collectionSplitIndex = GetCollectionSplitIndex(url);
        string connectionUrl = url[..collectionSplitIndex];
                
        ApplyCredentialsConversion(ref connectionUrl);
        return connectionUrl;
    }

    static string? CreateCollectionString(string url)
    {
        int collectionSplitIndex = GetCollectionSplitIndex(url);
        if (collectionSplitIndex > 0)
        {
            int startCollectionIndex = collectionSplitIndex + 2;
            return url[startCollectionIndex..];
        }

        return null;
    }
    
    static int GetCollectionSplitIndex(string connection)
    {
        int index = connection.LastIndexOf("++", StringComparison.Ordinal);
        return index < connection.Length - 1 ? index : -1;
    }

    /// <summary>
    /// Reads a raw Mongo Url and if the credentials are in a file path format, it will replace them will the file
    /// contents. Used primarily when using file-based secrets.
    /// </summary>
    static void ApplyCredentialsConversion(ref string connectionUrl)
    {
        int credentialEndIndex = connectionUrl.IndexOf('@');
        if (credentialEndIndex > 0)
        {
            string credentials = connectionUrl[10..credentialEndIndex];

            string[] credentialsSplit = credentials.Split(':');
            if (credentialsSplit.Length > 2)
                throw new ArgumentException("Invalid Mongo Url Credentials format. Received more arguments than username and password");
                    
            string? username = OptionsExtensions.GetRawOrFileString(credentialsSplit[0]);
            if (username is null)
                throw new ArgumentException("Expected a Mongo Username as part of the Mongo Url but none was provided");

            username = Uri.EscapeDataString(username);

            string convertedCredentials = username;
                    
            if (credentialsSplit.Length > 1)
            {
                string? password = OptionsExtensions.GetRawOrFileString(credentialsSplit[1]);
                if (password is null)
                    throw new ArgumentException("Expected a Mongo Password as part of the Mongo Url but none was provided");

                // Connection string needs to be URL-encoded.
                password = Uri.EscapeDataString(password);
                convertedCredentials = $"{convertedCredentials}:{password}";
            }
                    
            connectionUrl = connectionUrl.Replace(credentials, convertedCredentials);
        }
    }
}