using LactoseWebApp.Mongo;
using LactoseWebApp.Options;

namespace Lactose.Economy.Options;

[Options]
public class ItemsDatabaseOptions : MongoDatabaseOptions;

[Options]
public class TransactionsDatabaseOptions : MongoDatabaseOptions;

[Options]
public class UserItemsDatabaseOptions : MongoDatabaseOptions;