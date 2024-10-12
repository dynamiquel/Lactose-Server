namespace Lactose.Economy.Dtos.Items;

public class QueryItemsResponse
{
    public IList<string> ItemIds { get; set; } = new List<string>();
}