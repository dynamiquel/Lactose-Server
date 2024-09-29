namespace Lactose.Identity.Dtos.Roles;

public class RoleResponse
{
    public required string RoleId { get; set; }
    public required string RoleName { get; set; }
    public IList<string> Permissions { get; set; } = new List<string>();
}