using Lactose.Tasks.Data;
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
        ISet<string> foundUserTasks = await userTasksRepo.QueryUserTasks(request.UserId);

        return new QueryUserTasksResponse
        {
            UserTaskIds = foundUserTasks.ToList()
        };
    }

    [Authorize]
    public override async Task<ActionResult<GetUserTasksResponse>> Get(GetUserTasksRequest request)
    {
        var foundUserTasks = await userTasksRepo.Get(request.UserTaskIds.ToHashSet());
        return UserTaskMapper.ToDto(foundUserTasks);
    }

    [Authorize]
    public override async Task<ActionResult<GetUserTasksResponse>> GetById(GetUserTasksFromTaskIdRequest request)
    {
        var foundUserTasks = await userTasksRepo.GetUserTasksByTaskId(request.UserId, request.TaskIds);
        return UserTaskMapper.ToDto(foundUserTasks);
    }
}