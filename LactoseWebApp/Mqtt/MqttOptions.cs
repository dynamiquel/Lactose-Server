using LactoseWebApp.Options;

namespace LactoseWebApp.Mqtt;

[Options]
public class MqttOptions
{
    public required string ServerAddress { get; set; } = "lactose.mookrata.ovh/mqtt";
    public required int ServerPort { get; set; } = 443;
    public bool WebSockets { get; set; } = true;
}