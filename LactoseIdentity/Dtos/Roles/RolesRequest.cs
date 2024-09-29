namespace Lactose.Identity.Dtos.Roles;

public class RolesRequest
{
    public required IList<string> RoleIds { get; set; }
}