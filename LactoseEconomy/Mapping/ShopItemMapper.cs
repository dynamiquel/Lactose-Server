using Lactose.Economy.Models;
using LactoseEconomyContracts.Dtos.ShopItems;
using Riok.Mapperly.Abstractions;

namespace Lactose.Economy.Mapping;

[Mapper]
public partial class ShopItemMapper
{
    public static partial ShopItemDto ToDto(ShopItem shopItem);
    
    public static GetUserShopItemsResponse ToDto(ICollection<ShopItem> shopItems)
    {
        return new GetUserShopItemsResponse
        {
            ShopItems = shopItems.Select(ToDto).ToList()
        };
    }
    
    public static GetShopItemsResponse ToShopItemsDto(ICollection<ShopItem> shopItems)
    {
        return new GetShopItemsResponse
        {
            ShopItems = shopItems.Select(ToDto).ToList()
        };
    }
}