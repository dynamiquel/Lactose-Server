namespace Lactose.Economy.Dtos.UserItems;

public class QueryUserItemsResponse
{
    public IList<string> UserIds { get; set; } = new List<string>();
}