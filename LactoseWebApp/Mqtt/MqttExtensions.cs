using System.Buffers;
using System.Text.Json;
using System.Text.Json.Nodes;
using MQTTnet;
using MQTTnet.Formatter;

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
}