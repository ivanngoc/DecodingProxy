using IziHardGames.Libs.Binary.Readers;
using System.Security.Authentication;
using System.Buffers;
using System.Security.Authentication;
using Enumerator = IziHardGames.Libs.Cryptography.Tls.Shared.TlsHelloFromClientExtensionsEnumerator;
using EnumeratorServer = IziHardGames.Libs.Cryptography.Tls.Shared.TlsHelloFromServerExtensionsEnumerator;
using System;
using System.Collections.Generic;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Tls;

namespace IziHardGames.Libs.Cryptography
{
    /// <summary>
    /// <see cref="Infos.TlsExtensionInfo"/>
    /// </summary>
    public class TlsExtensionInfoReusable : IDisposable, IPoolBind<TlsExtensionInfoReusable>
    {
        private IPoolReturn<TlsExtensionInfoReusable> pool;
        public ETlsExtensions type;
        public byte[] datas;
        public int length;
        public static TlsExtensionInfoReusable FromStruct(in Infos.TlsExtensionInfo extension)
        {
            var pool = PoolObjectsConcurent<TlsExtensionInfoReusable>.Shared;
            TlsExtensionInfoReusable info = pool.Rent();
            info.BindToPool(pool);
            info.Initilize(in extension);
            return info;
        }

        private void Initilize(in Infos.TlsExtensionInfo extension)
        {
            this.type = (ETlsExtensions)extension.type;
            datas = extension.data.ToArray();
            length = extension.length;
        }

        public void Dispose()
        {
            pool.Return(this);
            pool = default;
            type = default;
            datas = default;
        }

        public void BindToPool(IPoolReturn<TlsExtensionInfoReusable> pool)
        {
            this.pool = pool;
        }
    }

    public class HandshakeCertificateInfo
    {
        public ushort protocolVersion;
        public int length;
        public byte handshakeMessageType;
        public int length1;
        private int lengthCertificates;
        public List<byte[]> certificates = new List<byte[]>();

        public int AnalyzeAsClient(in ReadOnlyMemory<byte> mem)
        {
            var span = mem.Span;
            this.protocolVersion = BufferReader.ToUshort(span[1], span[2]);
            var length = BufferReader.ToUshort(span[3], span[4]);
            this.length = length + 5;
            this.handshakeMessageType = span[5];
            this.length1 = BufferReader.ToInt32(span[6], span[7], span[8]);
            this.lengthCertificates = BufferReader.ToInt32(span[9], span[10], span[11]);
            int lengthLeft = this.lengthCertificates;
            int i = 12;
            var certsSpan = span.Slice(i);

            while (lengthLeft > 0)
            {
                int lengthCert = BufferReader.ToInt32Size3(certsSpan);
                certsSpan = certsSpan.Slice(3);
                var cert = certsSpan.Slice(0, lengthCert);
                certificates.Add(cert.ToArray());
                certsSpan = certsSpan.Slice(lengthCert);
                lengthLeft -= lengthCert + 3;
            }
            return this.length;
        }
    }

    public class HandshakeHelloInfo
    {
        public bool isAlpnH3;
        public bool isAlpnH2;
        public bool isHttp11;
        public string protocolsString = string.Empty;
        public SslProtocols protocolsSsl;
        public int length;

        public List<TlsExtensionInfoReusable> extensions = new List<TlsExtensionInfoReusable>();

        [HandshakeAnalyz(Side = ESide.Server)]
        public void AnalyzAsServer(ReadOnlyMemory<byte> mem)
        {
            throw new System.NotImplementedException();
        }

        [HandshakeAnalyz(Side = ESide.Client)]
        public int AnalyzeAsClient(in HandshakeRecord clientHello, in ReadOnlyMemory<byte> mem)
        {
            var span = mem.Span;
            var length = clientHello.record.Length;
            ushort clientVersion = clientHello.record.ProtocolVersion;

            if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS13) this.protocolsSsl = SslProtocols.Tls13;
            else if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS12) this.protocolsSsl = SslProtocols.Tls12;
            else if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS11) this.protocolsSsl = SslProtocols.Tls11;
            else if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS10) this.protocolsSsl = SslProtocols.Tls11;
            else throw new System.NotImplementedException();

            //Console.WriteLine($"Client handshake size: 5+{length}");
            Enumerator tlsExtensions = new Enumerator(in clientHello, new ReadOnlySequence<byte>(mem));

            while (tlsExtensions.MoveNext())
            {
                var extension = tlsExtensions.Current;
                TlsExtensionInfoReusable info = TlsExtensionInfoReusable.FromStruct(extension);
                extensions.Add(info);
                //Console.WriteLine($"DEBUG: {(ETlsExtensions)extension.type}. Length:{extension.length}. DataAsString: {Encoding.UTF8.GetString(extension.data)}. Data Raw:{ParseByte.ToHexStringFormated(extension.data)}");

                if (extension.type == (ushort)(ETlsExtensions.APPLICATION_LAYER_PROTOCOL_NEGOTIATION))
                {
                    //h3 - HTTP/3 0x68 0x33
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.h3))
                    {
                        this.isAlpnH3 = true;
                        protocolsString += $"h3 ";
                    }
                    // https://www.rfc-editor.org/rfc/rfc7301.html#section-6
                    // 0x00 0x0C 0x02
                    // 0x68 0x32 - "h2". The string is serialized into an ALPN protocol identifier as the two-octet sequence: 0x68, 0x32. https://httpwg.org/specs/rfc9113.html#versioning
                    // 0x08 - горизонтальная табуляция
                    // 0x68 0x74 0x74 0x70 0x2f 0x31 0x2e 0x31 = "http/1.1"
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.h2))
                    {
                        this.isAlpnH2 = true;
                        protocolsString += $"h2 ";
                    }
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.http11))
                    {
                        this.isHttp11 = true;
                        protocolsString += $"http11 ";
                    }
                }
            }
            this.length = length + ConstantsForTls.SIZE_RECORD;
            return this.length;
        }

        public string ToStringInfo()
        {
            return $"protocolsSsl:{protocolsSsl}; Length:{length}; {protocolsString};";
        }
    }
}
