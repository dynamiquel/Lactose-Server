using System.Text;
using LactoseWebApp.Auth;
using LactoseWebApp.Service;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Formatter;

namespace LactoseWebApp.Mqtt;

public class MqttService(
    IMqttClient client,
    IOptions<MqttOptions> options,
    IServiceInfo serviceInfo,
    ILogger<MqttService> logger,
    //IMqttEnhancedAuthenticationHandler authenticationHandler,
    IApiAuthHandler authHandler) : IHostedService
{
    CancellationToken _cancellationToken;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        while (authHandler.AccessToken is null)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }
        
        var clientOptions = new MqttClientOptionsBuilder()
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithWillTopic($"service/{serviceInfo.Id}/offline")
            .WithTlsOptions(o => o.WithCertificateValidationHandler(
                // The used public broker sometimes has invalid certificates. This sample accepts all
                // certificates. This should not be used in live environments.
                _ => true))
            //.WithEnhancedAuthentication("JWT")
            //.WithEnhancedAuthenticationHandler(authenticationHandler); Cannot use Enhanced Auth as most brokers don't support it  :(
            .WithCredentials(new MqttCredentialsProvider(authHandler))
            .WithClientId($"{serviceInfo.Id}-{Guid.NewGuid().ToString("N")[..8]}");
        
        logger.LogInformation("Attempting to connect to MQTT broker at {IpAddress}:{IpPort}", 
            options.Value.ServerAddress, 
            options.Value.ServerPort);

        if (options.Value.WebSockets)
        {
            string url = StringExtensions.CombineUrlWithPort(options.Value.ServerAddress, options.Value.ServerPort);
            logger.LogInformation("MQTT WebSocket URL: {WebSocketUri}", url);
            clientOptions.WithWebSocketServer(c => c.WithUri($"wss://{url}"));
        }
        else
        {
            clientOptions.WithTcpServer(options.Value.ServerAddress, options.Value.ServerPort);
        }
        
        client.ConnectedAsync += OnConnected;
        client.DisconnectedAsync += OnDisconnected;
        client.ConnectingAsync += OnConnecting;
        client.ApplicationMessageReceivedAsync += OnMessageReceived;
        client.WithAutomaticReconnect(logger, cancellationToken);

        MqttClientConnectResult? result = await client.ConnectAsync(clientOptions.Build(), cancellationToken);
        if (result?.ResultCode != MqttClientConnectResultCode.Success)
        {
            if (result is not null)
                logger.LogError("Failed to connect to MQTT broker. Reason: {ReasonCode} - {ReasonStr}",
                    result.ResultCode,
                    result.ReasonString);
            else
                logger.LogError("Failed to connect to MQTT broker");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (client.IsConnected)
        {
            await client.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic($"/{serviceInfo.Id}/health/offline")
                .WithRetainFlag(false)
                .Build(), 
                cancellationToken);
        }
        
        await client.TryDisconnectAsync();
    }
    
    Task OnConnecting(MqttClientConnectingEventArgs args)
    {
        logger.LogInformation("Connecting to MQTT broker");
        return Task.CompletedTask;
    }

    async Task OnConnected(MqttClientConnectedEventArgs args)
    {
        logger.LogInformation("Connected to MQTT broker");
        
        await client.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic($"/{serviceInfo.Id}/health/online")
            .WithRetainFlag(false)
            .Build(), 
            _cancellationToken);
    }

    Task OnDisconnected(MqttClientDisconnectedEventArgs args)
    {
        logger.LogInformation("Disconnected from MQTT broker. Reason: {ReasonCode} - {ReasonStr}", 
            args.Reason, 
            args.ReasonString);
        
        return Task.CompletedTask;
    }
    
    Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        logger.LogDebug("Received message from MQTT broker.\nTopic: {Topic}.\nPayload: {Payload}", 
            args.ApplicationMessage.Topic,
            args.ApplicationMessage.Payload);
        
        return Task.CompletedTask;
    }
}