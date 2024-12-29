using System.Data;
using System.Reflection;
using LactoseWebApp.Repo;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace LactoseWebApp.Mongo;

/// <summary>
/// Base class that implements basic key-value-like access to a Mongo collection.
/// Addition accessors can be created on the child class.
/// </summary>
/// <typeparam name="TParent">The interface that should be representing the concrete implementation. It is currently only being used for logging purposes</typeparam>
/// <typeparam name="TModel">The model that represents the document in the collection. Requires to implement IBasicKeyValueModel</typeparam>
/// <typeparam name="TDatabaseOptions">The Mongo database options class</typeparam>
public abstract class MongoBasicKeyValueRepo<TParent, TModel, TDatabaseOptions> : IBasicKeyValueRepo<TModel>
    where TModel : class, IBasicKeyValueModel
    where TDatabaseOptions : MongoDatabaseOptions
{
    protected IMongoCollection<TModel> Collection { get; }
    protected ILogger<TParent> Logger { get; }
    protected bool UsingObjectIds { get; }

    public MongoBasicKeyValueRepo(ILogger<TParent> logger, IOptions<TDatabaseOptions> databaseOptions)
    {
        var mongoClient = new MongoClient(databaseOptions.Value.Connection);
        var mongoDb = mongoClient.GetDatabase(databaseOptions.Value.Database);
        Collection = mongoDb.GetCollection<TModel>(databaseOptions.Value.Collection);
        Logger = logger;

        PropertyInfo? idProperty = typeof(TModel).GetProperty("Id");
        if (idProperty is null)
            throw new InvalidConstraintException($"Id Property was not found in {nameof(TModel)}. This is unexpected as it must inherit from {nameof(IBasicKeyValueModel)}");
        
        var bsonRepresentationAttribute = idProperty.GetCustomAttribute<BsonRepresentationAttribute>();
        UsingObjectIds = bsonRepresentationAttribute?.Representation == BsonType.ObjectId;
    }

    public Task<ISet<string>> Query()
    {
        Logger.LogInformation("Querying items");

        var results =
            from item in Collection.AsQueryable()
            select item.Id;

        var foundItems = results.ToHashSet();
        
        Logger.LogInformation($"Queried {foundItems.Count} items");

        return Task.FromResult<ISet<string>>(foundItems);
    }

    public Task<ICollection<TModel>> Get(ICollection<string> ids)
    {
        if (UsingObjectIds)
        {
            int mismatchedIds = ids.Remove(id => !id.IsValidObjectId());
            if (mismatchedIds > 0)
                Logger.LogError($"{mismatchedIds} items had invalid IDs. Removing them from the query");

            if (ids.IsEmpty())
            {
                Logger.LogError("No valid IDs were provided");
                return Task.FromResult<ICollection<TModel>>(new List<TModel>());
            }
        }

        Logger.LogInformation($"Retrieving {ids.Count} items with IDs '{ids.ToCommaSeparatedString()}'");

        var results =
            from item in Collection.AsQueryable()
            where ids.Contains(item.Id!)
            select item;
        
        var foundItems = results.ToList();
        
        Logger.LogInformation($"Retrieved {foundItems.Count} items");

        return Task.FromResult<ICollection<TModel>>(foundItems);
    }

    public async Task<TModel?> Set(TModel model)
    {
        if (model.Id is null)
        {
            Logger.LogInformation($"Inserting new item:\n{model.ToIndentedJson()}");

            var task = Collection.InsertOneAsync(model);
            await task;

            if (!task.IsCompletedSuccessfully)
            {
                Logger.LogError($"Failed to insert new item. Reason: {task.Exception}");
                return null;
            }
            
            Logger.LogInformation($"Inserted new item with ID: {model.Id}");
            return model;
        }
        
        Logger.LogInformation($"Replacing existing item with ID '{model.Id}' with:\n{model.ToIndentedJson()}");

        var result = await Collection.FindOneAndReplaceAsync<TModel>(
            filter => filter.Id == model.Id,
            model,
            new FindOneAndReplaceOptions<TModel>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            });

        if (result is null)
            Logger.LogError($"Failed to replace item with ID: {model.Id}");
        else
            Logger.LogInformation($"Replaced item with ID: {model.Id}");
        
        return result;
    }

    public async Task<ICollection<string>> Delete(ICollection<string> ids)
    {
        if (UsingObjectIds)
        {
            int mismatchedIds = ids.Remove(id => !id.IsValidObjectId());
            if (mismatchedIds > 0)
                Logger.LogError($"{mismatchedIds} items had invalid IDs. Removing them from the query");

            if (ids.IsEmpty())
            {
                Logger.LogError("No valid IDs were provided");
                return new List<string>();
            }
        }

        Logger.LogInformation($"Deleting {ids.Count} items with IDs '{ids.ToCommaSeparatedString()}'");

        var result = await Collection.DeleteManyAsync(item => ids.Contains(item.Id!));
        if (!result.IsAcknowledged)
        {
            Logger.LogError("Failed to deleted items");
            return new List<string>();
        }
        
        if (result.DeletedCount == ids.Count)
        {
            Logger.LogInformation($"Deleted {result.DeletedCount} items with IDs '{ids.ToCommaSeparatedString()}'");
            return ids;
        }

        // Not all the desired items were deleted. Figure out which items were not deleted.
        ISet<string> existingItems = await Query();
        var deletedItems = ids.Where(r => !existingItems.Contains(r)).ToList();
        
        Logger.LogInformation($"Deleted {result.DeletedCount} items with IDs '{deletedItems.ToCommaSeparatedString()}'");
        
        return deletedItems;
    }

    public async Task<bool> Clear()
    {
        var deleteResult = await Collection.DeleteManyAsync(_ => true);
        
        if (!deleteResult.IsAcknowledged)
            Logger.LogError("Failed to clear items");
        else
            Logger.LogInformation("Cleared items");
        
        return deleteResult.IsAcknowledged;
    }
}