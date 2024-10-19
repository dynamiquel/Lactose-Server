using Lactose.Config.Dtos.Config;
using Lactose.Config.Models;

namespace Lactose.Config.Data.Repositories;

public interface IConfigRepo
{
    Task<ConfigEntry?> GetEntry(ConfigEntryRequest entryRequest);
    Task<ConfigEntry?> GetEntryById(string entryId);
    Task<ICollection<ConfigEntry>> GetEntries(IEnumerable<ConfigEntryRequest> entryRequest);
    Task<ICollection<ConfigEntry>> GetConfig(ConfigRequest configRequest);
    Task<ConfigEntry?> SetEntry(ConfigEntry entryRequest);
    Task<ICollection<ConfigEntry>> SetEntries(IEnumerable<ConfigEntry> entryRequest);
    Task<bool> RemoveEntry(string entryId);
    Task<bool> RemoveEntries(IEnumerable<string> entryRequest);
    Task<bool> Clear();
}