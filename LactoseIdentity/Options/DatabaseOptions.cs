using LactoseWebApp.Mongo;
using LactoseWebApp.Options;

namespace Lactose.Identity.Options;

[Options]
public class RolesDatabaseOptions : MongoDatabaseOptions;

[Options]
public class UsersDatabaseOptions : MongoDatabaseOptions;