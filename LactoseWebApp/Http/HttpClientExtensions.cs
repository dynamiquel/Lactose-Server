using System.Security.Cryptography.X509Certificates;

namespace LactoseWebApp.Http;

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

    public static IServiceCollection AddHttpClientFactory(this IServiceCollection services)
    {
        services
            .AddSingleton<HttpCertificateValidator>()
            .AddHttpClient()
            .ConfigureHttpClientDefaults(httpClientBuilder =>
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<HttpCertificateValidator>>();
                    var customValidator = serviceProvider.GetRequiredService<HttpCertificateValidator>();

                    return new SocketsHttpHandler
                    {
                        SslOptions =
                        {
                            RemoteCertificateValidationCallback = (sender, cert, chain, errors) =>
                            {
                                var x509Certificate = cert as X509Certificate2;

                                if (chain is null || x509Certificate is null)
                                {
                                    logger.LogError("SSL validation callback received null sender or certificate.");
                                    return false;
                                }

                                return customValidator.ValidateServerCertificate(
                                    x509Certificate,
                                    chain, 
                                    errors);
                            }
                        }
                    };
                });
            });
        
        return services;
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