using System.Text;
using Lactose.Economy.Data.Repos;
using Lactose.Economy.Mapping;
using Lactose.Economy.Models;
using Lactose.Economy.ShopItems;
using Lactose.Economy.Transactions;
using LactoseWebApp;
using LactoseWebApp.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Economy.Controllers;

[ApiController]
[Route("[controller]")]
public class ShopItemsController(
    ILogger<ShopItemsController> logger,
    IShopItemsRepo shopItemsRepo,
    IUserItemsRepo userItemsRepo,
    TransactionsController transactionsController) : ShopItemsControllerBase
{
    [Authorize]
    public override async Task<ActionResult<GetShopItemsResponse>> Get(GetShopItemsRequest request)
    {
        if (!User.HasBoolClaim(Permissions.Read))
            return Unauthorized("You do not have permission to read items");
        
        ICollection<ShopItem> foundShopItems = await shopItemsRepo.Get(request.ShopItemIds);
        return ShopItemMapper.ToDto(foundShopItems);
    }

    [Authorize]
    public override async Task<ActionResult<GetShopItemsResponse>> GetUserShop(GetShopItemsForUserRequest request)
    {
        if (!User.HasBoolClaim(Permissions.Read))
            return Unauthorized("You do not have permission to read items");

        ICollection<ShopItem> foundShopItems = await shopItemsRepo.GetUserShopItems(request.UserId);

        GetShopItemsResponse userShopDto = ShopItemMapper.ToDto(foundShopItems);

        if (request.RetrieveUserQuantity)
        {
            // Do additional processing to get the quantity the user is actually able to sell.
            Models.UserItems? foundUserItems = await userItemsRepo.Get(request.UserId);
            
            foreach (var shopItemDto in userShopDto.ShopItems)
            {
                if (shopItemDto.TransactionType == ShopItemTransactionTypes.Sell)
                {
                    var foundUserItem = foundUserItems?.Items.FirstOrDefault(userItem => userItem.ItemId == shopItemDto.ItemId);
                    shopItemDto.Quantity = foundUserItem?.Quantity ?? 0;
                }
            }
        }

        return Ok(userShopDto);
    }

    [Authorize]
    public override async Task<ActionResult<UpdateUserShopItemsResponse>> UpdateUserShop(UpdateUserShopItemsRequest request)
    {
        if (request.NewItems.IsEmpty() && request.ItemsToRemove.IsEmpty())
            return BadRequest("You must provide at least one item to update or remove");
        
        if (!User.HasBoolClaim(Permissions.Write))
            return Unauthorized("You do not have permission to write items");

        if (!request.NewItems.IsEmpty())
        {
            var duplicateItemsSet = new HashSet<(string, string)>();
            foreach (var newItem in request.NewItems)
            {
                if (!duplicateItemsSet.Add((newItem.ItemId, newItem.TransactionType)))
                    return BadRequest($"Duplicate New Items with the Item ID '{newItem.ItemId}' and Transaction Type '{newItem.TransactionType}' exists");
            }
        }
        
        var foundShopItems = await shopItemsRepo.GetUserShopItems(request.UserId);
        
        var removedShopItems = new List<string>();

        if (!request.ItemsToRemove.IsEmpty())
        {
            var shopItemsToRemove = foundShopItems
                .Where(shopItem => request.ItemsToRemove.Contains(shopItem.ItemId))
                .Select(shopItem => shopItem.Id);
            
            removedShopItems = shopItemsToRemove.ToList()!;
        }
        
        var updatedShopItems = new List<ShopItem>();
        var newShopItems = new List<ShopItem>();

        if (!request.NewItems.IsEmpty())
        {
            foreach (var newShopItem in request.NewItems)
            {
                var existingShopItem = foundShopItems.FirstOrDefault(shopItem => shopItem.ItemId == newShopItem.ItemId);
                if (existingShopItem is not null)
                {
                    updatedShopItems.Add(new ShopItem
                    {
                        Id = existingShopItem.Id,
                        UserId = existingShopItem.UserId,
                        ItemId = existingShopItem.ItemId,
                        TransactionType = newShopItem.TransactionType,
                        TransactionItems = UserMapper.FromDto(newShopItem.TransactionItems)
                    });
                }
                else
                {
                    newShopItems.Add(new ShopItem
                    {
                        UserId = request.UserId,
                        ItemId = newShopItem.ItemId,
                        TransactionType = newShopItem.TransactionType,
                        TransactionItems = UserMapper.FromDto(newShopItem.TransactionItems)
                    });
                }
            }
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"User Shop for {request.UserId}");
            sb.AppendLine("\tNew Shop Items:");
            foreach (var newShopItem in newShopItems)
                sb.AppendLine($"\t\t- {newShopItem}");
            sb.AppendLine("\tUpdated Shop Items:");
            foreach (var updatedShopItem in updatedShopItems)
                sb.AppendLine($"\t\t- {updatedShopItem}");
            sb.AppendLine("\tRemoved Shop Items:");
            foreach (var removedShopItem in removedShopItems)
                sb.AppendLine($"\t\t- {removedShopItem}");

            logger.LogInformation(sb.ToString());
        }

        var setTasks = new List<Task<ShopItem?>>();
        setTasks.AddRange(updatedShopItems.Select(shopItemsRepo.Set));
        setTasks.AddRange(newShopItems.Select(shopItemsRepo.Set));

        ShopItem?[] completedTasks = await Task.WhenAll(setTasks);

        var response = new UpdateUserShopItemsResponse
        {
            AddedItems = completedTasks.Where(set => set?.Id is not null)
                .Select(set => set!.Id!)
                .ToList(),
            RemovedItems = []
        };

        if (!removedShopItems.IsEmpty())
        {
            var deleted = await shopItemsRepo.Delete(removedShopItems);
            response.RemovedItems = deleted.ToList();
        }

        return response;
    }

    [Authorize]
    public override async Task<ActionResult<DeleteUserShopResponse>> DeleteUserShop(DeleteUserShopRequest request)
    {
        if (!User.HasBoolClaim(Permissions.Write))
            return Unauthorized("You do not have permission to write items");
        
        bool success = await shopItemsRepo.DeleteUserShopItems(request.UserId);
        if (!success)
            return StatusCode(500, $"Could not delete user shop for user with ID '{request.UserId}'");
        
        return new DeleteUserShopResponse();
    }

    [Authorize]
    public override async Task<ActionResult<TradeShopItemResponse>> Trade(TradeShopItemRequest request)
    {
        // Get the Shop Item.
        var shopItem = await shopItemsRepo.Get(request.ShopItemId);
        if (shopItem is null)
            return NotFound();
        
        var desiredUserItem = new UserItems.UserItem
        {
            ItemId = shopItem.ItemId,
            Quantity = request.Quantity
        };

        var desiredShopItems = UserMapper.ToDto(shopItem.TransactionItems.Select(transactionItem => new UserItem
        {
            ItemId = transactionItem.ItemId, 
            Quantity = transactionItem.Quantity * request.Quantity
        }).ToList());

        // Create the Trade Request.
        var shopUserTradeItems = new List<UserItems.UserItem>();
        var instigatingUserTradeItems = new List<UserItems.UserItem>();
        if (shopItem.TransactionType == ShopItemTransactionTypes.Buy)
        {
            shopUserTradeItems.AddRange(desiredShopItems);
            instigatingUserTradeItems.Add(desiredUserItem);
        }
        else if (shopItem.TransactionType == ShopItemTransactionTypes.Sell)
        {
            shopUserTradeItems.Add(desiredUserItem);
            instigatingUserTradeItems.AddRange(desiredShopItems);
        }
        else
        {
            return BadRequest("Unknown transaction type");
        }
        
        var tradeRequest = new TradeRequest()
        {
            UserA = new UserTradeRequest
            {
                UserId = request.UserId,
                Items = instigatingUserTradeItems
            },
            UserB = new UserTradeRequest
            {
                UserId = shopItem.UserId,
                Items = shopUserTradeItems
            }
        };

        var tradeResult = await transactionsController.Trade(tradeRequest);
        if (tradeResult.Value is not null)
        {
            return new TradeShopItemResponse
            {
                Reason = tradeResult.Value.Reason
            };
        }

        return tradeResult.Result ?? StatusCode(500);
    }
}

public class ShopItemTransactionTypes
{
    public const string Buy = "Buy";
    public const string Sell = "Sell";
}