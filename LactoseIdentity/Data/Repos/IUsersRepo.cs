using Lactose.Identity.Dtos.Users;
using Lactose.Identity.Models;

namespace Lactose.Identity.Data.Repos;

public interface IUsersRepo
{
    Task<ISet<string>> QueryUsers();
    Task<User?> GetUserById(string userId);
    Task<User?> CreateUser(User user);
    Task<bool> DeleteUserById(string userId);
}