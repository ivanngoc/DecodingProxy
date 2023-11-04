using System.IO;
using System.Net.Security;
using System.Threading.Tasks;
using IziHardGames.Libs.Streams;

namespace IziHardGames.Libs.Cryptography.Recording
{
    public class TlsProxyDecoder
    {
        public Stream clientToProxy;
        public Stream proxyToClient;

        public Stream proxyToOrigin;
        public Stream originToProxy;

        private SslServerAuthenticationOptions optionsServer;
        private SslClientAuthenticationOptions optionsClient;

        public async Task Decode()
        {
            var streamWrapProxyToClient = new StreamAdapted();
            var streamWrapProxyToOrigin = new StreamAdapted();

            SslStream sslProxyToClient = new SslStream(streamWrapProxyToClient);
            SslStream sslProxyToOrigin = new SslStream(streamWrapProxyToOrigin);

            var t1 = sslProxyToOrigin.AuthenticateAsServerAsync(optionsServer);
            var t2 = sslProxyToClient.AuthenticateAsClientAsync(optionsClient);

            await Task.WhenAll(t1, t2);
        }
    }
}