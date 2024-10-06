using System.Collections.Generic;

namespace Lactose.Config.Dtos;

public class ConfigResponse
{
    public Dictionary<string, string> Entries { get;} = new();
}