using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lactose.Identity.Data.Repos;
using Lactose.Identity.Grpc;
using Lactose.Identity.Mapping;
using LactoseWebApp;

namespace Lactose.Identity.Services;

public class UsersService : Users.UsersBase
{
    readonly ILogger<UsersService> _logger;
    readonly IUsersRepo _usersRepo;
    readonly IRolesRepo _rolesRepo;

    public UsersService(ILogger<UsersService> logger, IUsersRepo usersRepo, IRolesRepo rolesRepo)
    {
        _logger = logger;
        _usersRepo = usersRepo;
        _rolesRepo = rolesRepo;
    }
    
    public override async Task<QueryUsersResponse> QueryUsers(QueryUsersRequest request, ServerCallContext context)
    {
        ISet<string> foundUsers = await _usersRepo.QueryUsers();

        return new QueryUsersResponse
        {
            UserIds = { foundUsers }
        };
    }

    public override async Task<UserResponse> GetUser(UserRequest request, ServerCallContext context)
    {
        var foundUser = await _usersRepo.GetUserById(request.UserId);
        
        if (foundUser is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"User with id {request.UserId} was not found."));

        return UserMapper.ToDto(foundUser);
    }

    public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var foundUser = await _usersRepo.GetUserById(request.UserId);
        if (foundUser is not null)
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"User with id {request.UserId} already exists."));
        
        var createdUser = await _usersRepo.CreateUser(request);
        if (createdUser is null)
            throw new RpcException(new Status(StatusCode.Unknown, $"Could not create user {request.UserId}"));
        
        return UserMapper.ToDto(createdUser);
    }

    public override async Task<Empty> DeleteUser(UserRequest request, ServerCallContext context)
    {
        var foundUser = await _usersRepo.GetUserById(request.UserId);
        if (foundUser is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"User with id {request.UserId} was not found."));

        var response = await _usersRepo.DeleteUserById(request.UserId);
        if (!response)
            throw new RpcException(new Status(StatusCode.Unknown, $"User with id {request.UserId} could not be deleted."));

        return new Empty();
    }

    public override async Task<Permissions> GetUserPermissions(UserRequest request, ServerCallContext context)
    {
        var foundUser = await _usersRepo.GetUserById(request.UserId);
        
        if (foundUser is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"User with id {request.UserId} was not found."));

        var userRoles = await _rolesRepo.GetRolesByIds(foundUser.Roles);
        
        var permissions = new HashSet<string>();
        foreach (var userRole in userRoles)
            permissions.Append(userRole.Permissions);
        
        return RoleMapper.ToDto(permissions);
    }
}