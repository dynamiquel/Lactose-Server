using Lactose.Economy.Data.Repos;
using Lactose.Economy.Items;
using Lactose.Economy.Models;
using Lactose.Economy.Mapping;
using LactoseWebApp;
using LactoseWebApp.Auth;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;

namespace Lactose.Economy.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController(
    ILogger<ItemsController> logger,
    IItemsRepo itemsRepo,
    IMqttClient mqttClient) : ItemsControllerBase
{
    [Authorize]
    public override async Task<ActionResult<QueryItemsResponse>> Query(QueryItemsRequest request)
    {
        ISet<string> foundItems = await itemsRepo.Query();

        return new QueryItemsResponse
        {
            ItemIds = foundItems.ToList()
        };
    }

    [Authorize]
    public override async Task<ActionResult<GetItemsResponse>> Get(GetItemsRequest request)
    {
        if (!User.HasBoolClaim(Permissions.Read))
            return Unauthorized("You do not have permission to read items");
        
        ICollection<Item> foundItems = await itemsRepo.Get(request.ItemIds.ToHashSet());
        return ItemMapper.ToDto(foundItems);
    }

    [Authorize]
    public override async Task<ActionResult<GetItemResponse>> Create(CreateItemRequest request)
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
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic("/economy/items/created")
            .WithPayload(new ItemEvent
            {
                ItemId = createdItem.Id!
            }.ToJson())
            .Build());
        
        return ItemMapper.ToDto(createdItem);
    }

    [Authorize]
    public override async Task<ActionResult<GetItemResponse>> Update(UpdateItemRequest request)
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
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic("/economy/items/updated")
            .WithPayload(new ItemEvent
            {
                ItemId = updatedItem.Id!
            }.ToJson())
            .Build());

        return ItemMapper.ToDto(updatedItem);
    }

    [Authorize]
    public override async Task<ActionResult<DeleteItemsResponse>> Delete(DeleteItemsRequest request)
    {
        if (!User.HasBoolClaim(Permissions.Write))
            return Unauthorized("You do not have permission to write items");
        
        if (request.ItemIds is null)
        {
            bool deletedAll = await itemsRepo.Clear();
            return deletedAll ? new DeleteItemsResponse { ItemIds = [] } : BadRequest();
        }
        
        var deletedItems = await itemsRepo.Delete(request.ItemIds);
        if (deletedItems.IsEmpty())
            return BadRequest();

        var publishEvents = deletedItems.Select(deletedItemId =>
            mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic("/economy/items/deleted")
                .WithPayload(new ItemEvent { ItemId = deletedItemId }.ToJson())
                .Build())
        );

        await Task.WhenAll(publishEvents);

        return new DeleteItemsResponse
        {
            ItemIds = deletedItems.ToList()
        };
    }
}