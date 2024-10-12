using Lactose.Config.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Config.Controllers;

public interface IConfigController
{
    Task<ActionResult<ConfigEntryResponse>> GetEntry(ConfigEntryRequest entryRequest);
    Task<ActionResult<ConfigEntryResponse>> GetEntry(ConfigEntryByIdRequest entryRequest);
    Task<ActionResult<ConfigResponse>> GetConfig(ConfigRequest readRequest);
    Task<ActionResult<ConfigEntryResponse>> SetEntry(UpdateConfigEntryRequest writeRequest);
    Task<ActionResult<IEnumerable<ConfigEntryResponse>>> SetEntries(IEnumerable<UpdateConfigEntryRequest> writeRequest);
    Task<IActionResult> RemoveEntry(ConfigEntryByIdRequest entryRequest);
    Task<IActionResult> RemoveEntries(DeleteConfigRequest deleteRequest); 
}