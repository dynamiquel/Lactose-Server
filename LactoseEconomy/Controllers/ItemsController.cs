using Lactose.Economy.Data.Repos;
using Lactose.Economy.Dtos.Items;
using Lactose.Economy.Models;
using Lactose.Economy.Mapping;
using LactoseWebApp;
using LactoseWebApp.Auth;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Economy.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController(
    ILogger<ItemsController> logger,
    IItemsRepo itemsRepo) : ControllerBase, IItemsController
{
    [HttpPost("query", Name = "Query Items")]
    public async Task<ActionResult<QueryItemsResponse>> QueryItems(QueryItemsRequest request)
    {
        ISet<string> foundItems = await itemsRepo.Query();

        return Ok(new QueryItemsResponse
        {
            ItemIds = foundItems.ToList()
        });
    }
    
    [HttpPost(Name = "Get Items")]
    [Authorize]
    public async Task<ActionResult<GetItemsResponse>> GetItems(GetItemsRequest request)
    {
        if (!User.HasBoolClaim(Permissions.Read))
            return Unauthorized("You do not have permission to read items");
        
        var foundItems = await itemsRepo.Get(request.ItemIds.ToHashSet());
        return Ok(ItemMapper.ToDto(foundItems));
    }

    [HttpPost("create", Name = "Create Item")]
    [Authorize]
    public async Task<ActionResult<GetItemResponse>> CreateItem(CreateItemRequest request)
    {
        if (!User.HasBoolClaim(Permissions.Write))
            return Unauthorized("You do not have permission to write items");
        
        var newItem = new Item
        {
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            GameImage = request.GameImage
        };
        
        var createdItem = await itemsRepo.Set(newItem);
        if (createdItem is null)
            return StatusCode(500, $"Could not create Item with name '{request.Name}'");
        
        return Ok(ItemMapper.ToDto(createdItem));
    }

    [HttpPost("update", Name = "Update Item")]
    [Authorize]
    public async Task<ActionResult<GetItemResponse>> UpdateItem(UpdateItemRequest request)
    {
        if (!request.ItemId.IsValidObjectId())
            return BadRequest($"ItemId '{request.ItemId}' is not a valid ItemId");
     
        if (!User.HasBoolClaim(Permissions.Write))
            return Unauthorized("You do not have permission to write items");
        
        var existingItem = await itemsRepo.Get(request.ItemId);
        if (existingItem is null)
            return BadRequest($"Item with Id '{request.ItemId}' does not exist");

        if (request.Name is not null)
            existingItem.Name = request.Name;
        if (request.Type is not null)
            existingItem.Type = request.Type;
        if (request.Description is not null)
            existingItem.Description = request.Description;
        if (request.GameImage is not null)
            existingItem.GameImage = request.GameImage;

        var updatedItem = await itemsRepo.Set(existingItem);
        if (updatedItem is null)
            return StatusCode(500, $"Could not update Item with Id '{request.ItemId}'");

        return Ok(ItemMapper.ToDto(updatedItem));
    }

    [HttpPost("delete", Name = "Delete Item")]
    [Authorize]
    public async Task<ActionResult<DeleteItemsResponse>> DeleteItems(DeleteItemsRequest request)
    {
        if (!User.HasBoolClaim(Permissions.Write))
            return Unauthorized("You do not have permission to write items");
        
        if (request.ItemIds is null)
        {
            bool deletedAll = await itemsRepo.Clear();
            return deletedAll ? Ok(new DeleteItemsResponse()) : BadRequest();
        }
        
        var deletedItems = await itemsRepo.Delete(request.ItemIds);
        if (deletedItems.IsEmpty())
            return BadRequest();

        return Ok(new DeleteItemsResponse
        {
            ItemIds = deletedItems.ToList()
        });
    }
}