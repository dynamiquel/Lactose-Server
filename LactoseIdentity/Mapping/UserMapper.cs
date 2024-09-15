using Lactose.Identity.Grpc;
using Lactose.Identity.Models;
using LactoseWebApp.Mapping;
using Riok.Mapperly.Abstractions;

namespace Lactose.Identity.Mapping;

[Mapper]
public partial class UserMapper : ProtobufMapper
{
    public static partial UserResponse ToDto(User user);
}