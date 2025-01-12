using Lactose.Identity.Models;
using LactoseWebApp.Repo;

namespace Lactose.Identity.Data.Repos;

public interface IUsersRepo : IBasicKeyValueRepo<User>
{
    public Task<User?> GetUserByEmail(string email);
}