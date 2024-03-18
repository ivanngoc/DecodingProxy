using System;
using System.Buffers;
using System.Threading.Tasks;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography;
using IziHardGames.Libs.Cryptography.Tls;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.Networking.Options;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Pools.Abstractions.NetStd21;
using Enumerator = IziHardGames.Libs.Cryptography.Tls.Shared.TlsHelloFromServerExtensionsEnumerator;

namespace IziHardGames.Libs.IO
{
    public class InterceptorTlsHandshakeServer : SocketReaderInterceptor, IPoolBind<InterceptorTlsHandshakeServer>
    {
        private IPoolReturn<InterceptorTlsHandshakeServer> pool;
        private ENetworkProtocols protocols;
        private readonly TlsHandshakeReadOperation tlsReader = new TlsHandshakeReadOperation();
        private bool isCopyOnIntercept;

        public ENetworkProtocols Protocols => protocols;

        public override void Initilize(SocketReader socketReader)
        {
            base.Initilize(socketReader);
            tlsReader.Initilize();
        }
        public override void Dispose()
        {
            base.Dispose();
            tlsReader.Dispose();
            isCopyOnIntercept = false;
        }
        public override EReadStatus Intercept(in ReadOnlySequence<byte> sequence)
        {
            throw new NotImplementedException();
        }

        public void EnableCopying()
        {
            this.isCopyOnIntercept = true;
        }

        public override EReadStatus Intercept(in Memory<byte> mem)
        {
            if (isCopyOnIntercept)
            {
                tlsReader.AddData(in mem);
                if (TlsHandshakeReadOperation.CheckIntegrity(tlsReader.GetBuffer()))
                {
                    return EReadStatus.PartialComplete;
                }
                return EReadStatus.PartialIncomplete;
            }
            return EReadStatus.None;
        }

        public void BindToPool(IPoolReturn<InterceptorTlsHandshakeServer> pool)
        {
            this.pool = pool;
        }


        public async Task AwaitFrame()
        {
            if (source is SocketReaderBuffered<SocketBufferDefault> reader)
            {
                while (true)
                {
                    await reader.ReadToBufferAsync();
                    var buf = reader.Buffer.GetBufferAsMemory();
                    if (TlsHandshakeReadOperation.CheckIntegrity(buf))
                    {
                        break;
                    }
                }
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        public void AnalyzFromCopy()
        {
            AnalyzFrame(tlsReader.GetBuffer());
        }

        public void AnalyzFrame(ReadOnlyMemory<byte> mem)
        {
            var span = mem.Length;
            Enumerator extensions = new Enumerator(new ReadOnlySequence<byte>(mem));

            while (extensions.MoveNext())
            {
                var extension = extensions.Current;
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
        public void AnalyzFrame()
        {
            if (source is SocketReaderBuffered<SocketBufferDefault> reader)
            {
                var mem = reader.Buffer.GetBufferAsMemory();
                AnalyzFrame(mem);
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
