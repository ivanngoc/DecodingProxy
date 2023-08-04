using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using IziHardGames.Proxy.TcpDecoder;

namespace IziHardGames.Libs.IO
{
    // https://tls12.xargs.org/#client-hello
    public class TlsReader12 : TlsReader
    {

        public bool TryAnalyzeTlsFrame(byte[] buffer, int offset, int length)
        {
            Array.Copy(buffer, offset, frameHello, this.length, length);
            this.length += length;
            return TryAnalyze();
        }

        public async Task TryAnalyzeTlsFrameAsync(NetworkStream networkStream)
        {
            while (true)
            {
                int readed = await networkStream.ReadAsync(frameHello, this.length, frameHello.Length - this.length);
                this.length += readed;
                TryAnalyze();
            }
        }

        protected bool TryAnalyze()
        {
            TlcHelloFromClient tlcHelloFromClient = new TlcHelloFromClient();
            TlcHelloFromClient.Read<IndexReaderForArray<byte>>(this.frameHello);
            return default;
        }
    }
}
