using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Attributes;

namespace IziHardGames.Libs.Cryptography.Tls12
{

    public static class TlsConnection12
    {
        public readonly ref struct ParseResult
        {
            public readonly TlsRecord record;
            public readonly HandshakeHeader handsakeHeader;
            public readonly ReadOnlyMemory<byte> payload;

            public ParseResult(in TlsRecord record, in HandshakeHeader header, in ReadOnlyMemory<byte> payload) : this()
            {
                //this.handshakeRecord = default;
                this.record = record;
                this.handsakeHeader = header;
                this.payload = payload;
            }

            public ParseResult(in TlsRecord record, in ReadOnlyMemory<byte> payload) : this()
            {
                //this.handshakeRecord = default;
                this.handsakeHeader = default;
                this.record = record;
                this.payload = payload;
            }

            internal static ParseResult FromHandshake(in TlsRecord record, ref ReadOnlyMemory<byte> data)
            {
                HandshakeHeader header = BufferReader.ToStruct<HandshakeHeader>(in data);
                int length = record.Length;
                var payload = data.Slice(0, length);
                data = data.Slice(length);
                return new ParseResult(in record, in header, in payload);
            }

            internal static ParseResult FromChangeCipherSpec(in TlsRecord record, ref ReadOnlyMemory<byte> data)
            {
                int length = record.Length;
                var payload = data.Slice(0, length);
                data = data.Slice(length);
                return new ParseResult(in record, in payload);
            }

            internal static ParseResult FromAlertRecord(in TlsRecord header, ref ReadOnlyMemory<byte> data)
            {
                return FromChangeCipherSpec(in header, ref data);
            }

            internal static ParseResult FromApplicationData(in TlsRecord header, ref ReadOnlyMemory<byte> data)
            {
                return FromChangeCipherSpec(in header, ref data);
            }

            public string ToStringInfo()
            {
                if (record.TypeRecord == ETlsTypeRecord.Handshake)
                {
                    return $"Record:[{record.ToStringInfo()}]; Handshake:[{handsakeHeader.ToStringInfo()}]";
                }
                return $"Record:[{record.ToStringInfo()}]";
            }
        }

