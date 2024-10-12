namespace Lactose.Economy.Dtos.Items;

public class GetItemsRequest
{
    public required IList<string> ItemIds { get; set; }
}