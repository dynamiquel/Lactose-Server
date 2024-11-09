using LactoseWebApp.Mongo;
using LactoseWebApp.Options;

namespace Lactose.Simulation.Options;

[Options]
public class CropsDatabaseOptions : MongoDatabaseOptions;

[Options(SectionName = "UserCrops:Database")]
public class UserCropsDatabaseOptions : MongoDatabaseOptions;