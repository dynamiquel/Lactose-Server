using Lactose.Tasks;
using Riok.Mapperly.Abstractions;
using UserTask = Lactose.Tasks.Models.UserTask;

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