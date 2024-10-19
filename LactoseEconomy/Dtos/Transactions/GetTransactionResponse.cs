namespace Lactose.Economy.Dtos.Transactions;

public class GetTransactionResponse
{
    public string? SourceUserId { get; set; }
    public string? DestinationUserId { get; set; }
    public required string ItemId { get; set; }
    public required int Quantity { get; set; }
    public required DateTime Time { get; set; }
}