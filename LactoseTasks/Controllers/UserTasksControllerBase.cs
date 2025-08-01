//
// This file was generated by Catalyst's C# compiler at 02/05/2025 17:12:38.
// It is recommended not to modify this file. Modify the source spec file instead.
//

using Microsoft.AspNetCore.Mvc;

namespace Lactose.Tasks;

[ApiController]
[Route("userTasks")]
public abstract class UserTasksControllerBase : ControllerBase
{
    [HttpPost("query", Name = "UserTasksQuery")]
    public abstract Task<ActionResult<Lactose.Tasks.QueryUserTasksResponse>> Query(Lactose.Tasks.QueryUserTasksRequest request);

    [HttpPost("get", Name = "UserTasksGet")]
    public abstract Task<ActionResult<Lactose.Tasks.GetUserTasksResponse>> Get(Lactose.Tasks.GetUserTasksRequest request);
    
    [HttpPost("getById", Name = "UserTasksGetById")]
    public abstract Task<ActionResult<Lactose.Tasks.GetUserTasksResponse>> GetById(Lactose.Tasks.GetUserTasksFromTaskIdRequest request);
}
