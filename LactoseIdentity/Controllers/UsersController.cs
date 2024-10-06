using Lactose.Identity.Data.Repos;
using Lactose.Identity.Dtos.Users;
using Lactose.Identity.Mapping;
using Lactose.Identity.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Identity.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(
    ILogger<UsersController> logger,
    IUsersRepo usersRepo) 
    : ControllerBase
{
    [HttpGet("query", Name = "Query Users")]
    public async Task<IActionResult> QueryUsers()
    {
        ISet<string> foundUsers = await usersRepo.QueryUsers();

        return Ok(new QueryUsersResponse
        {
            UserIds =  foundUsers.ToList()
        });
    }

    [HttpGet(Name = "Get User")]
    public async Task<IActionResult> GetUser(UserRequest request)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(request.UserId, out _))
            return BadRequest($"UserId '{request.UserId}' is not a valid UserId");
        
        var foundUser = await usersRepo.GetUserById(request.UserId);
        
        if (foundUser is null)
            return NotFound($"User with id '{request.UserId}' was not found");

        return Ok(UserMapper.ToDto(foundUser));
    }

    [HttpPost(Name = "Create User")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var newUser = new User
        {
            DisplayName = request.DisplayName,
            Roles = request.Roles.ToHashSet(),
            TimeCreated = DateTime.UtcNow
        };
        
        var createdUser = await usersRepo.CreateUser(newUser);
        if (createdUser is null)
            return StatusCode(500, $"Could not create user with name '{request.DisplayName}'");
        
        return Ok(UserMapper.ToDto(createdUser));
    }

    [HttpDelete(Name = "Delete User")]
    public async Task<IActionResult> DeleteUser(UserRequest request)
    {
        if (!MongoDB.Bson.ObjectId.TryParse(request.UserId, out _))
            return BadRequest($"UserId '{request.UserId}' is not a valid UserId");

        var foundUser = await usersRepo.GetUserById(request.UserId);
        if (foundUser is null)
            return NotFound($"User with ID '{request.UserId}' was not found");

        var response = await usersRepo.DeleteUserById(request.UserId);
        if (!response)
            return StatusCode(500, $"User with ID '{request.UserId}' could not be deleted");

        return Ok();
    }
}