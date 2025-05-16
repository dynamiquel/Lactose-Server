using LactoseWebApp.Auth;
using MQTTnet;

namespace LactoseWebApp.Mqtt;

public class MqttCredentialsProvider(IApiAuthHandler authHandler) : IMqttClientCredentialsProvider
{
    public byte[] GetPassword(MqttClientOptions clientOptions)
    {
        // Just needs to be non-empty due to some broker requirements.
        return [42];
    }

    public string GetUserName(MqttClientOptions clientOptions)
    {
        return authHandler.AccessToken?.EncodedToken ?? string.Empty;
    }
}