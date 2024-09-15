using Lactose.Identity.Grpc;
using Lactose.Identity.Models;

namespace Lactose.Identity.Data.Repos;

public interface IUsersRepo
{
    Task<ISet<string>> QueryUsers();
    Task<User?> GetUserById(string userId);
    Task<User?> CreateUser(CreateUserRequest createUserRequest);
    Task<bool> DeleteUserById(string userId);
}