namespace Lactose.Economy.Dtos.Items;

public class GetItemResponse
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}