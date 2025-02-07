namespace Lactose.Identity.Dtos.Apis;

public class CreateApiRequest
{
    public required string DisplayName { get; init; }
    public required string ApiId { get; init; }
    public required string ApiPassword { get; init; }
    public IList<string> Roles { get; set; } = new List<string>();
}
