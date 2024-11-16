using Lactose.Economy.Dtos.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Economy.Controllers;

public interface ITransactionsController
{
    Task<ActionResult<QueryTransactionsResponse>> QueryTransactions(QueryTransactionsRequest request);
    Task<ActionResult<GetTransactionResponse>> GetTransaction(GetTransactionRequest request);
    Task<ActionResult<TradeResponse>> Trade(TradeRequest request);
}