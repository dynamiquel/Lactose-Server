namespace Lactose.Identity.Dtos.Users;

public class CreateUserRequest
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}