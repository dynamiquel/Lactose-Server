namespace Lactose.Economy.Dtos.Transactions;

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