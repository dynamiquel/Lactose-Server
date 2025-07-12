using Lactose.Tasks.Models;
using Lactose.Tasks.Options;
using LactoseWebApp;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MQTTnet;
using Task = System.Threading.Tasks.Task;

namespace Lactose.Tasks.Data;

public class MongoTasksRepo : MongoBasicKeyValueRepo<MongoTasksRepo, Models.Task, TasksDatabaseOptions>
{
    public MongoTasksRepo(ILogger<MongoTasksRepo> logger, IOptions<TasksDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) { }

    public async Task<HashSet<string>> GetAllTriggerTopics()
    {
        var results =
            from item in Collection.AsQueryable()
            select item.Triggers;

        HashSet<string> allTopics = [];
        await results.ForEachAsync(taskTriggers => allTopics.AddRange(taskTriggers.Select(r => r.Topic)));
        return allTopics;
    }

    public async Task<List<Models.Task>> GetTasksWithTriggerTopic(string topic)
    {
        var results =
            from item in Collection.AsQueryable()
            select item;

        var tasks = await results.ToListAsync() ?? [];

        return tasks.Where(task => task.Triggers.Any(t => 
            MqttTopicFilterComparer.Compare(topic, t.Topic) == MqttTopicFilterCompareResult.IsMatch)).ToList();
    }
}

public class MongoUserTasksRepo : MongoBasicKeyValueRepo<MongoUserTasksRepo, UserTask, UserTasksDatabaseOptions>
{
    public MongoUserTasksRepo(ILogger<MongoUserTasksRepo> logger, IOptions<UserTasksDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) { }

    public Task<ISet<string>> QueryUserTasks(string userId)
    {
        Logger.LogInformation("Querying items");

        var results =
            from item in Collection.AsQueryable()
            where item.UserId == userId
            select item.Id;

        var foundItems = results.ToHashSet();
        
        Logger.LogInformation($"Queried {foundItems.Count} items");

        return Task.FromResult<ISet<string>>(foundItems);
    }

    public async Task<List<UserTask>> GetUserTasksByTaskId(string userId, IEnumerable<string> taskIds)
    {
        Logger.LogInformation($"Finding User Tasks for '{userId}'");

        var results =
            from item in Collection.AsQueryable()
            where item.UserId == userId && taskIds.Contains(item.TaskId)
            select item;
        
        return await results.ToListAsync();
    }
}