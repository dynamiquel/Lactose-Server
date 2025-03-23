using MQTTnet;
using MQTTnet.Formatter;

namespace LactoseWebApp.Mqtt;

public static class MqttExtensions
{
    public static IServiceCollection AddMqtt(this IServiceCollection services)
    {
        services.AddSingleton<IMqttClient>(_ =>
        {
            var factory = new MqttClientFactory();
            var client = factory.CreateMqttClient();
            return client;
        });

        services.AddHostedService<MqttService>();
        
        return services;
    }
}