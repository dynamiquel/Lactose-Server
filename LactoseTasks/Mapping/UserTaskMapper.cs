using Lactose.Tasks.Models;
using Lactose.Tasks.Repos;
using Riok.Mapperly.Abstractions;

namespace Lactose.Tasks.Mapping;

[Mapper]
public partial class UserTaskMapper
{
    public static partial UserTaskDto ToDto(UserTask userTask);

    public static GetUserTasksResponse ToDto(ICollection<UserTask> userTasks)
    {
        return new GetUserTasksResponse
        {
            UserTasks = userTasks.Select(ToDto).ToList()
        };
    }
}