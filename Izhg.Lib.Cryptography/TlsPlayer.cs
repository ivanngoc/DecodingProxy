using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Delegates;
using IziHardGames.Libs.Cryptography.Infos;
using IziHardGames.Libs.Cryptography.Readers;
using IziHardGames.Libs.Cryptography.Readers.Tls12;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography.Tls;
using IziHardGames.Libs.Cryptography.Tls12;
using static IziHardGames.Libs.Cryptography.Readers.Tls12.ParserForTls12;

namespace IziHardGames.Libs.Cryptography.Recording
{
    public class DecryptedData
    {
        public ReadOnlyMemory<byte> dataAgentToOrigin;
        public ReadOnlyMemory<byte> dataOriginToAgent;
        public TlsSession tlsSession;

        internal void AsClientFrom(List<TlsFrame> framesAgentToOrigin)
        {
            throw new NotImplementedException();
        }

        internal void AsServerFrom(List<TlsFrame> framesOriginToClient)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Play Recorded Raw Data
    /// </summary>
    public class TlsPlayer
    {
        private ReadOnlyMemory<byte> dataAgentToOrigin;
        private ReadOnlyMemory<byte> dataOriginToAgent;
        private TlsFrameReader readerAgentToOrigin;
        private TlsFrameReader readerOriginToAgent;
        private TlsSession session;
        private List<TlsFrame> framesAgentToOrigin = new List<TlsFrame>();
        private List<TlsFrame> framesOriginToClient = new List<TlsFrame>();
        private readonly Dictionary<ushort, TlsExtensionHandler> handlersForExtensions = new Dictionary<ushort, TlsExtensionHandler>();
        public TlsPlayer(ReadOnlyMemory<byte> client, ReadOnlyMemory<byte> server)
        {
            readerAgentToOrigin = new TlsFrameReader(client);
            readerOriginToAgent = new TlsFrameReader(server);

            this.dataAgentToOrigin = client;
            this.dataOriginToAgent = server;

            handlersForExtensions.Add((ushort)ETlsExtensions.APPLICATION_LAYER_PROTOCOL_NEGOTIATION, HandleExtAlpn);
            handlersForExtensions.Add((ushort)ETlsExtensions.client_certificate_url, HandleExtClientCertificateUrl);
        }

        private void HandleExtClientCertificateUrl(TlsExtension ext, in ReadOnlyMemory<byte> data, ESide side)
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
        private void HandleExtAlpn(TlsExtension ext, in ReadOnlyMemory<byte> data, ESide side)
        {
            var slice = data;
            for (int i = 2; i < data.Length; i++)
            {
                Console.WriteLine($"char:[{(char)data.Span[i]}]\tbyte:[{ParseByte.ByteToHexFormated(data.Span[i])}]");
            }
            ExtDataAlpnProtocolList list = BufferReader.ToStructConsume<ExtDataAlpnProtocolList>(ref slice);
            var payloadList = BufferReader.Consume(list.Length, ref slice);

            while (payloadList.Length > 0)
            {
                ExtDataAlpnProtocolItem item = BufferReader.ToStructConsume<ExtDataAlpnProtocolItem>(ref payloadList);
                var payloadItem = BufferReader.Consume(item.length, ref payloadList);
                var value = Encoding.UTF8.GetString(payloadItem.Span);
                if (side == ESide.Client)
                {
                    session!.client.payloadAlpn += value + ';';
                }
                else if (side == ESide.Server)
                {
                    session!.server.payloadAlpn += value + ';';
                }
            }
        }

