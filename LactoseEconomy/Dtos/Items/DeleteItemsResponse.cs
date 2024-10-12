namespace Lactose.Economy.Dtos.Items;

public class DeleteItemsResponse
{
    public IList<string> ItemIds { get; set; } = new List<string>();
}