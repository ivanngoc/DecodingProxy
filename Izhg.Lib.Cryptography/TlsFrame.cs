using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using IziHardGames.Libs.Cryptography.Readers;
using IziHardGames.Libs.Cryptography.Readers.Tls12;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.Libs.Cryptography.Infos
{
    /// <summary>
    /// Единица передачи протокола Tls
    /// </summary>
    /// <see cref="TlsReader"/>
    /// <see cref="TlsFrameReader"/>
    public class TlsFrame : IDisposable
    {
        public ETlsTypeRecord type;
        public int lengthTotal;
        public TlsRecord record;
        public ReadOnlyMemory<byte> data;

        public TlsFrame(ParserForTls12.FrameParseResult parseResult)
        {
            type = parseResult.record.TypeRecord;
            lengthTotal = parseResult.record.Length + ConstantsForTls.SIZE_RECORD;
            this.record = parseResult.record;
            this.data = parseResult.payload;
        }
        public void Dispose()
        {
            data = default;
        }
    }

    /// <summary>
    /// Datas About Session Between Server and Client
    /// </summary>
    public class TlsSession
    {
        public SslApplicationProtocol alpn;
        public readonly TlsSessionClient client = new TlsSessionClient();
        public readonly TlsSessionServer server = new TlsSessionServer();
    }
    public class TlsSessionClient
    {
        internal string payloadAlpn;
    }
    public class TlsSessionServer
    {
        public X509Certificate2 cert;
        internal string payloadAlpn;
    }
}