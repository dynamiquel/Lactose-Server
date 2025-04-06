using System.Text.Json;
using Lactose.Tasks.Data;
using Lactose.Tasks.Dtos;
using Lactose.Tasks.Mapping;
using Lactose.Tasks.Models;
using Lactose.Tasks.TaskTriggerHandlers;
using LactoseWebApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using Task = System.Threading.Tasks.Task;

namespace LactoseTasks.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController(
    ILogger<TasksController> logger,
    IMqttClient mqttClient,
    MongoTasksRepo tasksRepo,
    TaskTriggerHandlerRegistry triggerHandlerRegistry) 
    : ControllerBase
{
    [HttpPost("query", Name = "Query Tasks")]
    [Authorize]
    public async Task<ActionResult<QueryTasksResponse>> QueryTasks(QueryTasksRequest request)
    {
        ISet<string> foundTasks = await tasksRepo.Query();

        return Ok(new QueryTasksResponse
        {
            TaskIds = foundTasks.ToList()
        });
    }
    
    [HttpPost(Name = "Get Tasks")]
    [Authorize]
    public async Task<ActionResult<GetTasksResponse>> GetTasks(GetTasksRequest request)
    {
        var foundTasks = await tasksRepo.Get(request.TaskIds.ToHashSet());
        return Ok(TaskMapper.ToDto(foundTasks));
    }
    
    [HttpPost("create", Name = "Create Task")]
    public async Task<ActionResult<GetTaskResponse>> CreateTask(CreateTaskRequest request)
    {
        List<Trigger> triggers = [];
        foreach (var trigger in request.Triggers)
        {
            ITaskTriggerHandler? foundTriggerHandler = triggerHandlerRegistry.FindHandler(trigger.Handler);
            if (foundTriggerHandler is null)
                return BadRequest($"Task Trigger Handler with name '{trigger.Handler}' is not found");

            // Since Config is an object?, it should be deserialised as a JsonElement?, indicating it is
            // valid JSON but the program couldn't figure out its type.
            // We will figure out its type now based on the Trigger Handler.
            // We need to do this because Mongo's BSON serialiser doesn't understand JsonElements :(

            object? config = trigger.Config;
            if (trigger.Config is JsonElement configJson)
            {
                try
                {
                    config = configJson.Deserialize(foundTriggerHandler.ConfigType);
                }
                catch (Exception e)
                {
                    return BadRequest($"Failed to parse Handler Config into type {foundTriggerHandler.ConfigType.Name}.\nReceived: {configJson.GetRawText()}\nReason: {e.Message}");
                }
            }
            
            triggers.Add(new Trigger
            {
                Topic = trigger.Topic,
                Handler = trigger.Handler,
                Config = (TaskHandlerConfig?)config
            });
        }

        Lactose.Tasks.Models.Task newTask = new()
        {
            Name = request.Name,
            Description = request.Description,
            RequiredProgress = request.RequiredProgress,
            Triggers = triggers
        };
        
        var createdTask = await tasksRepo.Set(newTask);
        if (createdTask is null)
            return StatusCode(500, $"Could not create Task with name '{request.Name}'");
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic("/tasks/task/created")
            .WithPayload(new TaskEvent
            {
                TaskId = createdTask.Id!
            }.ToJson())
            .Build());
        
        return Ok(TaskMapper.ToDto(createdTask));
    }
    
    [HttpPost("delete", Name = "Delete Tasks")]
    public async Task<ActionResult<DeleteTasksResponse>> DeleteTasks(DeleteTasksRequest request)
    {
        if (request.TaskIds is null)
        {
            bool deletedAll = await tasksRepo.Clear();
            return deletedAll ? Ok(new DeleteTasksResponse()) : BadRequest();
        }
        
        var deletedTaskIds = await tasksRepo.Delete(request.TaskIds);
        if (deletedTaskIds.IsEmpty())
            return BadRequest();
        
        var publishEvents = deletedTaskIds.Select(deletedTaskId =>
            mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic("/tasks/task/deleted")
                .WithPayload(new TaskEvent { TaskId = deletedTaskId }.ToJson())
                .Build())
        );

        await Task.WhenAll(publishEvents);

        return Ok(new DeleteTasksResponse
        {
            DeletedTaskIds = deletedTaskIds.ToList()
        });
    }
}