using Lactose.Config.Models;

namespace Lactose.Config.Dtos;

public class UpdateConfigEntryRequest
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public ConfigEntryConditions? Conditions { get; set; } = default;
}