using System;
using System.Buffers;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography;
using IziHardGames.Libs.Cryptography.Tls;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.Networking.Options;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Pools.Abstractions.NetStd21;
using Enumerator = IziHardGames.Libs.Cryptography.Tls.Shared.TlsHelloFromClientExtensionsEnumerator;

namespace IziHardGames.Libs.IO
{
    public class InterceptorTlsHandshakeClient : SocketReaderInterceptor, IPoolBind<InterceptorTlsHandshakeClient>
    {
        private IPoolReturn<InterceptorTlsHandshakeClient>? pool;
        private SslProtocols protocolsTls;
        private ENetworkProtocols protocols;
        public readonly TlsHandshakeReadOperation tlsReader = new TlsHandshakeReadOperation();
        private bool isCopyData;

        public SslProtocols ProtocolsSsl => protocolsTls;
        public ENetworkProtocols Protocols => protocols;

        public async Task AwaitFrame()
        {
            if (source is SocketReaderBuffered<SocketBufferDefault> reader)
            {
                var buffer = reader.Buffer;
                while (true)
                {
                    await reader.ReadToBufferAsync();
                    var mem = buffer.PeekAsReadOnlyMemory();
                    if (TlsHandshakeReadOperation.CheckIntegrity(in mem))
                    {
                        break;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }



        public override void Initilize(SocketReader socketReader)
        {
            base.Initilize(socketReader);
            tlsReader.Initilize();
        }

        public void BindToPool(IPoolReturn<InterceptorTlsHandshakeClient> pool)
        {
            this.pool = pool;
        }
        public override void Dispose()
        {
            base.Dispose();
            pool!.Return(this);
            pool = default;
            protocolsTls = SslProtocols.None;
            protocols = ENetworkProtocols.None;
            tlsReader.Dispose();
            isCopyData = default;
        }

        public override EReadStatus Intercept(in ReadOnlySequence<byte> sequence)
        {
            throw new System.NotImplementedException();
        }
        public override EReadStatus Intercept(in Memory<byte> mem)
        {
            if (isCopyData)
            {
                tlsReader.AddData(in mem);

                if (!tlsReader.CheckIntegrity())
                {
                    return EReadStatus.Blocking;
                }
                AnalyzFrame();

                return EReadStatus.Complete;
            }
            else return EReadStatus.None;
        }

        public void AnalyzFrame()
        {
            if (source is SocketReaderBuffered<SocketBufferDefault> reader)
            {
                var buffer = reader.Buffer;
                var mem = buffer.GetBufferAsMemory();
                var span = mem.Span;
                var length = BufferReader.ToUshort(span[3], span[4]);
                short clientVersion = BufferReader.ToShort(span[9], span[10]);
                if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS13) protocolsTls = SslProtocols.Tls13;
                else if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS12) protocolsTls = SslProtocols.Tls12;
                else if (clientVersion == ConstantsForTls.CLIENT_VERSION_TLS11) protocolsTls = SslProtocols.Tls11;
                else throw new System.NotImplementedException();

                Enumerator tlsExtensions = new Enumerator(new ReadOnlySequence<byte>(mem));

                while (tlsExtensions.MoveNext())
                {
                    var extension = tlsExtensions.Current;
                    Console.WriteLine($"DEBUG: {(ETlsExtensions)extension.type}. Length:{extension.length}. DataAsString: {Encoding.UTF8.GetString(extension.data)}. Data Raw:{ParseByte.ToHexStringFormated(extension.data)}");

                    if (extension.type == (ushort)(ETlsExtensions.APPLICATION_LAYER_PROTOCOL_NEGOTIATION))
                    {
                        //h3 - HTTP/3 0x68 0x33
                        if (extension.data.ContainSequence(ConstantsForTls.ALPN.h3))
                        {
                            protocols |= ENetworkProtocols.HTTP3;
                        }
                        // https://www.rfc-editor.org/rfc/rfc7301.html#section-6
                        // 0x00 0x0C 0x02
                        // 0x68 0x32 - "h2". The string is serialized into an ALPN protocol identifier as the two-octet sequence: 0x68, 0x32. https://httpwg.org/specs/rfc9113.html#versioning
                        // 0x08 - горизонтальная табуляция
                        // 0x68 0x74 0x74 0x70 0x2f 0x31 0x2e 0x31 = "http/1.1"
                        if (extension.data.ContainSequence(ConstantsForTls.ALPN.h2))
                        {
                            protocols |= ENetworkProtocols.HTTP2;
                        }
                        if (extension.data.ContainSequence(ConstantsForTls.ALPN.http11))
                        {
                            protocols |= ENetworkProtocols.HTTP11;
                        }
                    }
                }
            }
        }
    }
}
