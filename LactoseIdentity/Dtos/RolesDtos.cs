namespace Lactose.Identity.Dtos.Roles;

public class QueryRolesRequest;

public class QueryRolesResponse
{
    public required IList<string> RoleIds { get; set; }
}

public class RolesRequest
{
    public required IList<string> RoleIds { get; set; }
}

public class RoleResponse
{
    public required string Id { get; set; }
    public required string RoleName { get; set; }
    public IList<string> Permissions { get; set; } = new List<string>();
}

public class RolesResponse
{
    public IList<RoleResponse> Roles { get; set; } = new List<RoleResponse>();
}

public class CreateRoleRequest
{
    public required string Id { get; set; }
    public required string RoleName { get; set; }
    public IList<string> Permissions { get; set; } = new List<string>();
}