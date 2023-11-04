using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using IziHardGames.Libs.Cryptography.Readers;
using IziHardGames.Proxy.TcpDecoder;

namespace IziHardGames.Libs.Cryptography.Tls12
{
    // https://tls12.xargs.org/#client-hello
    public class TlsReader12 : TlsReader
    {

        public bool TryAnalyzeTlsFrame(byte[] buffer, int offset, int length)
        {
            Array.Copy(buffer, offset, frame, this.offset, length);
            this.offset += length;
            return TryAnalyze();
        }

        public async Task TryAnalyzeTlsFrameAsync(NetworkStream networkStream)
        {
            while (true)
            {
                int readed = await networkStream.ReadAsync(frame, offset, frame.Length - offset);
                offset += readed;
                TryAnalyze();
            }
        }

        protected bool TryAnalyze()
        {
            TlsHelloFromClient tlcHelloFromClient = new TlsHelloFromClient();
            TlsHelloFromClient.Read<IndexReaderForArray<byte>>(frame);
            return default;
        }
    }
}
