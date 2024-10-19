using Lactose.Config.Models;

namespace Lactose.Config.Dtos.Config;

public class ConfigEntryRequest
{
    public required string Key { get; init; }
    public ConfigEntryConditions? Conditions { get; init; } = default;
}

public class ConfigEntryByIdRequest
{
    public required string EntryId { get; init; }
}

public class ConfigEntryResponse
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
}

public class ConfigRequest
{
    public ConfigEntryConditions? Conditions { get; init; } = default;
}

public class ConfigResponse
{
    public Dictionary<string, string> Entries { get; } = new();
}

public class UpdateConfigEntryRequest
{
    public required string Key { get; init; }
    public required string Value { get; init; }
    public ConfigEntryConditions? Conditions { get; init; } = default;
}

public class DeleteConfigRequest
{
    public IEnumerable<string>? EntriesToRemove { get; init; } = default;
}