using Lactose.Identity.Models;
using Lactose.Identity.Options;
using LactoseWebApp.Mongo;
using Microsoft.Extensions.Options;

namespace Lactose.Identity.Data.Repos;

public class MongoRolesRepo : MongoBasicKeyValueRepo<IRolesRepo, Role, RolesDatabaseOptions>, IRolesRepo
{
    public MongoRolesRepo(ILogger<IRolesRepo> logger, IOptions<RolesDatabaseOptions> databaseOptions) 
        : base(logger, databaseOptions) 
    { }
}