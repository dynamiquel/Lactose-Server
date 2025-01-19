using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Lactose.Economy.Data.Repos;
using Lactose.Economy.Dtos.UserItems;
using Lactose.Economy.Models;
using Lactose.Economy.Mapping;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Economy.Controllers;

[ApiController]
[Route("[controller]")]
public class UserItemsController(IUserItemsRepo userItemsRepo) : ControllerBase, IUserItemsController
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

        UserItems? foundUserItems = await userItemsRepo.Get(request.UserId);
        if (foundUserItems is null)
        {
            // TODO: Check that user exists with the provided ID before creating a User Items.
            
            // User Items don't exist for the specified user, create it.
            var newModel = new UserItems
            {
                Id = request.UserId
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
        
        if (request.UserId != User.FindFirstValue(JwtRegisteredClaimNames.Sub))
            return Unauthorized("You cannot delete items of another user");
        
        bool result = await userItemsRepo.Delete(request.UserId);
        return Ok(result);
    }

    [HttpPost("createVendor", Name = "Create Vendor")]
    [Authorize]
    public async Task<ActionResult<CreateVendorResponse>> CreateVendor(CreateVendorRequest request)
    {
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