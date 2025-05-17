using Lactose.Economy.UserItems;
using Riok.Mapperly.Abstractions;

namespace Lactose.Economy.Mapping;

[Mapper]
public partial class UserMapper
{
    public static partial UserItem ToDto(Models.UserItem userItem);
    public static partial Models.UserItem FromDto(UserItem userItemDto);
    
    public static GetUserItemsResponse ToDto(Models.UserItems userItems)
    {
        return new GetUserItemsResponse
        {
            UserItems = userItems.Items.Select(ToDto).ToList()
        };
    }
    
    public static IEnumerable<UserItem> ToDto(List<Models.UserItem> userItems)
    {
        return userItems.Select(ToDto).ToList();
    }
    
    public static List<Models.UserItem> FromDto(List<UserItem> userItemsDto)
    {
        return userItemsDto.Select(FromDto).ToList();
    }
}