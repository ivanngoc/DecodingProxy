using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using IziHardGames.Tls;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Cryptography.Defaults
{
    public class SslDefaults
    {
        public static SslClientAuthenticationOptions DefaultHttp2 => defaultHttp2.Value;

        private static readonly Lazy<SslClientAuthenticationOptions> defaultHttp2 = new Lazy<SslClientAuthenticationOptions>(() =>
        {
            return new SslClientAuthenticationOptions()
            {
                ApplicationProtocols = new System.Collections.Generic.List<SslApplicationProtocol>()
            {
                SslApplicationProtocol.Http2
            },
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11,
            };
        });
    }

    public class SslOptionsFactory
    {
        public static SslClientAuthenticationOptions CreateOptionsForClient(string host, SslProtocols clientProtocolsSsl, List<SslApplicationProtocol> alpnList)
        {
            return new SslClientAuthenticationOptions()
            {
                TargetHost = host,
                ApplicationProtocols = alpnList,
                EnabledSslProtocols = clientProtocolsSsl,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            };
        }

        public static async Task<SslServerAuthenticationOptions> CreateOptionsForServer(CertManager certManager, SslProtocols clientProtocolsSsl, List<SslApplicationProtocol> appProtocols, X509Certificate2 caCert, X509Certificate2 certOrigin)
        {
            return new SslServerAuthenticationOptions()
            {
                ServerCertificate = await certManager.ForgedGetOrCreateCertFromCacheAsync(certOrigin, caCert).ConfigureAwait(false),
                EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                EnabledSslProtocols = clientProtocolsSsl,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                ApplicationProtocols = appProtocols,
            };
        }
    }
}