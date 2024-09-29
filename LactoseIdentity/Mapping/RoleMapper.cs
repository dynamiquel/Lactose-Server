using Lactose.Identity.Dtos.Roles;
using Lactose.Identity.Models;
using Riok.Mapperly.Abstractions;

namespace Lactose.Identity.Mapping;

[Mapper]
public partial class RoleMapper
{
    public static partial RoleResponse ToDto(Role role);

    public static RolesResponse ToDto(IEnumerable<Role> roles)
    {
        return new RolesResponse
        {
            Roles = roles.Select(ToDto).ToList()
        };
    }

    public static partial Role ToModel(CreateRoleRequest createRoleRequest);
}