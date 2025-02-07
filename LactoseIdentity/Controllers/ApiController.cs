using Lactose.Identity.Data.Repos;
using Lactose.Identity.Dtos.Apis;
using Lactose.Identity.Dtos.Users;
using Lactose.Identity.Mapping;
using Lactose.Identity.Models;
using LactoseWebApp.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Identity.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController(
    ILogger<ApiController> logger,
    IUsersRepo usersRepo,
    IPasswordHasher<User> passwordHasher) 
    : ControllerBase
{
    [HttpPost("create", Name = "Create API")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> CreateApi(CreateApiRequest request)
    {
        if (!User.HasBoolClaim(Permissions.WriteOthers))
            return Unauthorized("You do not have permission to create an API");
        
        var existingUser = await usersRepo.GetUserByEmail(request.ApiId);
        if (existingUser is not null)
            return Conflict($"API already exists with name '{request.DisplayName}'");
        
        var createdUser = await CreateApiUserInternal(request);
        if (createdUser is null)
            return StatusCode(500, $"Could not create API with name '{request.DisplayName}'");
        
        return Ok(UserMapper.ToDto(createdUser));
    }

    internal async Task<User?> CreateApiUserInternal(CreateApiRequest request)
    {
        request.Roles.Add("api");
        // Create a new role with the API ID. This makes it easier to assign per-API permissions.
        request.Roles.Add(request.ApiId);
        
        var newUser = new User
        {
            DisplayName = request.DisplayName,
            Roles = request.Roles.ToHashSet(),
            TimeCreated = DateTime.UtcNow,
            Email = request.ApiId
        };
        
        newUser.PasswordHash = passwordHasher.HashPassword(newUser, request.ApiPassword);
        
        var createdUser = await usersRepo.Set(newUser);
        return createdUser;
    }
}