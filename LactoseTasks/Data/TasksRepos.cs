using Lactose.Tasks.Models;
using Lactose.Tasks.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Task = System.Threading.Tasks.Task;

namespace Lactose.Tasks.Data;

public class MongoTasksRepo : MongoBasicKeyValueRepo<MongoTasksRepo, Models.Task, TasksDatabaseOptions>
{
    public MongoTasksRepo(ILogger<MongoTasksRepo> logger, IOptions<TasksDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) { }
}

public class MongoUserTasksRepo : MongoBasicKeyValueRepo<MongoUserTasksRepo, UserTask, UserTasksDatabaseOptions>
{
    public MongoUserTasksRepo(ILogger<MongoUserTasksRepo> logger, IOptions<UserTasksDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) { }
    
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