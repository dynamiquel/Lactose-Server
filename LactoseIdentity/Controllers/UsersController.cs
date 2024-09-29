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
    [HttpHead(Name = "Query Users")]
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
        var foundUser = await usersRepo.GetUserById(request.UserId);
        
        if (foundUser is null)
        {
            logger.LogError($"User with id {request.UserId} was not found.");
            return NotFound($"User with id {request.UserId} was not found.");
        }

        return Ok(UserMapper.ToDto(foundUser));
    }

    [HttpPut(Name = "Create User")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var foundUser = await usersRepo.GetUserById(request.UserId);
        if (foundUser is not null)
        {
            logger.LogError($"User with id {request.UserId} already exists.");
            return Conflict($"User with id {request.UserId} already exists.");
        }

        var newUser = new User
        {
            UserId = request.UserId,
            Username = request.Username,
            Roles = request.Roles.ToHashSet(),
            TimeCreated = DateTime.UtcNow
        };
        
        var createdUser = await usersRepo.CreateUser(newUser);
        if (createdUser is null)
        {
            logger.LogError($"Could not create user {request.UserId}.");
            return StatusCode(500, $"Could not create user {request.UserId}.");
        }
        
        return Ok(UserMapper.ToDto(createdUser));
    }

    [HttpDelete("Delete User")]
    public async Task<IActionResult> DeleteUser(UserRequest request)
    {
        var foundUser = await usersRepo.GetUserById(request.UserId);
        if (foundUser is null)
        {
            logger.LogError($"User with id {request.UserId} was not found.");
            return NotFound($"User with id {request.UserId} was not found.");

        }
        var response = await usersRepo.DeleteUserById(request.UserId);
        if (!response)
        {
            logger.LogError($"User with id {request.UserId} could not be deleted.");
            return StatusCode(500, $"User with id {request.UserId} could not be deleted.");
        }

        return Ok();
    }
}