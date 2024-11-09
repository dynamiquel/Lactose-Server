using LactoseWebApp.Mongo;
using LactoseWebApp.Options;

namespace LactoseSimulation.Options;

[Options]
public class CropsDatabaseOptions : MongoDatabaseOptions;

[Options(SectionName = "UserCrops:Database")]
public class UserCropsDatabaseOptions : MongoDatabaseOptions;