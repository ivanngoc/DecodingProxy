using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Threading.Tasks;
using IziHardGames.Libs.Cryptography.Infos;
using IziHardGames.Libs.Cryptography.Readers.Tls12;
using static IziHardGames.Libs.Cryptography.Readers.Tls12.ParserForTls12;

namespace IziHardGames.Libs.Cryptography.Readers
{
    /// <summary>
    /// Frame Reader
    /// </summary>
    /// <see cref="TlsReader"/>
    public class TlsFrameReader
    {
        private readonly SslStream sslStream;
        private ReadOnlyMemory<byte> source;
        private ReadOnlyMemory<byte> slice;

        public TlsFrameReader(byte[] data)
        {
            sslStream = new SslStream(new MemoryStream(data));
        }
        public TlsFrameReader(ReadOnlyMemory<byte> bytes)
        {
            this.source = bytes;
            slice = bytes;
        }

        public TlsFrame ReadFrame()
        {
            throw new System.NotImplementedException();
        }
        public bool TryReadFrame(out TlsFrame frame)
        {
            if (ParserForTls12.TryParse(ref slice, out FrameParseResult parseResult))
            {
                frame = new TlsFrame(parseResult);
                return true;
            }
            frame = default;
            return false;
        }
        public ValueTask<TlsFrame> ReadFrameAsync()
        {
            throw new NotImplementedException();
        }
    }
}