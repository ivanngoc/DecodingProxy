using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using IziHardGames.Libs.IO;
using IziHardGames.Proxy.TcpDecoder;

namespace IziHardGames.Libs.Cryptography.Tls12
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
                int readed = await networkStream.ReadAsync(frameHello, length, frameHello.Length - length);
                length += readed;
                TryAnalyze();
            }
        }

        protected bool TryAnalyze()
        {
            TlsHelloFromClient tlcHelloFromClient = new TlsHelloFromClient();
            TlsHelloFromClient.Read<IndexReaderForArray<byte>>(frameHello);
            return default;
        }
    }
}
