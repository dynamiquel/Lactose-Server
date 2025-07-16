using Lactose.Identity.Data.Repos;
using Lactose.Identity.Dtos.Roles;
using Lactose.Identity.Mapping;
using Lactose.Identity.Models;
using LactoseWebApp;
using LactoseWebApp.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;

namespace Lactose.Identity.Controllers;

[ApiController]
[Route("[controller]")]
public class RolesController(
    ILogger<RolesController> logger, 
    IRolesRepo rolesRepo,
    IMqttClient mqttClient) 
    : ControllerBase
{
    [HttpPost("query", Name = "Query Roles")]
    public async Task<IActionResult> QueryRoles()
    {
        var foundRoles = await rolesRepo.Query();
        return Ok(new QueryRolesResponse
        {
            RoleIds = foundRoles.ToList()
        });
    }

    [HttpPost(Name = "Get Roles")]
    public async Task<IActionResult> GetRoles(RolesRequest request)
    {
        //ICollection<Role> foundRoles = await rolesRepo.Get(request.RoleIds.ToHashSet());
        
        // Atm, the REST API does not support the concept of 'inheritedRoles' or 'all' permissions, so
        // let's do the conversion here.
        // This is quite inefficient but fuck it. It's temporary.
        ICollection<Role> allRoles = await rolesRepo.Get(await rolesRepo.Query());

        void FlattenPermissionsForRole(Role role)
        {
            if (role.InheritedRoles.IsEmpty())
                return;
            
            IEnumerable<Role> inheritedRoles = role.InheritedRoles.SelectMany(ir => allRoles.Where(r => r.Id == ir));
            foreach (var inheritedRole in inheritedRoles)
            {
                FlattenPermissionsForRole(inheritedRole);
                role.Permissions.AddRange(inheritedRole.Permissions);
            }
        }

        List<Role> desiredRoles = allRoles.Where(r => request.RoleIds.Contains(r.Id)).ToList();
        desiredRoles.ForEach(FlattenPermissionsForRole);
        return Ok(RoleMapper.ToDto(desiredRoles));
    }

    [HttpPost("create", Name = "Create Role")]
    [Authorize]
    public async Task<IActionResult> CreateRole(CreateRoleRequest request)
    {
        if (!User.HasBoolClaim(Permissions.WriteRoles))
            return Unauthorized("You do not have permission to create roles");
        
        var foundRole = await rolesRepo.Get(request.Id);
        if (foundRole is not null)
            return Conflict($"Role with ID '{request.Id}' already exists");
        
        var createdRole = await rolesRepo.Set(RoleMapper.ToModel(request));
        if (createdRole is null)
            return StatusCode(500, $"Could not create Role with ID '{request.Id}'");
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic("/identity/roles/created")
            .WithPayload(new RoleEvent { RoleIds = [ createdRole.Id ] }.ToJson())
            .Build());
        
        return Ok(RoleMapper.ToDto(createdRole));
    }

    [HttpPost("delete", Name = "Delete Roles")]
    [Authorize]
    public async Task<IActionResult> DeleteRoles(RolesRequest request)
    {
        if (!User.HasBoolClaim(Permissions.WriteRoles))
            return Unauthorized("You do not have permission to delete roles");
        
        var deletedRoles = await rolesRepo.Delete(request.RoleIds);
        
        await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic("/identity/roles/deleted")
            .WithPayload(new RoleEvent { RoleIds = deletedRoles.ToList() }.ToJson())
            .Build());
        
        return Ok(new QueryRolesResponse
        {
            RoleIds = deletedRoles.ToList()
        });
    }
}