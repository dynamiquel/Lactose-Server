namespace Lactose.Economy.Dtos.Items;

public class UpdateItemRequest
{
    public required string ItemId { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}