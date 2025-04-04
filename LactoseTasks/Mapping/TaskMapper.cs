using Lactose.Tasks.Dtos;
using Riok.Mapperly.Abstractions;

namespace Lactose.Tasks.Mapping;

[Mapper]
public partial class TaskMapper
{
    public static partial GetTaskResponse ToDto(Lactose.Tasks.Models.Task task);

    public static GetTasksResponse ToDto(ICollection<Lactose.Tasks.Models.Task> tasks)
    {
        return new GetTasksResponse
        {
            Tasks = tasks.Select(ToDto).ToList()
        };
    }
}