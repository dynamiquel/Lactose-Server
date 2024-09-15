namespace Lactose.Identity.Models;

public class Role
{
    public required string RoleId { get; set; }
    public required string RoleName { get; set; }
    public ISet<string> Permissions { get; set; } = new HashSet<string>();
}