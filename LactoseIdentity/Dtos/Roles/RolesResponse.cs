namespace Lactose.Identity.Dtos.Roles;

public class RolesResponse
{
    public IList<RoleResponse> Roles { get; set; } = new List<RoleResponse>();
}