using Lactose.Economy.Models;

namespace LactoseEconomy.Dtos.UserItems;

public class GetUserItemsResponse
{
    public IList<UserItem> Items { get; set; } = new List<UserItem>();
}