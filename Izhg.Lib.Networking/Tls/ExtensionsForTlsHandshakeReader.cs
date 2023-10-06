using System;
using System.Buffers;
using System.Security.Authentication;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Tls;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.Networking.States;
using Enumerator = IziHardGames.Libs.Cryptography.Tls.Shared.TlsHelloFromClientExtensionsEnumerator;
using EnumeratorServer = IziHardGames.Libs.Cryptography.Tls.Shared.TlsHelloFromServerExtensionsEnumerator;

namespace IziHardGames.Libs.Networking.Tls
{
    public static class ExtensionsForTlsHandshakeReader
    {
        [HandshakeAnalyz]
        public static HandshakeAnalyzResult AnalyzAsServer(this TlsHandshakeReadOperation reader)
        {
            HandshakeAnalyzResult result = default;
            var mem = reader.GetFrameAsMemory();
            var span = mem.Span;
            int length = BufferReader.ToUshort(span[3], span[4]);
            EnumeratorServer extensions = new EnumeratorServer(new ReadOnlySequence<byte>(mem));
            //Console.WriteLine($"Server handshake size: 5+{length}");

            while (extensions.MoveNext())
            {
                var extension = extensions.Current;
                //Console.WriteLine($"DEBUG: {(ETlsExtensions)extension.type}. Length:{extension.length}. DataAsString: {Encoding.UTF8.GetString(extension.data)}. Data Raw:{ParseByte.ToHexStringFormated(extension.data)}");

                if (extension.type == (ushort)(ETlsExtensions.APPLICATION_LAYER_PROTOCOL_NEGOTIATION))
                {
                    //h3 - HTTP/3 0x68 0x33
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.h3))
                    {
                        result.protocols |= ENetworkProtocols.HTTP3;
                    }
                    // https://www.rfc-editor.org/rfc/rfc7301.html#section-6
                    // 0x00 0x0C 0x02
                    // 0x68 0x32 - "h2". The string is serialized into an ALPN protocol identifier as the two-octet sequence: 0x68, 0x32. https://httpwg.org/specs/rfc9113.html#versioning
                    // 0x08 - горизонтальная табуляция
                    // 0x68 0x74 0x74 0x70 0x2f 0x31 0x2e 0x31 = "http/1.1"
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.h2))
                    {
                        result.protocols |= ENetworkProtocols.HTTP2;
                    }
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.http11))
                    {
                        result.protocols |= ENetworkProtocols.HTTP11;
                    }
                }
            }
            return result;
        }

        [HandshakeAnalyz]
        public static HandshakeAnalyzResult AnalyzAsClient(this TlsHandshakeReadOperation reader)
        {
            HandshakeAnalyzResult result = default;

            var mem = reader.GetFrameAsMemory();
            var span = mem.Span;
            var length = BufferReader.ToUshort(span[3], span[4]);
            short clientVersion = BufferReader.ToShort(span[9], span[10]);
            if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS13) result.protocolsSsl = SslProtocols.Tls13;
            else if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS12) result.protocolsSsl = SslProtocols.Tls12;
            else if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS11) result.protocolsSsl = SslProtocols.Tls11;
            else throw new System.NotImplementedException();

            //Console.WriteLine($"Client handshake size: 5+{length}");
            Enumerator tlsExtensions = new Enumerator(new ReadOnlySequence<byte>(mem));

            while (tlsExtensions.MoveNext())
            {
                var extension = tlsExtensions.Current;
                //Console.WriteLine($"DEBUG: {(ETlsExtensions)extension.type}. Length:{extension.length}. DataAsString: {Encoding.UTF8.GetString(extension.data)}. Data Raw:{ParseByte.ToHexStringFormated(extension.data)}");

                if (extension.type == (ushort)(ETlsExtensions.APPLICATION_LAYER_PROTOCOL_NEGOTIATION))
                {
                    //h3 - HTTP/3 0x68 0x33
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.h3))
                    {
                        result.protocols |= ENetworkProtocols.HTTP3;
                    }
                    // https://www.rfc-editor.org/rfc/rfc7301.html#section-6
                    // 0x00 0x0C 0x02
                    // 0x68 0x32 - "h2". The string is serialized into an ALPN protocol identifier as the two-octet sequence: 0x68, 0x32. https://httpwg.org/specs/rfc9113.html#versioning
                    // 0x08 - горизонтальная табуляция
                    // 0x68 0x74 0x74 0x70 0x2f 0x31 0x2e 0x31 = "http/1.1"
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.h2))
                    {
                        result.protocols |= ENetworkProtocols.HTTP2;
                    }
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.http11))
                    {
                        result.protocols |= ENetworkProtocols.HTTP11;
                    }
                }
            }
            return result;
        }
    }


    public struct HandshakeAnalyzResult
    {
        public ENetworkProtocols protocols;
        public SslProtocols protocolsSsl;
    }
}
