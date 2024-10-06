using Lactose.Config.Dtos;
using Lactose.Config.Models;
using Lactose.Config.Options;
using LactoseWebApp;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Lactose.Config.Data.Repositories;

public class ConfigRepo : IConfigRepo
{
    readonly IMongoCollection<ConfigEntry> _configCollection;
    
    public ConfigRepo(IOptions<ConfigDatabaseOptions> configCloudDatabaseConfig)
    {
        var mongoClient = new MongoClient(configCloudDatabaseConfig.Value.Connection);
        var mongoDb = mongoClient.GetDatabase(configCloudDatabaseConfig.Value.Database);
        _configCollection = mongoDb.GetCollection<ConfigEntry>(configCloudDatabaseConfig.Value.Collection);
    }

    public async Task<ConfigEntry?> GetEntry(ConfigEntryRequest entryRequest)
    {
        var results = await _configCollection.FindAsync(
            e => e.Key == entryRequest.Key);

        var bestConfigEntry = GetEntryWithBestConditions(entryRequest.Conditions, await results.ToListAsync());

        return bestConfigEntry;
    }

    public async Task<ConfigEntry?> GetEntryById(string entryId)
    {
        var results = await _configCollection.FindAsync(
            e => e.Id == entryId);
        
        return await results.SingleOrDefaultAsync();
    }

    public async Task<ICollection<ConfigEntry>> GetEntries(IEnumerable<ConfigEntryRequest> entryRequest)
    {
        var results = await _configCollection.FindAsync(s => entryRequest.Any(x => x.Key == s.Key));
        return await results.ToListAsync();
    }

    public async Task<ICollection<ConfigEntry>> GetConfig(ConfigRequest configRequest)
    { 
        var results = await _configCollection.FindAsync(_ => true);
        var resultsAsList = await results.ToListAsync();

        var groupedEntries = GroupEntriesByKey(resultsAsList);
        var flattenedResults = GetBestEntriesFromGroupedEntries(configRequest, groupedEntries);

        return flattenedResults;
    }

    public async Task<ConfigEntry?> SetEntry(ConfigEntry entryRequest)
    {
        var result = await _configCollection.FindOneAndReplaceAsync<ConfigEntry>(
            filter => filter.Key == entryRequest.Key && filter.Conditions.Equals(entryRequest.Conditions),
            entryRequest,
            new FindOneAndReplaceOptions<ConfigEntry>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            });

        return result;
    }

    public async Task<ICollection<ConfigEntry>> SetEntries(IEnumerable<ConfigEntry> entryRequest)
    {
        var enumerable = entryRequest.ToList();
        
        foreach (var entry in enumerable)
            entry.Id = ObjectId.GenerateNewId().ToString();

        await _configCollection.InsertManyAsync(enumerable);
        
        return enumerable;
    }

    public async Task<bool> RemoveEntry(string entryId)
    {
        ConfigEntry? found = await _configCollection.FindOneAndDeleteAsync(s => s.Id == entryId);
        if (found is null)
            throw new KeyNotFoundException($"Could not find a Entry with ID {entryId} in the Collection");

        return true;
    }

    public async Task<bool> RemoveEntries(IEnumerable<string> entryRequest)
    {
        var deleteResult = await _configCollection.DeleteManyAsync(s => entryRequest.Any(x => s.Id == x));
        return deleteResult.IsAcknowledged;
    }

    public async Task<bool> Clear()
    {
        var deleteResult = await _configCollection.DeleteManyAsync(_ => true);
        return deleteResult.IsAcknowledged;
    }
    
    
    /** UTILITIES **/
    static ConfigEntry? GetEntryWithBestConditions(ConfigEntryConditions? conditions, IEnumerable<ConfigEntry> results)
    {
        // Return the Entry with the most number of Configuration matches.
        ConfigEntry? bestConfigEntry = null;
        int bestScore = 0;
        foreach (var configEntry in results)
        {
            int score = configEntry.Conditions.Matches(conditions);
            if (score > bestScore)
            {
                bestConfigEntry = configEntry;
                bestScore = score;
            }
        }

        return bestConfigEntry;
    }
    
    static Dictionary<string, List<ConfigEntry>> GroupEntriesByKey(List<ConfigEntry> resultsAsList)
    {
        // Group Config Entries by Key.
        Dictionary<string, List<ConfigEntry>> groupedEntries = new();
        foreach (var configEntry in resultsAsList)
        {
            var configEntryGroup = groupedEntries.GetOrAdd(configEntry.Key);
            configEntryGroup.Add(configEntry);
        }

        return groupedEntries;
    }
    
    static List<ConfigEntry> GetBestEntriesFromGroupedEntries(ConfigRequest configRequest, Dictionary<string, List<ConfigEntry>> groupedEntries)
    {
        // Flatten the Config Entry Groups by only selecting the best one.

        List<ConfigEntry> flattenedResults = new();
        foreach (var configEntryGroup in groupedEntries)
        {
            var bestEntry = GetEntryWithBestConditions(configRequest.Conditions, configEntryGroup.Value);
            if (bestEntry is not null)
                flattenedResults.Add(bestEntry);
        }

        return flattenedResults;
    }
}