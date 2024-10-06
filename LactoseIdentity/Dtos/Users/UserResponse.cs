namespace Lactose.Identity.Dtos.Users;

public class UserResponse
{
    public required string UserId { get; set; }
    public required string DisplayName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime TimeCreated { get; set; }
    public DateTime TimeLastLoggedIn { get; set; }
}