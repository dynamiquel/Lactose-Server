using Lactose.Economy.Models;

namespace Lactose.Economy.Dtos.Transactions;

public class QueryTransactionsRequest;

public class QueryTransactionsResponse
{
    public IList<string> TransactionIds { get; set; } = new List<string>();
}

public class GetTransactionRequest
{
    public required string TransactionId { get; set; }
}

public class GetTransactionResponse
{
    public string? SourceUserId { get; set; }
    public string? DestinationUserId { get; set; }
    public required string ItemId { get; set; }
    public required int Quantity { get; set; }
    public required DateTime Time { get; set; }
}

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

public enum TradeResponseReason
{
    Success,
    UserANotFound,
    UserBNotFound,
    UserAInsufficientFunds,
    UserBInsufficientFunds
}

public class TradeResponse
{
    public TradeResponseReason Reason { get; set; }
}