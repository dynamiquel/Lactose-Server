using LactoseWebApp.Mongo;
using LactoseWebApp.Options;

namespace Lactose.Tasks.Options;

[Options]
public class TasksDatabaseOptions : MongoDatabaseOptions;

[Options(SectionName = "UserTasks:Database")]
public class UserTasksDatabaseOptions : MongoDatabaseOptions;