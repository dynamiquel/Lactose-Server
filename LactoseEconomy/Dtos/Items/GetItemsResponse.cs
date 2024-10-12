namespace Lactose.Economy.Dtos.Items;

public class GetItemsResponse
{
    public IList<GetItemResponse> Items { get; set; } = new List<GetItemResponse>();
}