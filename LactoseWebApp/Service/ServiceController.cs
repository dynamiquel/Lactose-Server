using Microsoft.AspNetCore.Mvc;

namespace LactoseWebApp.Service;

[Route("")]
[ApiController]
public class ServiceController(IServiceInfo serviceInfo) : Controller
{
    [HttpGet]
    public IActionResult HomePage() => View(serviceInfo);

    [HttpGet("status", Name = "Status")]
    [HttpGet("info", Name = "Info")]
    public IActionResult GetInfo()
    {
        return Ok(serviceInfo);
    }
}