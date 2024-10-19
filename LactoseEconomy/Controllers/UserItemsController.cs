using Lactose.Economy.Data.Repos;
using Lactose.Economy.Dtos.UserItems;
using Lactose.Economy.Models;
using Lactose.Identity.Mapping;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Economy.Controllers;

[ApiController]
[Route("[controller]")]
public class UserItemsController(IUserItemsRepo userItemsRepo) : ControllerBase, IUserItemsController
{
    [HttpGet("query", Name = "Query User Items")]
    public async Task<ActionResult<QueryUserItemsResponse>> QueryUserItems(QueryUserItemsRequest request)
    {
        ISet<string> foundItems = await userItemsRepo.Query();

        return Ok(new QueryUserItemsResponse
        {
            UserIds =  foundItems.ToList()
        });
    }
    
    [HttpGet(Name = "Get User Items")]
    public async Task<ActionResult<GetUserItemsResponse>> GetUserItems(GetUserItemsRequest request)
    {
        if (!request.UserId.IsValidObjectId())
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

    [HttpDelete(Name = "Delete User Items")]
    public async Task<ActionResult<bool>> DeleteUserItems(GetUserItemsRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"UserId '{request.UserId}' is not a valid UserId");
        
        bool result = await userItemsRepo.Delete(request.UserId);
        return Ok(result);
    }
}