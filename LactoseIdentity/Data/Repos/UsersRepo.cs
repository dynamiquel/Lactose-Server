using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Lactose.Identity.Data.Repos;

public class MongoUsersRepo : MongoBasicKeyValueRepo<IUsersRepo, User, UsersDatabaseOptions>, IUsersRepo
{
    public MongoUsersRepo(ILogger<IUsersRepo> logger, IOptions<UsersDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) 
    { }

    public async Task<User?> GetUserByEmail(string email, CancellationToken ct)
    {
        Logger.LogInformation("Finding user by email: {Email}", email);

        User? user = await (await Collection.FindAsync(f => f.Email == email, cancellationToken: ct)).FirstOrDefaultAsync(cancellationToken: ct);
        return user;
    }
}