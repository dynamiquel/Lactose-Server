namespace Lactose.Economy.Dtos.Items;

public class CreateItemRequest
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}