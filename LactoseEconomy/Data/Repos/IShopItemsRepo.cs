using Lactose.Economy.Models;
using LactoseEconomyContracts.Dtos.ShopItems;
using LactoseWebApp.Repo;

namespace Lactose.Economy.Data.Repos;

public interface IShopItemsRepo : IBasicKeyValueRepo<ShopItem>
{
    public Task<ICollection<ShopItem>> GetUserShopItems(string userId);
    public Task<bool> DeleteUserShopItems(string userId);
}