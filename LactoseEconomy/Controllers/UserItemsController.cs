using Lactose.Economy.Data.Repos;
using Lactose.Economy.Dtos.UserItems;
using Lactose.Economy.Models;
using Lactose.Economy.Mapping;
using Lactose.Economy.Options;
using LactoseWebApp;
using LactoseWebApp.Auth;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace Lactose.Economy.Controllers;

[ApiController]
[Route("[controller]")]
public class UserItemsController(
    IUserItemsRepo userItemsRepo, 
    IOptions<UserStartingItemsOptions> userStartingItems,
    IMqttClient mqttClient) : ControllerBase, IUserItemsController
{
    static bool IsValidEconomyUser(string userId) => userId.IsValidObjectId() || userId.StartsWith("vendor");
    
    [HttpPost("query", Name = "Query User Items")]
    [Authorize]
    public async Task<ActionResult<QueryUserItemsResponse>> QueryUserItems(QueryUserItemsRequest request)
    {
        ISet<string> foundItems = await userItemsRepo.Query();

        return Ok(new QueryUserItemsResponse
        {
            UserIds =  foundItems.ToList()
        });
    }
    
    [HttpPost(Name = "Get User Items")]
    [Authorize]
    public async Task<ActionResult<GetUserItemsResponse>> GetUserItems(GetUserItemsRequest request)
    {
        if (!IsValidEconomyUser(request.UserId))
            return BadRequest($"UserId '{request.UserId}' is not a valid UserId");
        
        bool bCanRead = User.MatchesId(request.UserId) && User.HasBoolClaim(Permissions.ReadUserSelf) ||
                        User.HasBoolClaim(Permissions.ReadUserOthers);
        
        if (!bCanRead)
            return Unauthorized($"You do not have permission to read user's '{request.UserId}' items");

        UserItems? foundUserItems = await userItemsRepo.Get(request.UserId);
        if (foundUserItems is null)
        {
            // TODO: Check that user exists with the provided ID before creating a User Items.
            
            // User Items don't exist for the specified user, create it with starting items.
            var newModel = new UserItems
            {
                Id = request.UserId,
                // Not the most ideal place to do this. I would assume some other system would listen for some kind
                // of user created event and would initialise necessary user data?
                Items = userStartingItems.Value.StartingUserItems
            };

            UserItems? createdModel = await userItemsRepo.Set(newModel);
            if (createdModel is null)
                return StatusCode(500, $"Could not create User Items for User with ID '{request.UserId}'");
            
            foundUserItems = createdModel;
        }

        return Ok(UserMapper.ToDto(foundUserItems));
    }

    [HttpPost("delete", Name = "Delete User Items")]
    [Authorize]
    public async Task<ActionResult<bool>> DeleteUserItems(GetUserItemsRequest request)
    {
        if (!IsValidEconomyUser(request.UserId))
            return BadRequest($"UserId '{request.UserId}' is not a valid UserId");
        
        bool bCanWrite = User.MatchesId(request.UserId) && User.HasBoolClaim(Permissions.WriteUserSelf) ||
                         User.HasBoolClaim(Permissions.WriteUserOthers);
        
        if (!bCanWrite)
            return Unauthorized($"You do not have permission to write user's '{request.UserId}' items");
        
        bool result = await userItemsRepo.Delete(request.UserId);
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/economy/useritems/{request.UserId}/deleted")
            .WithPayload(new UserItemsDeletedEvent()
            {
                UserId = request.UserId
            }.ToJson())
            .Build());
        
        return Ok(result);
    }

    [HttpPost("createVendor", Name = "Create Vendor")]
    [Authorize]
    public async Task<ActionResult<CreateVendorResponse>> CreateVendor(CreateVendorRequest request)
    {
        if (!User.HasBoolClaim(Permissions.WriteVendors))
            return Unauthorized("You do not have permission to create Vendors");
        
        var fullVendorId = $"vendor-{request.VendorId}";
        UserItems? existingVendorItems = await userItemsRepo.Get(fullVendorId);
        
        if (existingVendorItems is not null)
            return Conflict($"Vendor with ID '{fullVendorId}' already exists");

        var newVendorItems = new UserItems
        {
            Id = fullVendorId
        };
        
        UserItems? createdVendorItems = await userItemsRepo.Set(newVendorItems);
        if (createdVendorItems is null)
            return StatusCode(500, $"Could not create User Items for User with ID '{fullVendorId}'");
        
        return Ok(new CreateVendorResponse
        {
            UserId = fullVendorId
        });
    }
}