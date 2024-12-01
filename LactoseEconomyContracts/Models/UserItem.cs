namespace Lactose.Economy.Models;

public class UserItem
{
    public required string ItemId { get; set; }
    public int Quantity { get; set; } = 1;

    public bool HasInfiniteQuantity() => Quantity == -1;
}