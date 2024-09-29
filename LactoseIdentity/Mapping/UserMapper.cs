using Lactose.Identity.Dtos.Users;
using Lactose.Identity.Models;
using Riok.Mapperly.Abstractions;

namespace Lactose.Identity.Mapping;

[Mapper]
public partial class UserMapper
{
    public static partial UserResponse ToDto(User user);
}