using LactoseWebApp.Mongo;
using LactoseWebApp.Options;

namespace Lactose.Economy.Options;

[Options]
public class ItemsDatabaseOptions : MongoDatabaseOptions;

[Options]
public class TransactionsDatabaseOptions : MongoDatabaseOptions;

[Options(SectionName = "UserItems:Database")]
public class UserItemsDatabaseOptions : MongoDatabaseOptions;

[Options(SectionName = "ShopItems:Database")]
public class ShopItemsDatabaseOptions : MongoDatabaseOptions;