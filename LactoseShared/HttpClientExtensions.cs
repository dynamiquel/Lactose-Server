using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Lactose.Client;

public static class HttpClientExtensions
{
    public static async Task<T?> SendFromJson<T>(this HttpClient client, HttpRequestMessage request)
    {
        var response = await client.SendAsync(request).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return default;

        var content = await response.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
        return content;
    }

    public static IHttpClientBuilder DisableSslValidation(this IHttpClientBuilder builder)
    {
        return builder.ConfigurePrimaryHttpMessageHandler(_ =>
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            return handler;
        });
    }
}