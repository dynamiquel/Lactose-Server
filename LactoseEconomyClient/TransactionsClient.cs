using System.Net.Http.Json;
using Lactose.Economy.Controllers;
using Lactose.Economy.Dtos.Transactions;
using LactoseClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Lactose.Economy;

public class TransactionsClient(
    HttpClient httpClient,
    IOptions<EconomyClientOptions> options) : ITransactionsController
{
    public async Task<ActionResult<QueryTransactionsResponse>> QueryTransactions(QueryTransactionsRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/transactions/query"),
            Content = JsonContent.Create(request)
        };

        var response = await httpClient.SendFromJson<QueryTransactionsResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public async Task<ActionResult<GetTransactionResponse>> GetTransaction(GetTransactionRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/transactions"),
            Content = JsonContent.Create(request)
        };

        var response = await httpClient.SendFromJson<GetTransactionResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public async Task<ActionResult<TradeResponse>> Trade(TradeRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/transactions/trade"),
            Content = JsonContent.Create(request)
        };

        var response = await httpClient.SendFromJson<TradeResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }
}