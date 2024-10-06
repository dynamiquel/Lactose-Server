namespace Lactose.Config.Models;

public class Config
{
    public string EnvironmentId { get; set; } = default!;
    public Dictionary<string, byte[]> Entries { get; } = new();
}