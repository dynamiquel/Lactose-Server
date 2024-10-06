using Lactose.Config.Dtos;
using Lactose.Config.Models;
using Lactose.Config.Data.Repositories;
using Lactose.Config.Mapping;
using LactoseWebApp;
using Microsoft.AspNetCore.Mvc;

namespace Lactose.Config.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigController(IConfigRepo repo) : ControllerBase, IConfigController
{

    //[Authorize(Permission.Read)]
    [HttpGet("Entry", Name = "Get Entry")]
    //[Cache]
    public async Task<ActionResult<ConfigEntryResponse>> GetEntry(ConfigEntryRequest entryRequest)
    {
        ConfigEntry? entry = await repo.GetEntry(entryRequest);
        if (entry is null)
            return NotFound();
        
        return Ok(ConfigEntryMapper.ToDto(entry));
    }

    //[Authorize(Permission.Read)]
    [HttpGet("Entry/ID", Name = "Get Entry By ID")]
    //[Cache]
    public async Task<ActionResult<ConfigEntryResponse>> GetEntry(ConfigEntryByIdRequest entryRequest)
    {
        ConfigEntry? entry = await repo.GetEntryById(entryRequest.EntryId);
        if (entry is null)
            return NotFound();
        
        return Ok(ConfigEntryMapper.ToDto(entry));
    }

    //[Authorize(Permission.Read)]
    [HttpGet("Config", Name = "Get Config")]
    public async Task<ActionResult<ConfigResponse>> GetConfig(ConfigRequest? readRequest)
    {
        // Repo depends on a valid Read Request, so construct a default one, if necessary.
        
        var config = await repo.GetConfig(readRequest.ConstructIfNull());
        if (config.Count == 0)
            return NotFound();
        
        return Ok(ConfigEntryMapper.ToConfigDto(config));
    }

    //[Authorize(Permission.Write)]
    [HttpPut("Entry", Name = "Set Entry")]
    public async Task<ActionResult<ConfigEntryResponse>> SetEntry(UpdateConfigEntryRequest writeRequest)
    {
        var model = ConfigEntryMapper.ToModel(writeRequest);
        var postedEntry = await repo.SetEntry(model);
        if (postedEntry is null)
            return BadRequest();

        var readDto = ConfigEntryMapper.ToDto(postedEntry);
        return CreatedAtAction(nameof(GetEntry), new { entryId = readDto.Key }, readDto);
    }

    //[Authorize(Permission.Write)]
    [HttpPut("SetEntries", Name = "Set Entries")]
    public async Task<ActionResult<IEnumerable<ConfigEntryResponse>>> SetEntries(IEnumerable<UpdateConfigEntryRequest> writeRequest)
    {
        var models = ConfigEntryMapper.ToModel(writeRequest);
        var postedEntries = await repo.SetEntries(models);
        if (postedEntries.Count == 0)
            return BadRequest();

        var readDto = ConfigEntryMapper.ToDto(postedEntries);
        return Ok(readDto);
    }
    
    //[Authorize(Permission.Write)]
    [HttpDelete("Entry/ID", Name = "Delete Entry by ID")]
    public async Task<IActionResult> RemoveEntry(ConfigEntryByIdRequest entryRequest)
    {
        var deleted = await repo.RemoveEntry(entryRequest.EntryId);
        return deleted ? Ok() : BadRequest();
    }

    //[Authorize(Permission.Write)]
    [HttpDelete(Name = "Delete Entries")]
    public async Task<IActionResult> RemoveEntries(IEnumerable<string> removeRequest)
    {
        var deleted = await repo.RemoveEntries(removeRequest);
        return deleted ? Ok() : BadRequest();
    }

    //[Authorize(Permission.Write)]
    [HttpDelete(Name = "Delete All")]
    public async Task<IActionResult> Clear()
    {
        var deleted = await repo.Clear();
        return deleted ? Ok() : BadRequest();
    }
}