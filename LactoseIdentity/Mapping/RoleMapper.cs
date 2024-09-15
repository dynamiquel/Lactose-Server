using Lactose.Identity.Grpc;
using Lactose.Identity.Models;
using LactoseWebApp.Mapping;
using Riok.Mapperly.Abstractions;

namespace Lactose.Identity.Mapping;

[Mapper]
public partial class RoleMapper : ProtobufMapper
{
    public static partial RoleResponse ToDto(Role role);

    public static RolesResponse ToDto(IEnumerable<Role> roles)
    {
        return new RolesResponse
        {
            Roles = { roles.Select(ToDto) }
        };
    }

    public static partial Role ToModel(CreateRoleRequest createRoleRequest);
    
    public static Permissions ToDto(ISet<string> permissions) => new() { Permissions_ = { permissions } };
    public static ISet<string> ToModel(Permissions permissions) => new HashSet<string>(permissions.Permissions_);
}