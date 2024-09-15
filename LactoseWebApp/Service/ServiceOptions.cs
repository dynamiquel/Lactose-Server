using LactoseWebApp.Options;

namespace LactoseWebApp.Service;

[Options]
public class ServiceOptions
{
    public string ServiceName { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Version { get; set; } = default;
    public string[] Dependencies { get; set; } = default!;
}