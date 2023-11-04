using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Readers;
using IziHardGames.Libs.Cryptography.Readers.Tls12;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography.Tls12;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IziHardGames.Libs.Cryptography.Infos
{
    /// <summary>
    /// Единица передачи протокола Tls
    /// </summary>
    /// <see cref="TlsReader"/>
    /// <see cref="TlsFrameReader"/>
    public class TlsFrame : IDisposable
    {
        public ETlsProtocol type;
        public int lengthTotal;
        public TlsRecord record;
        public ReadOnlyMemory<byte> payload;
        public ReadOnlyMemory<byte> dataWholeFrame;

        internal TlsFrame(in TlsRecord tlsRecord, ReadOnlyMemory<byte> dataWholeFrame)
        {
            this.record = tlsRecord;
            this.dataWholeFrame = dataWholeFrame;
            payload = dataWholeFrame.Slice(ConstantsForTls.SIZE_RECORD);
            lengthTotal = dataWholeFrame.Length;
            type = tlsRecord.TypeProtocol;
        }
        internal TlsFrame(ParserForTls12.FrameParseResult parseResult)
        {
            type = parseResult.record.TypeProtocol;
            this.record = parseResult.record;
            this.payload = parseResult.payload;
            this.dataWholeFrame = parseResult.wholeFrame;
            lengthTotal = parseResult.wholeFrame.Length;
        }
        public void Dispose()
        {
            payload = default;
        }

        internal ReadOnlyMemory<byte> GetMemSLice(int offsetRead)
        {
            return dataWholeFrame.Slice(offsetRead);
        }
        internal ReadOnlyMemory<byte> GetMemSLice(int offsetRead, int length)
        {
            return dataWholeFrame.Slice(offsetRead, length);
        }

        internal string ToStringInfo()
        {
            return $"Type:{type}\tLengthTotal:{lengthTotal}\tRecord:{record.ToStringInfo()}.\tReadOnlyMemLength:{payload.Length}";
        }

        public string ToStringInfoAsClient()
        {
            return $"{record.ToStringInfo()}\t{ToStringInfoPayloadAsClient()}";
        }

        public string ToStringInfoAsServer()
        {
            return $"{record.ToStringInfo()}\t{ToStringInfoPayloadAsServer()}";
        }

        [HandshakeAnalyz(Side = ESide.Client), Map("Read Tls Frames To String Info")]
        public string ToStringInfoPayloadAsClient()
        {
            var slice = payload;
            string result = string.Empty;
            switch (record.TypeProtocol)
            {
                case ETlsProtocol.Handshake:
                    {
                        HandshakeHeader header = BufferReader.ToStructConsume<HandshakeHeader>(ref slice);
                        result += $"{header.ToStringInfo()}";
                        break;
                    }
                case ETlsProtocol.ChangeCipherSpec:
                    break;
                case ETlsProtocol.AlertRecord:
                    break;
                case ETlsProtocol.ApplicationData:
                    break;
                default:
                    break;
            }
            return result;
        }
        [HandshakeAnalyz(Side = ESide.Server), Map("Read Tls Frames To String Info")]
        public string ToStringInfoPayloadAsServer()
        {
            var slice = payload;
            string result = string.Empty;
            switch (record.TypeProtocol)
            {
                case ETlsProtocol.Handshake:
                    {
                        HandshakeHeader header = BufferReader.ToStructConsume<HandshakeHeader>(ref slice);
                        result += $"{header.ToStringInfo()}";
                        break;
                    }
                case ETlsProtocol.ChangeCipherSpec:
                    break;
                case ETlsProtocol.AlertRecord:
                    break;
                case ETlsProtocol.ApplicationData:
                    break;
                default:
                    break;
            }
            return result;
        }

        internal bool IsRequestHello()
        {
            if (record.TypeProtocol == ETlsProtocol.Handshake)
            {
                HandshakeHeader handshakeHeader = BufferReader.ToStruct<HandshakeHeader>(payload);
                return handshakeHeader.Type == ETlsTypeHandshakeMessage.HelloRequest;
            }
            return false;
        }
    }

    /// <summary>
    /// Datas About Session Between Server and Client
    /// </summary>
    public class TlsSession
    {
        public SslApplicationProtocol alpn;
        public readonly TlsSessionClient proxyFromClient = new TlsSessionClient();
        public readonly TlsSessionServer proxyToClient = new TlsSessionServer();

        public readonly TlsSessionClient proxyToOrigin = new TlsSessionClient();
        public readonly TlsSessionServer proxyFromOrigin = new TlsSessionServer();
    }
    public class TlsSessionClient
    {
        internal string payloadAlpn;
        internal EAlpn alpn;
        internal List<X509Certificate2> certs = new List<X509Certificate2>();

        internal SslProtocols GetFlags()
        {
            return SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
        }
        internal List<SslApplicationProtocol> GetAppProtocols()
        {
            List<SslApplicationProtocol> list = new List<SslApplicationProtocol>();
            if (alpn.HasFlag(EAlpn.h3))
            {
                list.Add(SslApplicationProtocol.Http3);
            }
            if (alpn.HasFlag(EAlpn.h2))
            {
                list.Add(SslApplicationProtocol.Http2);
            }
            if (alpn.HasFlag(EAlpn.http11))
            {
                list.Add(SslApplicationProtocol.Http11);
            }
            return list;
        }
    }
    public class TlsSessionServer
    {
        public X509Certificate2 Cert => certs.First();
        internal string payloadAlpn;
        internal EAlpn alpn;
        internal List<X509Certificate2> certs = new List<X509Certificate2>();

        internal List<SslApplicationProtocol> GetAppProtocols()
        {
            List<SslApplicationProtocol> list = new List<SslApplicationProtocol>();
            if (alpn.HasFlag(EAlpn.h3))
            {
                list.Add(SslApplicationProtocol.Http3);
            }
            if (alpn.HasFlag(EAlpn.h2))
            {
                list.Add(SslApplicationProtocol.Http2);
            }
            if (alpn.HasFlag(EAlpn.http11))
            {
                list.Add(SslApplicationProtocol.Http11);
            }
            return list;
        }
        internal SslProtocols GetFlags()
        {
            return SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
        }
    }
}