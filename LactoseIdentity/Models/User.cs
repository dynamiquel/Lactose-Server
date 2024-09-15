namespace Lactose.Identity.Models;

public class User
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public ISet<string> Roles { get; set; } = new HashSet<string>();
    public DateTime TimeCreated { get; set; }
    public DateTime TimeLastLoggedIn { get; set; }
}