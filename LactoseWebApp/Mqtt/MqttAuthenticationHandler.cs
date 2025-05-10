using System.Text;
using LactoseWebApp.Auth;
using MQTTnet;
using MQTTnet.Protocol;

namespace LactoseWebApp.Mqtt;

public class MqttAuthenticationHandler(
    ILogger<MqttAuthenticationHandler> logger,
    IApiAuthHandler authHandler)
    : IMqttEnhancedAuthenticationHandler
{
    public async Task HandleEnhancedAuthenticationAsync(MqttEnhancedAuthenticationEventArgs eventArgs)
    {
        logger.LogInformation("Handling MQTT authentication...");

        if (eventArgs.ReasonCode is MqttAuthenticateReasonCode.Success)
        {
            logger.LogInformation("MQTT authentication successful");
            return;
        }
        
        logger.LogInformation("MQTT auth data received: {Data}", Encoding.UTF8.GetString(eventArgs.AuthenticationData));

        while (authHandler.AccessToken is null)
            await Task.Delay(TimeSpan.FromMilliseconds(100), eventArgs.CancellationToken);
        
        byte[] tokenBytes = Encoding.UTF8.GetBytes(authHandler.AccessToken.UnsafeToString());

        await eventArgs.SendAsync(
            new SendMqttEnhancedAuthenticationDataOptions { Data = tokenBytes },
            eventArgs.CancellationToken
        );
    }
}