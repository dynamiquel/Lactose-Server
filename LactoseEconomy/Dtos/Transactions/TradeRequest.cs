using Lactose.Economy.Models;

namespace Lactose.Economy.Dtos.Transactions;

public class UserTradeRequest
{
    public string? UserId { get; set; }
    public IList<UserItem> Items { get; set; } = new List<UserItem>();
}

public class TradeRequest
{
    public required UserTradeRequest UserA { get; set; }
    public required UserTradeRequest UserB { get; set; }
}