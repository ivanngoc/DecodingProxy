using System;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography.Tls;
using IziHardGames.Libs.Cryptography.Tls12;
using ServerExtensions = IziHardGames.Libs.Cryptography.Tls.Shared.TlsHelloFromServerExtensionsEnumerator;

namespace IziHardGames.Libs.Cryptography
{
    public readonly ref struct HandshakeHelloFromServerAnalyz
    {
        public static bool TryAnalyz(in HandshakeRecord serverHello, in ReadOnlyMemory<byte> payloadServerHello, out HandshakeHelloFromServerAnalyz result)
        {
            var mem = payloadServerHello;
            HandshakeHeader header = serverHello.handshakeHeader;
            mem = mem.Slice(ConstantsForTls.SIZE_HANDSHAKE_HEADER);
            var payload = mem.Slice(0, header.Length);
            REPEAT:
            if (mem.Length > 0)
            {
                switch (header.Type)
                {
                    case ETlsTypeHandshakeMessage.HelloRequest:
                        {
                            // Body is empty
                            goto REPEAT;
                        }
                    case ETlsTypeHandshakeMessage.ClientHello:
                        {

                            goto REPEAT;
                        }
                    case ETlsTypeHandshakeMessage.ServerHello:
                        {
                            //ProtocolVersion server_version;
                            //Random random;
                            //SessionID session_id;
                            //CipherSuite cipher_suite;
                            //CompressionMethod compression_method;
                            ServerHello hello = BufferReader.ToStruct<ServerHello>(in mem);
                            var sessionId = mem.Slice(0, hello.session_id.length);
                            mem = mem.Slice(hello.session_id.length);
                            CipherSuiteValue cipherSuite = BufferReader.ToStructConsume<CipherSuiteValue>(ref mem);
                            CompressionMethodValue compressionMethod = BufferReader.ToStructConsume<CompressionMethodValue>(ref mem);
                            // extensions
                            while (Infos.TlsExtensionInfo.TryReadConsume(ref mem, out var ext))
                            {
                                Console.WriteLine(ext.ToStringInfo());
                            }
                            goto REPEAT;
                        }
                    case ETlsTypeHandshakeMessage.Certificate: goto REPEAT;
                    case ETlsTypeHandshakeMessage.ServerKeyExchange: goto REPEAT;
                    case ETlsTypeHandshakeMessage.CertificateRequest: goto REPEAT;
                    case ETlsTypeHandshakeMessage.ServerHelloDone: goto REPEAT;
                    case ETlsTypeHandshakeMessage.CertificateVerify: goto REPEAT;
                    case ETlsTypeHandshakeMessage.ClientKeyExchange: goto REPEAT;
                    case ETlsTypeHandshakeMessage.Finished: goto REPEAT;
                    default: throw new System.NotImplementedException();
                }
                throw new NotImplementedException();
            }
            throw new System.NotImplementedException();
        }
    }
}
