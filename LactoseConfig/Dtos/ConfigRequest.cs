using Lactose.Config.Models;

namespace Lactose.Config.Dtos;

public class ConfigRequest
{
    public ConfigEntryConditions? Conditions { get; set; } = default;
}