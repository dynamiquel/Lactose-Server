using Lactose.Economy.Data.Repos;
using Lactose.Economy.Models;
using Lactose.Economy.Mapping;
using Lactose.Economy.Transactions;
using LactoseWebApp;
using LactoseWebApp.Auth;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;

namespace Lactose.Economy.Controllers;

public class TradeResponseReason
{
    public const string Success = "Success";
    public const string UserANotFound = "UserANotFound";
    public const string UserBNotFound = "UserBNotFound";
    public const string UserAInsufficientFunds = "UserAInsufficientFunds";
    public const string UserBInsufficientFunds = "UserBInsufficientFunds";
}

[Controller]
[Route("[controller]")]
public class TransactionsController(
    ITransactionsRepo transactionsRepo,
    IUserItemsRepo userItemsRepo,
    ILogger<TransactionsController> logger,
    IMqttClient mqttClient) : TransactionsControllerBase
{
    [Authorize]
    public override async Task<ActionResult<QueryTransactionsResponse>> Query(QueryTransactionsRequest request)
    {
        if (!User.HasBoolClaim(Permissions.ReadTransactions))
            return Unauthorized("You do not have permission to read transactions");
        
        ISet<string> foundTransactionItems = await transactionsRepo.Query();
        
        return new QueryTransactionsResponse
        {
            TransactionIds = foundTransactionItems.ToList()
        };
    }

    [Authorize]
    public override async Task<ActionResult<GetTransactionResponse>> Get(GetTransactionRequest request)
    {
        if (!request.TransactionId.IsValidObjectId())
            return BadRequest($"TransactionId '{request.TransactionId}' is not a valid TransactionId");
        
        if (!User.HasBoolClaim(Permissions.ReadTransactions))
            return Unauthorized("You do not have permission to read transactions");
        
        Transaction? foundTransaction = await transactionsRepo.Get(request.TransactionId);
        if (foundTransaction is null)
            return NotFound($"Transaction with ID '{request.TransactionId}' was not found");

        return TransactionMapper.ToDto(foundTransaction);
    }

    [HttpPost("trade", Name = "Trade")]
    [Authorize]
    public override async Task<ActionResult<TradeResponse>> Trade([FromBody] TradeRequest request)
    {
        //if (!User.HasBoolClaim(Permissions.WriteTransactions))
        //   return Unauthorized("You do not have permission to create trades");
        
        bool bUserAExists = !string.IsNullOrWhiteSpace(request.UserA.UserId);
        bool bUserBExists = !string.IsNullOrWhiteSpace(request.UserB.UserId);
        
        if (!bUserAExists && !bUserBExists)
            return BadRequest("At least one user is required");
        
        // If one of the User IDs are not then assume it is the 'void' with infinite items.

        Models.UserItems? userAItems = null;

        // If User A is set, check they have enough items to transfer to User B.
        if (bUserAExists)
        {
            userAItems = await userItemsRepo.Get(request.UserA.UserId!);
            if (userAItems is null)
            {
                return new TradeResponse
                {
                    Reason = TradeResponseReason.UserANotFound
                };
            }

            foreach (var itemToRemove in request.UserA.Items)
            {
                bool bHasEnoughItem = userAItems.Items.Any(item => item.ItemId == itemToRemove.ItemId && (item.Quantity >= itemToRemove.Quantity || item.HasInfiniteQuantity()));
                if (!bHasEnoughItem)
                {
                    return Ok(new TradeResponse
                    {
                        Reason = TradeResponseReason.UserAInsufficientFunds
                    });
                }
            }
        }

        Models.UserItems? userBItems = null;
        
        // If User B is set, check they have enough items to transfer to User A.
        if (bUserBExists)
        {
            userBItems = await userItemsRepo.Get(request.UserB.UserId!);
            if (userBItems is null)
            {
                return new TradeResponse
                {
                    Reason = TradeResponseReason.UserBNotFound
                };
            }

            foreach (var itemToRemove in request.UserB.Items)
            {
                bool bHasEnoughItem = userBItems.Items.Any(item => item.ItemId == itemToRemove.ItemId && (item.Quantity >= itemToRemove.Quantity || item.HasInfiniteQuantity()));
                if (!bHasEnoughItem)
                {
                    return new TradeResponse
                    {
                        Reason = TradeResponseReason.UserBInsufficientFunds
                    };
                }
            }
        }
        
        // Create a transaction for each item and transfer between User Items.
        
        var transactionTime = DateTime.UtcNow;
        
        IList<Transaction> userAToBTransactions = new List<Transaction>();
        
        // Transfer from User A to User B.
        foreach (var itemToTransfer in request.UserA.Items)
        {
            var newTransaction = new Transaction
            {
                SourceUserId = request.UserA.UserId,
                DestinationUserId = request.UserB.UserId,
                ItemId = itemToTransfer.ItemId,
                Quantity = itemToTransfer.Quantity,
                Time = transactionTime
            };

            Transaction? transaction = await transactionsRepo.Set(newTransaction);
            if (transaction is null)
                return StatusCode(500, "Could not create a new Transaction");
            
            userAToBTransactions.Add(transaction);
            
            userAItems?.DecreaseItemQuantity(itemToTransfer.ItemId, itemToTransfer.Quantity);
            userBItems?.IncreaseItemQuantity(itemToTransfer.ItemId, itemToTransfer.Quantity);
            
            logger.LogInformation("Transferred {Quantity} x '{ItemId}' from user '{UserAUserId}' to user '{UserBUserId}'", 
                itemToTransfer.Quantity, itemToTransfer.ItemId, request.UserA.UserId, request.UserB.UserId);
        }

        IList<Transaction> userBToATransactions = new List<Transaction>();
        
        // Transfer from User B to User A.
        foreach (var itemToTransfer in request.UserB.Items)
        {
            var newTransaction = new Transaction
            {
                SourceUserId = request.UserB.UserId,
                DestinationUserId = request.UserA.UserId,
                ItemId = itemToTransfer.ItemId,
                Quantity = itemToTransfer.Quantity,
                Time = DateTime.UtcNow
            };

            Transaction? transaction = await transactionsRepo.Set(newTransaction);
            if (transaction is null)
                return StatusCode(500, "Could not create a new Transaction");
            
            userBToATransactions.Add(transaction);

            userBItems?.DecreaseItemQuantity(itemToTransfer.ItemId, itemToTransfer.Quantity);
            userAItems?.IncreaseItemQuantity(itemToTransfer.ItemId, itemToTransfer.Quantity);
            
            logger.LogInformation($"Transferred {itemToTransfer.Quantity} x '{itemToTransfer.ItemId}' from user '{request.UserB.UserId}' to user '{request.UserA.UserId}'");
        }

        if (userAItems is not null)
        {
            userAItems = await userItemsRepo.Set(userAItems);
            if (userAItems is null)
                return StatusCode(500, $"Could not update User Items for user with UserID '{request.UserA.UserId}'");
        }

        if (userBItems is not null)
        {
            userBItems = await userItemsRepo.Set(userBItems);
            if (userBItems is null)
                return StatusCode(500, $"Could not update User Items for user with UserID '{request.UserB.UserId}'");
        }
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/economy/transactions/{request.UserA.UserId ?? "null"}/{request.UserB.UserId ?? "null"}")
            .WithPayload(new TradeEvent
            {
                OutgoingUserId = request.UserA.UserId,
                IncomingUserId = request.UserB.UserId,
                TransactionIds = userAToBTransactions.Select(t => t.Id).ToList()!
            }.ToJson())
            .Build());
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/economy/transactions/{request.UserB.UserId ?? "null"}/{request.UserA.UserId ?? "null"}")
            .WithPayload(new TradeEvent
            {
                OutgoingUserId = request.UserB.UserId,
                IncomingUserId = request.UserA.UserId,
                TransactionIds = userBToATransactions.Select(t => t.Id).ToList()!
            }.ToJson())
            .Build());

        return new TradeResponse
        {
            Reason = TradeResponseReason.Success
        };
    }
}