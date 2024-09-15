using Lactose.Identity.Grpc;
using Lactose.Identity.Models;

namespace Lactose.Identity.Data.Repos;

public interface IRolesRepo
{
    Task<ISet<string>> QueryRoles();
    Task<ICollection<Role>> GetRolesByIds(ICollection<string> roleIds);
    Task<Role?> CreateRole(Role role);
    Task<IEnumerable<string>> DeleteRoles(ICollection<string> roleIds);
}