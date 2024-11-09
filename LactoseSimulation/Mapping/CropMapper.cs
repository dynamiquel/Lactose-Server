using Lactose.Simulation.Dtos.Crops;
using Lactose.Simulation.Models;
using Riok.Mapperly.Abstractions;

namespace Lactose.Simulation.Mapping;

[Mapper]
public partial class CropMapper
{
    public static partial GetCropResponse ToDto(Crop crop);

    public static GetCropsResponse ToDto(ICollection<Crop> crops)
    {
        return new GetCropsResponse
        {
            Crops = crops.Select(ToDto).ToList()
        };
    }

}