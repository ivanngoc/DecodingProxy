using System.IO.Pipelines;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Pipelines.Wraps
{
    public class TcpWrapPipedSsl : TcpWrapPiped
    {
        private SslStream sslStream;
        private PipeReader readerSsl;

        private void InitSsl()
        {
            sslStream = new SslStream(client.GetStream());
            readerSsl = PipeReader.Create(sslStream);
        }

        public async Task AuthAsClientAsync()
        {
            await sslStream.AuthenticateAsClientAsync(host).ConfigureAwait(false);
        }
        public async Task AuthAsServerAsync(X509Certificate2 serverCert)
        {
            await sslStream.AuthenticateAsServerAsync(serverCert).ConfigureAwait(false);
        }

        public override void Dispose()
        {
            base.Dispose();
            sslStream.Dispose();
            sslStream = default;
        }
    }
}
