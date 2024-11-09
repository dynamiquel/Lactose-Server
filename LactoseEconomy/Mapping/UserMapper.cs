using Lactose.Economy.Dtos.UserItems;
using Lactose.Economy.Models;
using Riok.Mapperly.Abstractions;

namespace Lactose.Economy.Mapping;

[Mapper]
public partial class UserMapper
{
    public static partial GetUserItemsResponse ToDto(UserItems userItems);
}