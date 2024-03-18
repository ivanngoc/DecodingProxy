using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.NodeProxies.Nodes.Tls
{

    internal class NodeTlsHandshakeFromClient : Node
    {
        private DataFragment? dataFragment;
        private TlsRecord tlsRecord;
        public void SetData(DataFragment dataFragment)
        {
            this.dataFragment = dataFragment;
        }
        internal override void Execute()
        {
            var slice = dataFragment!.ReadOnly;
            this.tlsRecord = BufferReader.ToStructConsume<TlsRecord>(ref slice);
            if (tlsRecord.TypeProtocol != ETlsProtocol.Handshake) throw new FormatException($"This node only process Handshake protool!");

            while (slice.Length > 0)
            {
                HandshakeHeader header = BufferReader.ToStructConsume<HandshakeHeader>(ref slice);
                var handshakePayload = BufferReader.Consume(header.Length, ref slice);

                Console.WriteLine($"Client HandshakeHeader type:{header.Type}");
                switch (header.Type)
                {
                    case ETlsTypeHandshakeMessage.HelloRequest: goto END;
                    case ETlsTypeHandshakeMessage.ClientHello:
                        {
                            ProtocolVersion protocolVersion = BufferReader.ToStructConsume<ProtocolVersion>(ref handshakePayload);
                            TlsRandom tlsRandom = BufferReader.ToStructConsume<TlsRandom>(ref handshakePayload);
                            TlsSessionId tlsSessionId = BufferReader.ToStructConsume<TlsSessionId>(ref handshakePayload);
                            ReadOnlyMemory<byte> sessionIdPayload = BufferReader.Consume(tlsSessionId.lengthFollowed, ref handshakePayload);
                            CipherSuite cipherSuite = BufferReader.ToStructConsume<CipherSuite>(ref handshakePayload);
                            ReadOnlyMemory<byte> cipherSuitePayload = BufferReader.Consume(cipherSuite.Length, ref handshakePayload);
                            CompressionMethod compressionMethod = BufferReader.ToStructConsume<CompressionMethod>(ref handshakePayload);
                            ReadOnlyMemory<byte> compressionMethodPayload = BufferReader.Consume(compressionMethod.lengthFollowed, ref handshakePayload);
                            TlsExtensions extensions = BufferReader.ToStructConsume<TlsExtensions>(ref handshakePayload);
                            ReadOnlyMemory<byte> extensionsPayload = BufferReader.Consume(extensions.Length, ref handshakePayload);

                            var extSlice = extensionsPayload;
                            Console.WriteLine($"Extensions Begin");
                            while (extSlice.Length > 0)
                            {
                                TlsExtension tlsExtension = BufferReader.ToStructConsume<TlsExtension>(ref extSlice);
                                ReadOnlyMemory<byte> payload = BufferReader.Consume(tlsExtension.Length, ref extSlice);
                                Console.WriteLine(tlsExtension.Type);
                            }
                            break;
                        }
                    case ETlsTypeHandshakeMessage.Certificate:
                        {
                            var payload = BufferReader.Consume(header.Length, ref handshakePayload);
                            break;
                        }
                    case ETlsTypeHandshakeMessage.CertificateVerify: break;
                    case ETlsTypeHandshakeMessage.ClientKeyExchange: break;
                    case ETlsTypeHandshakeMessage.Finished: break;
                    default: throw new ArgumentOutOfRangeException(header.Type.ToString());
                }
            }
            END: return;
        }
        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Sync;
        }

#if DEBUG
        internal static async Task Test()
        {
            string st = await File.ReadAllTextAsync("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\NodeProxy\\TestData\\Nutaku Handshake.hex");
            byte[] bytes = ParseByte.ToBytes(st);
            DataFragment fragment = new DataFragment(bytes);
            NodeTlsHandshakeFromClient node = new NodeTlsHandshakeFromClient();
            node.SetData(fragment);
            node.Execute();
        }
#endif
    }
}
