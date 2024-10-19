using Lactose.Config.Models;

namespace Lactose.Config.Dtos.Config;

public class ConfigEntryRequest
{
    public required string Key { get; set; }
    public ConfigEntryConditions? Conditions { get; set; } = default;
}

public class ConfigEntryByIdRequest
{
    public required string EntryId { get; set; }
}

public class ConfigEntryResponse
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
}

public class ConfigRequest
{
    public ConfigEntryConditions? Conditions { get; set; } = default;
}

public class ConfigResponse
{
    public Dictionary<string, string> Entries { get;} = new();
}

public class UpdateConfigEntryRequest
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public ConfigEntryConditions? Conditions { get; set; } = default;
}

public class DeleteConfigRequest
{
    public IEnumerable<string>? EntriesToRemove { get; set; } = default;
}