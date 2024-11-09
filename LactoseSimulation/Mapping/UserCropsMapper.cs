using Lactose.Simulation.Dtos.UserCrops;
using Lactose.Simulation.Models;
using Riok.Mapperly.Abstractions;

namespace Lactose.Simulation.Mapping;

[Mapper]
public partial class UserCropsMapper
{
    public static partial GetUserCropsResponse? ToDto(UserCropInstances? crop);
}