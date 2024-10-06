using Lactose.Config.Dtos;
using Lactose.Config.Models;
using Riok.Mapperly.Abstractions;

namespace Lactose.Config.Mapping;

[Mapper]
public partial class ConfigEntryMapper
{
    public static partial ConfigEntryResponse ToDto(ConfigEntry model);
   
    public static IEnumerable<ConfigEntryResponse> ToDto(ICollection<ConfigEntry> models)
    {
        return models.Select(ToDto);
    }
    
    public static ConfigResponse ToConfigDto(ICollection<ConfigEntry> models)
    {
        var dto = new ConfigResponse();
        foreach (var model in models)
            dto.Entries.Add(model.Key, model.Value);

        return dto;
    }
    
    public static partial ConfigEntry ToModel(UpdateConfigEntryRequest dto);

    public static IEnumerable<ConfigEntry> ToModel(IEnumerable<UpdateConfigEntryRequest> dto)
    {
        return dto.Select(ToModel);
    }
}