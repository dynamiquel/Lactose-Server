using Lactose.Economy.Data.Repos;
using Lactose.Economy.Mapping;
using Lactose.Economy.Options;
using Lactose.Economy.UserItems;
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
    IMqttClient mqttClient) : UserItemsControllerBase
{
    static bool IsValidEconomyUser(string userId) => userId.IsValidObjectId() || userId.StartsWith("vendor");

    [Authorize]
    public override async Task<ActionResult<QueryUserItemsResponse>> Query(QueryUserItemsRequest request)
    {
        ISet<string> foundItems = await userItemsRepo.Query();

        return new QueryUserItemsResponse
        {
            UserIds =  foundItems.ToList()
        };
    }

    [Authorize]
    public override async Task<ActionResult<GetUserItemsResponse>> Get(GetUserItemsRequest request)
    {
        if (!IsValidEconomyUser(request.UserId))
            return BadRequest($"UserId '{request.UserId}' is not a valid UserId");
        
        bool bCanRead = User.MatchesId(request.UserId) && User.HasBoolClaim(Permissions.ReadUserSelf) ||
                        User.HasBoolClaim(Permissions.ReadUserOthers);
        
        if (!bCanRead)
            return Unauthorized($"You do not have permission to read user's '{request.UserId}' items");

        Models.UserItems? foundUserItems = await userItemsRepo.Get(request.UserId);
        if (foundUserItems is null)
        {
            // TODO: Check that user exists with the provided ID before creating a User Items.
            
            // User Items don't exist for the specified user, create it with starting items.
            var newModel = new Models.UserItems
            {
                Id = request.UserId,
                // Not the most ideal place to do this. I would assume some other system would listen for some kind
                // of user created event and would initialise necessary user data?
                Items = userStartingItems.Value.StartingUserItems
            };

            Models.UserItems? createdModel = await userItemsRepo.Set(newModel);
            if (createdModel is null)
                return StatusCode(500, $"Could not create User Items for User with ID '{request.UserId}'");
            
            foundUserItems = createdModel;
        }

        return UserMapper.ToDto(foundUserItems);
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
        
        return result;
    }

    [HttpPost("createVendor", Name = "Create Vendor")]
    [Authorize]
    public override async Task<ActionResult<CreateVendorResponse>> CreateVendor(CreateVendorRequest request)
    {
        if (!User.HasBoolClaim(Permissions.WriteVendors))
            return Unauthorized("You do not have permission to create Vendors");
        
        var fullVendorId = $"vendor-{request.VendorId}";
        Models.UserItems? existingVendorItems = await userItemsRepo.Get(fullVendorId);
        
        if (existingVendorItems is not null)
            return Conflict($"Vendor with ID '{fullVendorId}' already exists");

        var newVendorItems = new Models.UserItems
        {
            Id = fullVendorId
        };
        
        Models.UserItems? createdVendorItems = await userItemsRepo.Set(newVendorItems);
        if (createdVendorItems is null)
            return StatusCode(500, $"Could not create User Items for User with ID '{fullVendorId}'");
        
        return new CreateVendorResponse
        {
            UserId = fullVendorId
        };
    }
}