using Grpc.Core;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Grpc;
using Lactose.Identity.Mapping;

namespace Lactose.Identity.Services;

public class RolesService : Roles.RolesBase
{
    readonly ILogger<RolesService> _logger;
    readonly IRolesRepo _rolesRepo;

    public RolesService(ILogger<RolesService> logger, IRolesRepo rolesRepo)
    {
        _logger = logger;
        _rolesRepo = rolesRepo;
    }
    
    public override async Task<QueryRolesResponse> QueryRoles(QueryRolesRequest request, ServerCallContext context)
    {
        ISet<string> foundRoles = await _rolesRepo.QueryRoles();

        return new QueryRolesResponse
        {
            RoleIds = { foundRoles }
        };
    }

    public override async Task<RolesResponse> GetRoles(RolesRequest request, ServerCallContext context)
    {
        var foundRoles = await _rolesRepo.GetRolesByIds(request.RoleIds.ToHashSet());

        return RoleMapper.ToDto(foundRoles);
    }

    public override async Task<RoleResponse> CreateRole(CreateRoleRequest request, ServerCallContext context)
    {
        var foundRole = await _rolesRepo.GetRolesByIds([request.RoleId]);
        if (foundRole is not null)
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Role with id {request.RoleId} already exists."));
        
        var createdRole = await _rolesRepo.CreateRole(RoleMapper.ToModel(request));
        if (createdRole is null)
            throw new RpcException(new Status(StatusCode.Unknown, $"Could not create role {request.RoleId}."));
        
        return RoleMapper.ToDto(createdRole);
    }

    public override async Task<QueryRolesResponse> DeleteRoles(RolesRequest request, ServerCallContext context)
    {
        var deletedRoles = await _rolesRepo.DeleteRoles(request.RoleIds);
        return new QueryRolesResponse
        {
            RoleIds = { deletedRoles }
        };
    }
}