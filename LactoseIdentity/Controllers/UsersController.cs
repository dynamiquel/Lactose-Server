using Lactose.Identity.Data.Repos;
using Lactose.Identity.Dtos.Users;
using Lactose.Identity.Mapping;
using Lactose.Identity.Models;
using LactoseWebApp.Auth;
using LactoseWebApp.Mongo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Identity.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(
    ILogger<UsersController> logger,
    IUsersRepo usersRepo) 
    : ControllerBase
{
    [HttpPost("query", Name = "Query Users")]
    [Authorize]
    public async Task<ActionResult<QueryUsersResponse>> QueryUsers()
    {
        ISet<string> foundUsers = await usersRepo.Query();

        return Ok(new QueryUsersResponse
        {
            UserIds =  foundUsers.ToList()
        });
    }

    [HttpPost(Name = "Get User")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUser(UserRequest request)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(request.UserId, out _))
            return BadRequest($"UserId '{request.UserId}' is not a valid UserId");

        bool bCanRead = User.HasBoolClaim(Permissions.ReadOthers) ||
                        User.MatchesId(request.UserId) && User.HasBoolClaim(Permissions.ReadSelf);

        if (!bCanRead)
            return Unauthorized($"You cannot get information for user '{request.UserId}'");
        
        var foundUser = await usersRepo.Get(request.UserId);
        
        if (foundUser is null)
            return NotFound($"User with id '{request.UserId}' was not found");

        return Ok(UserMapper.ToDto(foundUser));
    }

    [HttpPost("create", Name = "Create User")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
    {
        if (!User.HasBoolClaim(Permissions.WriteOthers))
            return Unauthorized("You do not have permission to create a user");
        
        var newUser = new User
        {
            DisplayName = request.DisplayName,
            Roles = request.Roles.ToHashSet(),
            TimeCreated = DateTime.UtcNow
        };
        
        var createdUser = await usersRepo.Set(newUser);
        if (createdUser is null)
            return StatusCode(500, $"Could not create user with name '{request.DisplayName}'");
        
        return Ok(UserMapper.ToDto(createdUser));
    }

    [HttpPost("delete", Name = "Delete User")]
    [Authorize]
    public async Task<ActionResult> DeleteUser(UserRequest request)
    {
        if (!request.UserId.IsValidObjectId())
            return BadRequest($"UserId '{request.UserId}' is not a valid UserId");

        if (!User.HasBoolClaim(Permissions.WriteOthers))
            return Unauthorized("You do not have permission to delete a user");
        
        var foundUser = await usersRepo.Get(request.UserId);
        if (foundUser is null)
            return NotFound($"User with ID '{request.UserId}' was not found");

        var response = await usersRepo.Delete(request.UserId);
        if (!response)
            return StatusCode(500, $"User with ID '{request.UserId}' could not be deleted");

        return Ok();
    }
}