        public static bool TryParse(ref ReadOnlyMemory<byte> data, out ParseResult result)
        {
            if (data.Length > 0)
            {
                TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
                if (header.Validate())
                {
                    data = data.Slice(ConstantsForTls.SIZE_RECORD);
                    switch (header.TypeRecord)
                    {
                        case ETlsTypeRecord.Handshake: result = ParseResult.FromHandshake(in header, ref data); return true;
                        case ETlsTypeRecord.ChangeCipherSpec: result = ParseResult.FromChangeCipherSpec(in header, ref data); return true;
                        case ETlsTypeRecord.AlertRecord: result = ParseResult.FromAlertRecord(in header, ref data); return true;
                        case ETlsTypeRecord.ApplicationData: result = ParseResult.FromApplicationData(in header, ref data); return true;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }
            result = default;
            return false;
        }
        public static void Parse(ref ReadOnlyMemory<byte> data, out ParseResult result)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            data = data.Slice(ConstantsForTls.SIZE_RECORD);
            switch (header.TypeRecord)
            {
                case ETlsTypeRecord.Handshake: result = ParseResult.FromHandshake(in header, ref data); return;
                case ETlsTypeRecord.ChangeCipherSpec: result = ParseResult.FromChangeCipherSpec(in header, ref data); return;
                case ETlsTypeRecord.AlertRecord: result = ParseResult.FromAlertRecord(in header, ref data); return;
                case ETlsTypeRecord.ApplicationData: result = ParseResult.FromApplicationData(in header, ref data); return;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// [1] Sender:Client
        /// </summary>
        [HandshakeStage(Numeric = 1, SideAccepting = EHandshakeSide.Server, Stage = EHandshakeStage.ClientHello)]
        public static HandshakeRecord ParseClientHello(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            HandshakeRecord header = BufferReader.ToStruct<HandshakeRecord>(in data);
            int length = header.record.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD_HANDSHAKE, length);
            data = data.Slice(length + ConstantsForTls.SIZE_RECORD);
            return header;
        }

        /// <summary>
        /// [2] Sernder:Server
        /// </summary>
        /// <param name="payload"> Memory Slice without <see cref="TlsRecord"/> But with <see cref="HandshakeHeader"/> at start</param>
        /// <returns></returns>
        [HandshakeStage(Numeric = 2, SideAccepting = EHandshakeSide.Client, Stage = EHandshakeStage.ServerHello)]
        public static HandshakeRecord ParseServerHello(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            HandshakeRecord header = BufferReader.ToStruct<HandshakeRecord>(in data);
            int length = header.record.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD_HANDSHAKE, length);
            data = data.Slice(length + ConstantsForTls.SIZE_RECORD);
            return header;
        }

        /// <summary>
        /// [3] Sender:Server
        /// </summary>
        /// <param name="data"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HandshakeStage(Numeric = 3, SideAccepting = EHandshakeSide.Client, Stage = EHandshakeStage.ServerCertificate)]
        public static HandshakeRecord ParseServerCertificate(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            HandshakeRecord header = BufferReader.ToStruct<HandshakeRecord>(in data);
            int length = header.record.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD_HANDSHAKE, length);
            data = data.Slice(length + ConstantsForTls.SIZE_RECORD);
            return header;
        }
        /// <summary>
        /// [4] Sender:Server
        /// </summary>
        [HandshakeStage(Numeric = 4, SideAccepting = EHandshakeSide.Client, Stage = EHandshakeStage.ServerKeyExchange)]
        public static HandshakeRecord ParseServerKeyExchange(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            HandshakeRecord header = BufferReader.ToStruct<HandshakeRecord>(in data);
            int length = header.record.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD_HANDSHAKE, length);
            data = data.Slice(length + ConstantsForTls.SIZE_RECORD);
            return header;
        }
        /// <summary>
        /// [5] Sernder:Server
        /// </summary>
        [HandshakeStage(Numeric = 5, SideAccepting = EHandshakeSide.Client, Stage = EHandshakeStage.ServerHelloDone)]
        public static HandshakeRecord ParseServerHelloDone(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            HandshakeRecord header = BufferReader.ToStruct<HandshakeRecord>(in data);
            int length = header.record.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD_HANDSHAKE, length);
            data = data.Slice(length + ConstantsForTls.SIZE_RECORD);
            return header;
        }
        /// <summary>
        /// [6] Sender:Client
        /// </summary>
        [HandshakeStage(Numeric = 6, SideAccepting = EHandshakeSide.Server, Stage = EHandshakeStage.ClientKeyExchange)]
        public static HandshakeRecord ParseClientKeyExchange(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            HandshakeRecord header = BufferReader.ToStruct<HandshakeRecord>(in data);
            int length = header.record.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD_HANDSHAKE, length);
            data = data.Slice(length + ConstantsForTls.SIZE_RECORD);
            return header;
        }
        /// <summary>
        /// [7] Sender:Client
        /// </summary>
        [HandshakeStage(Numeric = 7, SideAccepting = EHandshakeSide.Server, Stage = EHandshakeStage.ClientChangeCipherSpec)]
        public static TlsRecord ParseClientChangeCipherSpec(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            int length = header.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD, length);
            data = data.Slice(ConstantsForTls.SIZE_RECORD + length);
            return header;
        }
        /// <summary>
        /// [8] Sender:Client
        /// </summary>
        [HandshakeStage(Numeric = 8, SideAccepting = EHandshakeSide.Server, Stage = EHandshakeStage.ClientHandshakeFinished)]
        public static TlsRecord ParseClientHandshakeFinished(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            int length = header.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD, length);
            data = data.Slice(ConstantsForTls.SIZE_RECORD + length);
            return header;
        }
        /// <summary>
        /// [9] Sender:Server
        /// </summary>
        [HandshakeStage(Numeric = 9, SideAccepting = EHandshakeSide.Client, Stage = EHandshakeStage.ServerChangeCipherSpec)]
        public static TlsRecord ParseServerChangeCipherSpec(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            int length = header.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD, length);
            data = data.Slice(ConstantsForTls.SIZE_RECORD + length);
            return header;
        }
        /// <summary>
        /// [10] Sender:Server
        /// </summary>
        [HandshakeStage(Numeric = 10, SideAccepting = EHandshakeSide.Client, Stage = EHandshakeStage.ServerHandshakeFinished)]
        public static TlsRecord ParseServerHandshakeFinished(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            int length = header.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD, length);
            data = data.Slice(ConstantsForTls.SIZE_RECORD + length);
            return header;
        }
        /// <summary>
        /// [11] Sender:Client
        /// </summary>
        [HandshakeStage(Numeric = 11, SideAccepting = EHandshakeSide.Server, Stage = EHandshakeStage.ClientApplicationData)]
        public static TlsRecord ParseClientApplicationData(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            int length = header.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD, length);
            data = data.Slice(ConstantsForTls.SIZE_RECORD + length);
            return header;
        }
        /// <summary>
        /// [12] Sender:Server
        /// </summary>
        [HandshakeStage(Numeric = 12, SideAccepting = EHandshakeSide.Server, Stage = EHandshakeStage.ServerApplicationData)]
        public static TlsRecord ParseServerApplicationData(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            int length = header.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD, length);
            data = data.Slice(ConstantsForTls.SIZE_RECORD + length);
            return header;
        }
        /// <summary>
        /// [13] Sender:Client
        /// </summary>
        [HandshakeStage(Numeric = 13, SideAccepting = EHandshakeSide.Server, Stage = EHandshakeStage.ClientCloseNotify)]
        public static TlsRecord ParseClientCloseNotify(ref ReadOnlyMemory<byte> data, out ReadOnlyMemory<byte> payload)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            int length = header.Length;
            payload = data.Slice(ConstantsForTls.SIZE_RECORD, length);
            data = data.Slice(ConstantsForTls.SIZE_RECORD + length);
            return header;
        }

        internal static void TestClient(in ReadOnlyMemory<byte> mem)
        {
            var sliceRequest = mem;
            var clientHello = TlsConnection12.ParseClientHello(ref sliceRequest, out ReadOnlyMemory<byte> payload0);
            var clientKeyExchange = TlsConnection12.ParseClientKeyExchange(ref sliceRequest, out ReadOnlyMemory<byte> payload2);
            var clientChangeCipherSpec = TlsConnection12.ParseClientChangeCipherSpec(ref sliceRequest, out ReadOnlyMemory<byte> payload3);
            var clientHandshakeFinished = TlsConnection12.ParseClientHandshakeFinished(ref sliceRequest, out ReadOnlyMemory<byte> payload4);
            var clientApplicationData = TlsConnection12.ParseClientApplicationData(ref sliceRequest, out ReadOnlyMemory<byte> payload5);
            var clientCloseNotify = TlsConnection12.ParseClientCloseNotify(ref sliceRequest, out ReadOnlyMemory<byte> payload6);

        }
        internal static void TestServer(in ReadOnlyMemory<byte> mem)
        {
            var sliceResponse = mem;
            var serverHello = TlsConnection12.ParseServerHello(ref sliceResponse, out ReadOnlyMemory<byte> payloadS0);
            var serverCertificate = TlsConnection12.ParseServerCertificate(ref sliceResponse, out ReadOnlyMemory<byte> payloadS1);
            var serverKeyExchange = TlsConnection12.ParseServerKeyExchange(ref sliceResponse, out ReadOnlyMemory<byte> payloadS2);
            var serverHelloDone = TlsConnection12.ParseServerHelloDone(ref sliceResponse, out ReadOnlyMemory<byte> payloadS3);
            var changeCipherSpec = TlsConnection12.ParseServerChangeCipherSpec(ref sliceResponse, out ReadOnlyMemory<byte> payloadS4);
            var serverHandshakeFinished = TlsConnection12.ParseServerHandshakeFinished(ref sliceResponse, out ReadOnlyMemory<byte> payloadS5);
            var serverApplicationData = TlsConnection12.ParseServerApplicationData(ref sliceResponse, out ReadOnlyMemory<byte> payloadS6);
        }
    }
}