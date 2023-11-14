using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Defaults;
using IziHardGames.Libs.Cryptography.Delegates;
using IziHardGames.Libs.Cryptography.Infos;
using IziHardGames.Libs.Cryptography.Readers;
using IziHardGames.Libs.Cryptography.Readers.Tls12;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography.Tls;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.Streams;
using IziHardGames.Tls;
using static IziHardGames.Libs.Cryptography.Readers.Tls12.ParserForTls12;

namespace IziHardGames.Libs.Cryptography.Recording
{

    /// <summary>
    /// Play Recorded Raw Data
    /// </summary>
    public class TlsPlayerWithProxy
    {
        private string host;
        private ReadOnlyMemory<byte> dataProxyReadFromClient;
        private ReadOnlyMemory<byte> dataProxyWriteToClient;
        private ReadOnlyMemory<byte> dataProxyWriteToOrigin;
        private ReadOnlyMemory<byte> dataProxyReadFromOrigin;

        private TlsFrameReader readerProxyReadFromClient;
        private TlsFrameReader readerProxyWriteToClient;
        private TlsFrameReader readerProxyWriteToOrigin;
        private TlsFrameReader readerProxyReadFromOrigin;

        private TlsSession session;
        private readonly List<TlsFrame> framesProxyReadFromClient = new List<TlsFrame>();
        private readonly List<TlsFrame> framesProxySendToClient = new List<TlsFrame>();
        private readonly List<TlsFrame> framesProxySendToOrigin = new List<TlsFrame>();
        private readonly List<TlsFrame> framesProxyReadFromOrigin = new List<TlsFrame>();

        private readonly Dictionary<ushort, TlsHandlerForHandshakeExtensions> handlersForExtensions = new Dictionary<ushort, TlsHandlerForHandshakeExtensions>();
        private readonly Dictionary<byte, TlsHandlerForHandshake> handlersHandshakeTls = new Dictionary<byte, TlsHandlerForHandshake>();

        public TlsPlayerWithProxy(string host,
            ReadOnlyMemory<byte> proxyReadFromClient, ReadOnlyMemory<byte> proxyWriteToClient,
            ReadOnlyMemory<byte> dataProxyWriteToOrigin, ReadOnlyMemory<byte> dataProxyReadFromOrigin)
        {
            this.host = host;
            readerProxyReadFromClient = new TlsFrameReader(proxyReadFromClient);
            readerProxyWriteToClient = new TlsFrameReader(proxyWriteToClient);

            readerProxyWriteToOrigin = new TlsFrameReader(dataProxyWriteToOrigin);
            readerProxyReadFromOrigin = new TlsFrameReader(dataProxyReadFromOrigin);

            this.dataProxyReadFromClient = proxyReadFromClient;
            this.dataProxyWriteToClient = proxyWriteToClient;
            this.dataProxyWriteToOrigin = dataProxyWriteToOrigin;
            this.dataProxyReadFromOrigin = dataProxyReadFromOrigin;

            handlersForExtensions.Add((ushort)ETlsExtensions.APPLICATION_LAYER_PROTOCOL_NEGOTIATION, HandleExtAlpn);
            handlersForExtensions.Add((ushort)ETlsExtensions.client_certificate_url, HandleExtClientCertificateUrl);

            handlersHandshakeTls.Add((byte)ETlsTypeHandshakeMessage.Certificate, HandleHandshakeCert);
        }

        private void HandleHandshakeCert(in ReadOnlyMemory<byte> payload, ESide side, object container)
        {
            X509Certificate2 x509Certificate2 = new X509Certificate2(payload.Span);
            if (side == ESide.Server)
            {
                TlsSessionServer cont = (TlsSessionServer)container;
                cont.certs.Add(x509Certificate2);
            }
            else if (side == ESide.Client)
            {
                TlsSessionClient cont = (TlsSessionClient)container;
                cont.certs.Add(x509Certificate2);
            }
            else throw new System.NotImplementedException();
        }

