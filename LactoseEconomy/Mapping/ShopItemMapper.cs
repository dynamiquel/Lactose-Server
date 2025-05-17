using Lactose.Economy.Models;
using Lactose.Economy.ShopItems;
using Riok.Mapperly.Abstractions;

namespace Lactose.Economy.Mapping;

[Mapper]
public partial class ShopItemMapper
{
    public static partial ShopItemDto ToDto(ShopItem shopItem);
    
    public static GetShopItemsResponse ToDto(ICollection<ShopItem> shopItems)
    {
        return new GetShopItemsResponse
        {
            ShopItems = shopItems.Select(ToDto).ToList()
        };
    }
}