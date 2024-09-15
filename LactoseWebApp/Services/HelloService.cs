using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lactose.Grpc;
using LactoseWebApp.Service;

namespace LactoseWebApp.Services;

public class HelloService : Hello.HelloBase
{
    readonly ILogger<HelloService> _logger;
    readonly IServiceInfo _serviceInfo;
    
    HelloService(ILogger<HelloService> logger, IServiceInfo serviceInfo)
    {
        _logger = logger;
        _serviceInfo = serviceInfo;
    }
    
    public override Task<HelloResponse> Hello(HelloRequest request, ServerCallContext context)
    {
        var now = DateTime.UtcNow;
        var latency = now.TimeOfDay - request.RequestTime.ToDateTime().TimeOfDay;
        _logger.LogInformation($"Hello from {request.ClientIdentifier} ({context.Peer}) in {latency.TotalMilliseconds}ms");

        var response = new HelloResponse()
        {
            RequestTime = request.RequestTime,
            ResponseTime = Timestamp.FromDateTime(now),
            ServiceName = _serviceInfo.Name
        };
        
        return Task.FromResult(response);
    }
}