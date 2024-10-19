using Lactose.Economy.Models;

namespace Lactose.Economy.Dtos.UserItems;

public class GetUserItemsResponse
{
    public IList<UserItem> Items { get; set; } = new List<UserItem>();
}