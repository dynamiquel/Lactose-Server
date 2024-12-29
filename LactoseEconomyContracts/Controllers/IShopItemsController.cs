using LactoseEconomyContracts.Dtos.ShopItems;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Economy.Controllers;

public interface IShopItemsController
{
    Task<ActionResult<GetShopItemsRequest>> GetShopItems(GetShopItemsRequest request);
    Task<ActionResult<GetUserShopItemsResponse>> GetUserShopItems(GetUserShopItemsRequest request);
    Task<ActionResult<UpdateUserShopItemsResponse>> UpdateUserShopItems(UpdateUserShopItemsRequest request);
    Task<ActionResult<DeleteUserShopResponse>> DeleteUserShop(DeleteUserShopRequest request);
}