        private void HandleExtClientCertificateUrl(TlsExtension ext, in ReadOnlyMemory<byte> data, ESide side, object container)
        {
            return;
            var slice = data;
            ExtDataClientCertificateUrl extDataClientCertificateUrl = BufferReader.ToStructConsume<ExtDataClientCertificateUrl>(ref slice);
            ReadOnlyMemory<byte> payload = BufferReader.Consume(extDataClientCertificateUrl.UrlAndHash.length, ref slice);

            if (side == ESide.Client)
            {
                string s = Encoding.UTF8.GetString(payload.Span);
                Console.WriteLine(extDataClientCertificateUrl.ToStringInfo() + '\t' + s);
            }
            else
            {
                throw new System.NotSupportedException("This extension Supose to be from client only?");
            }
        }

        /// <summary>
        /// https://www.rfc-editor.org/rfc/rfc7301.html#section-3.1
        /// <see cref="ETlsExtensions.APPLICATION_LAYER_PROTOCOL_NEGOTIATION"/>
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="data"></param>
        /// <param name="side"></param>
        private void HandleExtAlpn(TlsExtension ext, in ReadOnlyMemory<byte> data, ESide side, object sessionContainer)
        {
            var slice = data;
            ExtDataAlpnProtocolList list = BufferReader.ToStructConsume<ExtDataAlpnProtocolList>(ref slice);
            var payloadList = BufferReader.Consume(list.Length, ref slice);

            while (payloadList.Length > 0)
            {
                ExtDataAlpnProtocolItem item = BufferReader.ToStructConsume<ExtDataAlpnProtocolItem>(ref payloadList);
                var payloadItem = BufferReader.Consume(item.length, ref payloadList);
                var span = payloadItem.Span;
                var value = Encoding.UTF8.GetString(span);
                /// <see cref="ConstantsForTls.ALPN.h2"/>
                if (side == ESide.Client)
                {
                    TlsSessionClient container = (sessionContainer as TlsSessionClient) ?? throw new ArgumentException($"Must be typeof ({typeof(TlsSessionClient)})");
                    container.payloadAlpn += value + ';';
                    container.alpn |= TlsFlags.GetAlpnProtocol(in payloadItem);
                }
                else if (side == ESide.Server)
                {
                    TlsSessionServer cont = (TlsSessionServer)sessionContainer;
                    cont.payloadAlpn += value + ';';
                    cont.alpn |= TlsFlags.GetAlpnProtocol(in payloadItem);
                }
            }
        }

