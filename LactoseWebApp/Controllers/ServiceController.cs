using System.Text;
using LactoseWebApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace LactoseWebApp.Controllers;

[Route("")]
[ApiController]
public class ServiceController(IServiceInfo serviceInfo) : ControllerBase
{
    [HttpGet]
    public IActionResult Home()
    {
        var sb = new StringBuilder(256);
        sb.AppendLine($"<!DOCTYPE html> <html> <head> <title>Aurora {serviceInfo.Name}</title> </head>")
            .AppendLine($"<h1>Lactose {serviceInfo.Name} is {serviceInfo.Status.ToString().ToLower()}!</h1>")
            .AppendLine().AppendLine($"{serviceInfo.Description}.")
            .AppendLine().AppendLine($"<h2>Dependencies ({serviceInfo.Dependencies.Length})</h2>")
            .AppendLine("<ul>");
        foreach (var dependency in serviceInfo.Dependencies)
            sb.AppendLine($"<li>{dependency}</li>");
        sb.AppendLine("</ul>").AppendLine("</body> </html>");

        return Content(sb.ToString(), "text/html");
    }
    
    [HttpGet("status", Name = "Status")]
    [HttpGet("info", Name = "Info")]
    public IActionResult GetInfo()
    {
        return Ok(serviceInfo);
    }
}