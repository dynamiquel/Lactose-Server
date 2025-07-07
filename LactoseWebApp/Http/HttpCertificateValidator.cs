using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Globalization; 

namespace LactoseWebApp.Http;

public class HttpCertificateValidator(ILogger<HttpCertificateValidator> logger)
{
    public bool ValidateServerCertificate(
        X509Certificate2 certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        var errorDetails = new
        {
            Message = "Certificate validation failed",
            CertValidationDetails = new
            {
                PolicyErrors = sslPolicyErrors.ToString(), // String representation of SslPolicyErrors
                ServerCertificate = new
                {
                    Subject = certificate.Subject,
                    Issuer = certificate.Issuer,
                    Thumbprint = certificate.Thumbprint,
                    ExpirationDate = certificate.NotAfter.ToString("o", CultureInfo.InvariantCulture), // ISO 8601
                    HashAlgorithm = certificate.SignatureAlgorithm.FriendlyName,
                    PublicKeyAlgorithm = certificate.PublicKey.Oid.FriendlyName
                },
                Chain = chain.ChainElements
                    .Reverse() // Iterate from root to leaf for logical order
                    .Select((element, index) => new
                    {
                        Index = index, // Re-index for this order
                        Subject = element.Certificate.Subject,
                        Issuer = element.Certificate.Issuer,
                        ExpirationDate = element.Certificate.NotAfter.ToString("o", CultureInfo.InvariantCulture),
                        Status = element.ChainElementStatus.Select(s => s.Status.ToString()).ToList(),
                        StatusInformation = element.ChainElementStatus
                            .Select(s => s.StatusInformation)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList(),
                        IsUntrustedRoot = element.ChainElementStatus.Any(s => s.Status == X509ChainStatusFlags.UntrustedRoot)
                    })
                    .ToList()
            }
        };
        
        logger.LogError("{Message}:\n{Details}", errorDetails.Message, errorDetails.CertValidationDetails.ToIndentedJson());
        return false;
    }
}