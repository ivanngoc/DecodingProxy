// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp.Tls;
using System.Collections.Concurrent;

namespace IziHardGames.Proxy
{
    public class ManagerForConnectionsToDomain
    {
        public ConcurrentDictionary<string, ConnectionsToDomainTls> items = new ConcurrentDictionary<string, ConnectionsToDomainTls>();

        // NOTE: NO DISPOSE IMPLEMENTED
        public ConnectionsToDomainTls GetOrCreate(StartOptions options, System.Security.Cryptography.X509Certificates.X509Certificate2 caRootCert)
        {
            string key = options.Host;

            if (items.TryGetValue(key, out var existed))
            {
                return existed;
            }
            else
            {
                var result = new ConnectionsToDomainTls();
                result.InitAsync(options, caRootCert);
                items.TryAdd(key, result);
                return result;
            }
        }
    }
}