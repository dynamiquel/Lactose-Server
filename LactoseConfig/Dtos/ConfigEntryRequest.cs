using Lactose.Config.Models;

namespace Lactose.Config.Dtos;

public class ConfigEntryRequest
{
    public required string Key { get; set; }
    public ConfigEntryConditions? Conditions { get; set; } = default;
}