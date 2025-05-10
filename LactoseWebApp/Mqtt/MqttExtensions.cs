using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using MQTTnet;
using MQTTnet.Exceptions;

namespace LactoseWebApp.Mqtt;

public static class MqttExtensions
{
    public static IServiceCollection AddMqtt(this IServiceCollection services)
    {
        services.AddSingleton<MqttClientFactory>();

        // Creates the main MQTT client. Others can be made by using the above factory instead.
        services.AddSingleton<IMqttClient>(s =>
        {
            var factory = s.GetRequiredService<MqttClientFactory>();
            var client = factory.CreateMqttClient();
            return client;
        });

        services.AddHostedService<MqttService>();
        //services.AddSingleton<IMqttEnhancedAuthenticationHandler, MqttAuthenticationHandler>();
        
        return services;
    }

    public static JsonObject? ToJsonObject(this MqttApplicationMessage message)
    {
        if (message.Payload.IsEmpty)
            return null;

        try
        {
            if (message.Payload.IsSingleSegment)
                return JsonNode.Parse(message.Payload.FirstSpan) as JsonObject;
            
            byte[] payloadArray = message.Payload.ToArray();
            return JsonNode.Parse(payloadArray) as JsonObject;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static T? FromJson<T>(this MqttApplicationMessage message) where T : class
    {
        var jsonObject = message.ToJsonObject();
        return jsonObject?.Deserialize<T>();
    }
    
    public static object? FromJson(this MqttApplicationMessage message, Type type)
    {
        var jsonObject = message.ToJsonObject();
        return jsonObject?.Deserialize(type);
    }
    
    public static IMqttClient WithAutomaticReconnect(
        this IMqttClient client,
        ILogger? logger = null,
        Action<MqttClientOptions>? modifyOptionsAction = null,
        CancellationToken cancellationToken = default)
    {
        StrongBox<int> reconnectAttempts = new();
        
        // Define the disconnection handler
        async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            logger?.LogWarning(
                "MQTT client disconnected. Reason: {ReasonCode} - {ReasonString}. Client was connected: {ClientWasConnected}",
                args.Reason, args.ReasonString, args.ClientWasConnected);
            
             /*if (args.Reason is MqttClientDisconnectReason.NormalDisconnection)
             {
                 logger?.LogInformation("Graceful disconnect, automatic reconnect not initiated");
                 return;
             }*/

            if (cancellationToken.IsCancellationRequested)
            {
                logger?.LogInformation("Application shutdown requested, not attempting MQTT reconnect");
                return;
            }

            if (client.IsConnected)
            {
                logger?.LogInformation("MQTT client was already reconnected");
                reconnectAttempts.Value = 0;
                return;
            }

            reconnectAttempts.Value++;
            
            double delaySeconds = Math.Pow(2, reconnectAttempts.Value - 1);
            var delay = TimeSpan.FromSeconds(Math.Min(32, delaySeconds));
            
            logger?.LogInformation(
                "Attempting MQTT reconnect (Attempt #{Attempt}). Waiting for {DelaySeconds} seconds...",
                reconnectAttempts.Value, delay.TotalSeconds);
            
            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger?.LogInformation("MQTT reconnection delay cancelled");
                return;
            }
            
            if (client.IsConnected)
            {
                logger?.LogInformation("MQTT client was already reconnected");
                reconnectAttempts.Value = 0;
                return;
            }

            try
            {
                logger?.LogInformation("Executing MQTT ReconnectAsync...");

                modifyOptionsAction?.Invoke(client.Options);
                
                MqttClientConnectResult connectResult = await client.ConnectAsync(client.Options, cancellationToken);

                if (client.IsConnected)
                {
                    logger?.LogInformation("Successfully reconnected to MQTT broker");
                    reconnectAttempts.Value = 0;
                }
                else
                {
                    logger?.LogError("Failed to reconnect to MQTT broker. Result: {ResultCode}", 
                        connectResult.ResultCode);
                }
            }
            catch (TaskCanceledException)
            {
                logger?.LogInformation("MQTT ReconnectAsync was cancelled");
            }
            catch (MqttCommunicationException ex)
            {
                logger?.LogError(ex, "MQTT server not reachable (Attempt #{Attempt})", reconnectAttempts.Value);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Exception during MQTT ReconnectAsync (Attempt #{Attempt}):\n{Exception}", 
                    reconnectAttempts.Value, ex);
            }
        }
        
        client.DisconnectedAsync += HandleDisconnectedAsync;
        logger?.LogInformation("MQTT client configured with automatic reconnect logic.");

        return client;
    }
}