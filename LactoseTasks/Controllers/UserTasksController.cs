using Lactose.Tasks.Data;
using Lactose.Tasks.Dtos;
using Lactose.Tasks.Mapping;
using Lactose.Tasks.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;

namespace LactoseTasks.Controllers;

[ApiController]
[Route("[controller]")]
public class UserTasksController(
    ILogger<TasksController> logger,
    MongoUserTasksRepo userTasksRepo) : ControllerBase
{
    [HttpPost("query", Name = "Query User Tasks")]
    [Authorize]
    public async Task<ActionResult<QueryUserTasksResponse>> QueryTasks(QueryUserTasksRequest request)
    {
        ISet<string> foundUserTasks = await userTasksRepo.Query();

        return Ok(new QueryTasksResponse
        {
            TaskIds = foundUserTasks.ToList()
        });
    }
    
    [HttpPost(Name = "Get User Tasks")]
    [Authorize]
    public async Task<ActionResult<GetTasksResponse>> GetTasks(GetUserTasksRequest request)
    {
        var foundUserTasks = await userTasksRepo.Get(request.UserTaskIds.ToHashSet());
        return Ok(UserTaskMapper.ToDto(foundUserTasks));
    }
    
    [HttpPost("byTaskId", Name = "Get User Tasks by Task ID")]
    [Authorize]
    public async Task<ActionResult<GetTasksResponse>> GetTasks(GetUserTasksFromTaskIdRequest request)
    {
        var foundUserTasks = await userTasksRepo.GetUserTasksByTaskId(request.UserId, request.TaskIds);
        return Ok(UserTaskMapper.ToDto(foundUserTasks));
    }
}