using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;

namespace Lactose.Identity.Data.Repos;

public class MongoRefreshTokensRepo : MongoBasicKeyValueRepo<IRefreshTokensRepo, RefreshToken, RefreshTokensDatabaseOptions>, IRefreshTokensRepo
{
    public MongoRefreshTokensRepo(ILogger<IRefreshTokensRepo> logger, IOptions<RefreshTokensDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) 
    { }
}