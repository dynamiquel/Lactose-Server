namespace Lactose.Economy.Dtos.Transactions;

public class QueryTransactionsResponse
{
    public IList<string> TransactionIds { get; set; } = new List<string>();
}