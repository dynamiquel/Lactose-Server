using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LactoseWebApp.Filters;

public class LogActionFilter(ILogger<LogActionFilter> logger) : IActionFilter
{
    readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };
    
    public void OnActionExecuting(ActionExecutingContext context)
    { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Logs the Action context and result as a JSON object for better debugging.
        
        var jsonObject = new JsonObject
        {
            { "Action", context.ActionDescriptor.DisplayName },
            { "Connection", new JsonObject
                {
                    { "Address", context.HttpContext.Connection.RemoteIpAddress?.ToString() },
                    { "Port", context.HttpContext.Connection.RemotePort }
                } 
            }
        };

        int? statusCode = null;
        if (context.Result is IStatusCodeActionResult statusResult)
        {
            statusCode = statusResult.StatusCode;
            jsonObject.Add("Status", statusResult.StatusCode);
        }
        
        if (context.Result is ObjectResult { Value: not null } objectResult)
            jsonObject.Add("Result", JsonSerializer.SerializeToNode(objectResult.Value));
        else
            jsonObject.Add("Result", JsonSerializer.SerializeToNode(context.Result));
        
        var logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Information;
        var jsonString = jsonObject.ToJsonString(_jsonSerializerOptions);
        
        logger.Log(logLevel, $"Action Executed: {jsonString}");
    }
}