        public async Task<DecryptedData> Decrypt()
        {
            this.session = new TlsSession();
            TlsPlayerWithProxy.ParseFrames(readerProxyReadFromClient, framesProxyReadFromClient);
            TlsPlayerWithProxy.ParseFrames(readerProxyWriteToClient, framesProxySendToClient);
            TlsPlayerWithProxy.ParseFrames(readerProxyWriteToOrigin, framesProxySendToOrigin);
            TlsPlayerWithProxy.ParseFrames(readerProxyReadFromOrigin, framesProxyReadFromOrigin);

            ParseClientFrames(framesProxyReadFromClient, session.proxyFromClient);
            ParseServerFrames(framesProxySendToClient, session.proxyToClient);

            ParseClientFrames(framesProxySendToOrigin, session.proxyToOrigin);
            ParseServerFrames(framesProxyReadFromOrigin, session.proxyFromOrigin);

            var decryptedData = new DecryptedData()
            {
                tlsSession = session,
            };
            DecodingArgs decodingArgs = new DecodingArgs()
            {
                host = host,
                session = session,
                decryptedData = decryptedData,

                dataProxyReadFromClient = dataProxyReadFromClient,
                dataProxyWriteToClient = dataProxyWriteToClient,

                readerProxyReadFromClient = readerProxyReadFromClient,
                framesReadProxyFromClient = framesProxyReadFromClient,

                readerProxyWriteToClient = readerProxyWriteToClient,
                framesSendProxyToClient = framesProxySendToClient,

                readerProxyWriteToOrigin = readerProxyWriteToOrigin,
                framesSendProxyToOrigin = framesProxySendToOrigin,

                readerProxyReadFromOrigin = readerProxyReadFromOrigin,
                framesReadProxyFromOrigin = framesProxyReadFromOrigin,
            };

            await DecodeTraffic(decodingArgs).ConfigureAwait(false);
            return null;
            decryptedData.AsClientFrom(framesProxyReadFromClient);
            decryptedData.AsServerFrom(framesProxySendToClient);
            return decryptedData;
        }
        private static void ParseFrames(TlsFrameReader reader, List<TlsFrame> results)
        {
            while (reader.TryReadFrame(out var frame))
            {
                results.Add(frame);
            }
        }
        [HandshakeAnalyz(Side = ESide.Client), Map("Read Tls Frames. Пока самый полный")]
        private void ParseClientFrames(List<TlsFrame> framesProxyReadFromClient, TlsSessionClient proxyFromClient)
        {
            foreach (var frame in framesProxyReadFromClient)
            {
                var data = frame.payload;
                switch (frame.type)
                {
                    case ETlsProtocol.Handshake:
                        {
                            var slice = data;
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
                                            // many CipherSuite cipher_suites<2..2^16-2>;
                                            ReadOnlyMemory<byte> cipherSuitePayload = BufferReader.Consume(cipherSuite.Length, ref handshakePayload);
                                            CompressionMethod compressionMethod = BufferReader.ToStructConsume<CompressionMethod>(ref handshakePayload);
                                            ReadOnlyMemory<byte> compressionMethodPayload = BufferReader.Consume(compressionMethod.lengthFollowed, ref handshakePayload);
                                            TlsExtensions extensions = BufferReader.ToStructConsume<TlsExtensions>(ref handshakePayload);
                                            ReadOnlyMemory<byte> extensionsPayload = BufferReader.Consume(extensions.Length, ref handshakePayload);

                                            var extSlice = extensionsPayload;
                                            while (extSlice.Length > 0)
                                            {
                                                TlsExtension tlsExtension = BufferReader.ToStructConsume<TlsExtension>(ref extSlice);
                                                //Console.WriteLine($"TlsExtension client:{tlsExtension.Type}\tLength:{tlsExtension.Length}");
                                                ReadOnlyMemory<byte> payload = BufferReader.Consume(tlsExtension.Length, ref extSlice);
                                                if (handlersForExtensions.TryGetValue(tlsExtension.TypeUshort, out var handler))
                                                {
                                                    handler.Invoke(tlsExtension, in payload, ESide.Client, proxyFromClient);
                                                }
                                            }
                                            break;
                                        }
                                    case ETlsTypeHandshakeMessage.Certificate:
                                        {
                                            var payload = BufferReader.Consume(header.Length, ref handshakePayload);
                                            if (handlersHandshakeTls.TryGetValue((byte)ETlsTypeHandshakeMessage.Certificate, out var handler))
                                            {
                                                handler.Invoke(in payload, ESide.Client, proxyFromClient);
                                            }
                                            break;
                                        }
                                    case ETlsTypeHandshakeMessage.CertificateVerify:
                                        break;
                                    case ETlsTypeHandshakeMessage.ClientKeyExchange:
                                        break;
                                    case ETlsTypeHandshakeMessage.Finished:
                                        break;
                                    default: throw new ArgumentOutOfRangeException(header.Type.ToString());
                                }
                            }
                            END:
                            break;
                        }
                    case ETlsProtocol.ChangeCipherSpec:
                        {
                            ChangeCipherSpec changeCipherSpec = BufferReader.ToStructConsume<ChangeCipherSpec>(ref data);
                            Console.WriteLine($"Record Layer - ChangeCipherSpec: {changeCipherSpec.ToStringInfo()}");
                            break;
                        }
                    case ETlsProtocol.AlertRecord:
                        break;
                    case ETlsProtocol.ApplicationData:
                        break;
                    default:
                        break;
                }
                if (data.Length > 0)
                {
                    Console.WriteLine($"Slece length left:{data.Length}. TotalLength:{frame.record.Length}");
                }
            }
        }

        [HandshakeAnalyz(Side = ESide.Server), Map("Read Tls Frames")]
        private void ParseServerFrames(List<TlsFrame> framesProxySendToClient, TlsSessionServer proxyFromOrigin)
        {
            foreach (var frame in framesProxySendToClient)
            {
                Console.WriteLine(frame.ToStringInfo());
                switch (frame.type)
                {
                    case ETlsProtocol.Handshake:
                        {
                            var data = frame.payload;
                            int dataLemgth = data.Length;

                            while (data.Length > 0)
                            {
                                HandshakeHeader header = BufferReader.ToStructConsume<HandshakeHeader>(ref data);
                                var slice = BufferReader.Consume(header.Length, ref data);

                                Console.WriteLine($"data left:{data.Length}\tServer HandshakeHeader type:{header.ToStringInfo()}");
                                switch (header.Type)
                                {
                                    case ETlsTypeHandshakeMessage.HelloRequest: goto END_LOOP;
                                    case ETlsTypeHandshakeMessage.ServerHello:
                                        {
                                            ProtocolVersion protocolVersion = BufferReader.ToStructConsume<ProtocolVersion>(ref slice);
                                            TlsRandom tlsRandom = BufferReader.ToStructConsume<TlsRandom>(ref slice);
                                            TlsSessionId tlsSessionId = BufferReader.ToStructConsume<TlsSessionId>(ref slice);
                                            ReadOnlyMemory<byte> sessionIdPayload = BufferReader.Consume(tlsSessionId.lengthFollowed, ref slice);
                                            CipherSuiteValue cipherSuite = BufferReader.ToStructConsume<CipherSuiteValue>(ref slice);
                                            Console.WriteLine(cipherSuite.TlsCipherSuite.ToString());
                                            CompressionMethod compressionMethod = BufferReader.ToStructConsume<CompressionMethod>(ref slice);
                                            ReadOnlyMemory<byte> compressionMethodPayload = BufferReader.Consume(compressionMethod.lengthFollowed, ref slice);
                                            TlsExtensions extensions = BufferReader.ToStructConsume<TlsExtensions>(ref slice);
                                            ReadOnlyMemory<byte> extensionsPayload = BufferReader.Consume(extensions.Length, ref slice);

                                            var extSlice = extensionsPayload;
                                            while (extSlice.Length > 0)
                                            {
                                                TlsExtension tlsExtension = BufferReader.ToStructConsume<TlsExtension>(ref extSlice);
                                                //Console.WriteLine($"TlsExtension server:{tlsExtension.Type}\tLength:{tlsExtension.Length}");
                                                ReadOnlyMemory<byte> payload = BufferReader.Consume(tlsExtension.Length, ref extSlice);
                                                if (handlersForExtensions.TryGetValue(tlsExtension.TypeUshort, out var handler))
                                                {
                                                    handler.Invoke(tlsExtension, in payload, ESide.Server, proxyFromOrigin);
                                                }
                                            }
                                            break;
                                        }
                                    case ETlsTypeHandshakeMessage.Certificate:
                                        {
                                            HeaderForCertificates certificates = BufferReader.ToStructConsume<HeaderForCertificates>(ref slice);
                                            int lengthLeft = certificates.Length;
                                            while (lengthLeft > 0)
                                            {
                                                HeaderForCertificate certificate = BufferReader.ToStructConsume<HeaderForCertificate>(ref slice);
                                                int certLength = certificate.Length;
                                                lengthLeft -= certLength + 3;
                                                var payload = BufferReader.Consume(certLength, ref slice);
                                                if (handlersHandshakeTls.TryGetValue((byte)ETlsTypeHandshakeMessage.Certificate, out var handler))
                                                {
                                                    handler.Invoke(in payload, ESide.Server, proxyFromOrigin);
                                                }
                                            }
                                            break;
                                        }
                                    case ETlsTypeHandshakeMessage.ServerKeyExchange:
                                        Console.WriteLine("ServerKeyExchange");
                                        break;
                                    case ETlsTypeHandshakeMessage.CertificateRequest:
                                        Console.WriteLine("ETlsTypeHandshakeMessage.CertificateRequest");
                                        break;
                                    case ETlsTypeHandshakeMessage.ServerHelloDone:
                                        break;

                                    case ETlsTypeHandshakeMessage.Finished:
                                        break;
                                    default: Console.WriteLine($"Undefined handshake: {header.Type.ToString()}"); break;
                                }
                            }
                            END_LOOP:
                            break;
                        }
                    case ETlsProtocol.ChangeCipherSpec:
                        break;
                    case ETlsProtocol.AlertRecord:
                        break;
                    case ETlsProtocol.ApplicationData:
                        break;
                }
            }
        }

        private TlsSession ParseTlsHandshake(ReadOnlyMemory<byte> memRaw, ref ReadOnlyMemory<byte> sliceRequest)
        {
            var tlsSession = this.session;
            var clientHello = ParserForTls12.ParseClientHello(ref sliceRequest, out ReadOnlyMemory<byte> payload0);
            var clientKeyExchange = ParserForTls12.ParseClientKeyExchange(ref sliceRequest, out ReadOnlyMemory<byte> payload2);
            var clientChangeCipherSpec = ParserForTls12.ParseClientChangeCipherSpec(ref sliceRequest, out ReadOnlyMemory<byte> payload3);
            var clientHandshakeFinished = ParserForTls12.ParseClientHandshakeFinished(ref sliceRequest, out ReadOnlyMemory<byte> payload4);
            var clientApplicationData = ParserForTls12.ParseClientApplicationData(ref sliceRequest, out ReadOnlyMemory<byte> payload5);
            var clientCloseNotify = ParserForTls12.ParseClientCloseNotify(ref sliceRequest, out ReadOnlyMemory<byte> payload6);
            return tlsSession;
        }

        /// <summary>
        /// <see cref=""/>
        /// </summary>
        /// <param name="decryptedData"></param>
        /// <param name="readerAgentToOrigin"></param>
        /// <param name="framesProxyReadFromClient"></param>
        /// <param name="readerOriginToAgent"></param>
        /// <param name="framesProxyWriteToClient"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static async Task DecodeTraffic(DecodingArgs args)
        {
            var session = args.session;
            var host = args.host;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Begin Decoding");

            StreamForTlsFrames streamForFramesProxyToClient = new StreamForTlsFrames(args.framesReadProxyFromClient, ESide.Server);
            StreamForTlsFrames streamForFramesClientToProxy = new StreamForTlsFrames(args.framesSendProxyToClient, ESide.Client);

            StreamForTlsFrames streamForFramesProxyToOrigin = new StreamForTlsFrames(args.framesSendProxyToClient, ESide.Client);
            StreamForTlsFrames streamForFramesOriginToProxy = new StreamForTlsFrames(args.framesReadProxyFromOrigin, ESide.Server);

            StreamAdapted streamProxyToClient = new StreamAdapted();
            StreamAdapted streamClientToProxy = new StreamAdapted();

            //streamProxyToClient.Initilize(streamForFramesProxyToClient, );

            // этот стрим будет писать Client Hello и считывать ServerHello. Это значит что внутренний стрим должен давать данные от сервера
            SslStream sslConnectionProxyWithClientAsClient = new SslStream(streamForFramesClientToProxy);
            // принимает Client Hello отправляет Server Hello. будет читать в качестве ответа данные от сервера
            SslStream sslConnectionProxyWithClientAsServer = new SslStream(streamForFramesProxyToClient);

            //var t1 = sslAgent.WriteAsync(dataProxyToAgent);
            var certManager = CertManager.GetOrCreateShared();
            var ca = CertManager.SharedCa;
            var certOriginal = session.proxyFromOrigin.Cert;

            var clientOptions = SslOptionsFactory.CreateOptionsForClient(host, session.proxyFromClient.GetFlags(), session.proxyFromClient.GetAppProtocols());
            var serverFlags = session.proxyToClient.GetFlags();
            var serverProtocols = session.proxyToClient.GetAppProtocols();
            SslServerAuthenticationOptions optionsServer = await SslOptionsFactory.CreateOptionsForServer(certManager, serverFlags, serverProtocols, ca, certOriginal);

            //var t1 = sslConnectionProxyWithClientAsClient.AuthenticateAsClientAsync(clientOptions);
            var t2 = sslConnectionProxyWithClientAsServer.AuthenticateAsServerAsync(optionsServer);

            //await t1.ConfigureAwait(false);
            await t2.ConfigureAwait(false);

            throw new System.NotImplementedException();
        }

        internal class DecodingArgs
        {
            internal string host;
            public TlsSession session;
            public DecryptedData decryptedData;

            public TlsFrameReader readerProxyReadFromClient;
            public List<TlsFrame> framesReadProxyFromClient;

            public TlsFrameReader readerProxyWriteToClient;
            public List<TlsFrame> framesSendProxyToClient;

            internal TlsFrameReader readerProxyWriteToOrigin;
            internal List<TlsFrame> framesSendProxyToOrigin;

            internal TlsFrameReader readerProxyReadFromOrigin;
            internal List<TlsFrame> framesReadProxyFromOrigin;

            internal ReadOnlyMemory<byte> dataProxyReadFromClient;
            internal ReadOnlyMemory<byte> dataProxyWriteToClient;
        }
    }
}