        public DecryptedData Decrypt()
        {
            this.session = new TlsSession();
            ParseFrames(readerAgentToOrigin, framesAgentToOrigin);
            ParseFrames(readerOriginToAgent, framesOriginToClient);
            ParseClientHandshake();
            return null;
            ParseServerHandshake();
            var decryptedData = new DecryptedData()
            {
                tlsSession = session,
            };
            decryptedData.AsClientFrom(framesAgentToOrigin);
            decryptedData.AsServerFrom(framesOriginToClient);
            return decryptedData;
        }
        private void ParseFrames(TlsFrameReader reader, List<TlsFrame> results)
        {
            while (reader.TryReadFrame(out var frame))
            {
                results.Add(frame);
            }
        }
        [HandshakeAnalyz(Side = ESide.Client), Map("Read Tls Frames")]
        private void ParseClientHandshake()
        {
            foreach (var frame in framesAgentToOrigin)
            {
                switch (frame.type)
                {
                    case ETlsTypeRecord.Handshake:
                        {
                            var slice = frame.data;
                            HandshakeHeader header = BufferReader.ToStructConsume<HandshakeHeader>(ref slice);
                            Console.WriteLine($"HandshakeHeader type:{header.Type}");
                            switch (header.Type)
                            {
                                case ETlsTypeHandshakeMessage.HelloRequest: break;
                                case ETlsTypeHandshakeMessage.ClientHello:
                                    {
                                        ProtocolVersion protocolVersion = BufferReader.ToStructConsume<ProtocolVersion>(ref slice);
                                        TlsRandom tlsRandom = BufferReader.ToStructConsume<TlsRandom>(ref slice);
                                        TlsSessionId tlsSessionId = BufferReader.ToStructConsume<TlsSessionId>(ref slice);
                                        ReadOnlyMemory<byte> sessionIdPayload = BufferReader.Consume(tlsSessionId.lengthFollowed, ref slice);
                                        CipherSuite cipherSuite = BufferReader.ToStructConsume<CipherSuite>(ref slice);
                                        // many CipherSuite cipher_suites<2..2^16-2>;
                                        ReadOnlyMemory<byte> cipherSuitePayload = BufferReader.Consume(cipherSuite.Length, ref slice);
                                        CompressionMethod compressionMethod = BufferReader.ToStructConsume<CompressionMethod>(ref slice);
                                        ReadOnlyMemory<byte> compressionMethodPayload = BufferReader.Consume(compressionMethod.lengthFollowed, ref slice);
                                        TlsExtensions extensions = BufferReader.ToStructConsume<TlsExtensions>(ref slice);
                                        ReadOnlyMemory<byte> extensionsPayload = BufferReader.Consume(extensions.Length, ref slice);

                                        var extSlice = extensionsPayload;
                                        while (extSlice.Length > 0)
                                        {
                                            TlsExtension tlsExtension = BufferReader.ToStructConsume<TlsExtension>(ref extSlice);
                                            Console.WriteLine($"TlsExtension:{tlsExtension.Type}\tLength:{tlsExtension.Length}");
                                            ReadOnlyMemory<byte> payload = BufferReader.Consume(tlsExtension.Length, ref extSlice);
                                            if (handlersForExtensions.TryGetValue(tlsExtension.TypeUshort, out var handler))
                                            {
                                                handler.Invoke(tlsExtension, in payload, ESide.Client);
                                            }
                                        }
                                        break;
                                    }
                                case ETlsTypeHandshakeMessage.Certificate:
                                    break;
                                case ETlsTypeHandshakeMessage.CertificateVerify:
                                    break;
                                case ETlsTypeHandshakeMessage.ClientKeyExchange:
                                    break;
                                case ETlsTypeHandshakeMessage.Finished:
                                    break;
                                default: throw new ArgumentOutOfRangeException(header.Type.ToString());
                            }
                            break;
                        }
                    case ETlsTypeRecord.ChangeCipherSpec:
                        break;
                    case ETlsTypeRecord.AlertRecord:
                        break;
                    case ETlsTypeRecord.ApplicationData:
                        break;
                    default:
                        break;
                }
            }
        }
        private void ParseServerHandshake()
        {

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

            HandshakeHelloInfo handshakeInfo = default;// BuildTlsHandshakeAsClient(in clientHello, in payload0);

            Console.WriteLine(handshakeInfo.ToStringInfo());

            if (handshakeInfo.isAlpnH3)
            {
                throw new System.NotImplementedException();
            }
            else if (handshakeInfo.isAlpnH2)
            {
                var sliceResponse = new ReadOnlyMemory<byte>(); //entry.agentResponses.datas;
                X509Certificate2Collection certCollection = default;

                var serverHello = ParserForTls12.ParseServerHello(ref sliceResponse, out var payloadServerHello);

                if (!HandshakeHelloFromServerAnalyz.TryAnalyz(in serverHello, in payloadServerHello, out var serverHello0))
                {

                }

                while (ParserForTls12.TryParse(ref sliceResponse, out FrameParseResult parseResult))
                {
                    if (sliceResponse.Length == 0)
                    {
                        Console.WriteLine("Reached Zero");
                    }
                    Console.WriteLine(parseResult.ToStringInfo() + $" LEFT:{sliceResponse.Length}");

                    if (parseResult.record.TypeRecord == ETlsTypeRecord.Handshake)
                    {
                        if (parseResult.handsakeHeader.Type == ETlsTypeHandshakeMessage.Certificate)
                        {
                            certCollection = TlsParser.ParseServerCert(in parseResult, in parseResult.payload);
                        }
                    }
                }
                var serverCert = certCollection[0];
                tlsSession.server.cert = serverCert;
            }
            else if (handshakeInfo.isHttp11)
            {
                throw new System.NotImplementedException();

                //foreach (var map in new EnumHttp11(sliceRequest))
                //{

                //}
            }
            return tlsSession;
        }
    }
}