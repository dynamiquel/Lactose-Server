namespace Lactose.Identity.Dtos.Users;

public class CreateUserRequest
{
    public required string DisplayName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}