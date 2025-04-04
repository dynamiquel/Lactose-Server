using Lactose.Tasks.Data;
using Lactose.Tasks.Dtos;
using Lactose.Tasks.Mapping;
using Lactose.Tasks.Models;
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
    MongoTasksRepo tasksRepo) 
    : ControllerBase
{
    [HttpPost("query", Name = "Query Tasks")]
    [Authorize]
    public async Task<ActionResult<QueryTasksRequest>> QueryTasks(QueryTasksRequest request)
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
        List<Trigger> triggers = request.Triggers.Select(trigger => new Trigger
        {
            Topic = trigger.Topic, 
            Handler = trigger.Handler, 
            Config = trigger.HandlerConfig
        }).ToList();

        Lactose.Tasks.Models.Task newTask = new()
        {
            TaskName = request.Name,
            TaskDescription = request.Description,
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