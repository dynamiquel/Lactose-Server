using Lactose.Tasks.Data;
using Lactose.Tasks.Dtos;
using Lactose.Tasks.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Tasks.Controllers;

public class UserTasksController(
    ILogger<UserTasksController> logger,
    MongoUserTasksRepo userTasksRepo) : UserTasksControllerBase
{
    [Authorize]
    public override async Task<ActionResult<QueryUserTasksResponse>> Query(QueryUserTasksRequest request)
    {
        ISet<string> foundUserTasks = await userTasksRepo.Query();

        return Ok(new QueryTasksResponse
        {
            TaskIds = foundUserTasks.ToList()
        });
    }

    [Authorize]
    public override async Task<ActionResult<GetUserTasksResponse>> Get(GetUserTasksRequest request)
    {
        var foundUserTasks = await userTasksRepo.Get(request.UserTaskIds.ToHashSet());
        return Ok(UserTaskMapper.ToDto(foundUserTasks));
    }

    [Authorize]
    public override async Task<ActionResult<GetUserTasksResponse>> GetById(GetUserTasksFromTaskIdRequest request)
    {
        var foundUserTasks = await userTasksRepo.GetUserTasksByTaskId(request.UserId, request.TaskIds);
        return Ok(UserTaskMapper.ToDto(foundUserTasks));
    }
}