namespace Lactose.Config.Dtos;

public class DeleteConfigRequest
{
    public IEnumerable<string>? EntriesToRemove { get; set; } = default;
}