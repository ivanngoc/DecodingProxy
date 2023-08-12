using System;
using System.Net.Security;
using System.Security.Authentication;

namespace IziHardGames.Libs.Networking.Options
{
    public class NetworkDefaults
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
}