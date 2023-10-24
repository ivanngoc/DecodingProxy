using System;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.Libs.Cryptography.Readers.Tls12
{
    public static class ParserForTls12
    {
        public readonly ref struct FrameParseResult
        {
            public readonly TlsRecord record;
            public readonly HandshakeHeader handsakeHeader;
            public readonly ReadOnlyMemory<byte> payload;

            public FrameParseResult(in TlsRecord record, in HandshakeHeader header, in ReadOnlyMemory<byte> payload) : this()
            {
                //this.handshakeRecord = default;
                this.record = record;
                handsakeHeader = header;
                this.payload = payload;
            }

            public FrameParseResult(in TlsRecord record, in ReadOnlyMemory<byte> payload) : this()
            {
                //this.handshakeRecord = default;
                handsakeHeader = default;
                this.record = record;
                this.payload = payload;
            }

            internal static FrameParseResult FromHandshake(in TlsRecord record, ref ReadOnlyMemory<byte> data)
            {
                HandshakeHeader header = BufferReader.ToStruct<HandshakeHeader>(in data);
                int length = record.Length;
                var payload = data.Slice(0, length);
                data = data.Slice(length);
                return new FrameParseResult(in record, in header, in payload);
            }

            internal static FrameParseResult FromChangeCipherSpec(in TlsRecord record, ref ReadOnlyMemory<byte> data)
            {
                int length = record.Length;
                var payload = data.Slice(0, length);
                data = data.Slice(length);
                return new FrameParseResult(in record, in payload);
            }

            internal static FrameParseResult FromAlertRecord(in TlsRecord header, ref ReadOnlyMemory<byte> data)
            {
                return FromChangeCipherSpec(in header, ref data);
            }

            internal static FrameParseResult FromApplicationData(in TlsRecord header, ref ReadOnlyMemory<byte> data)
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

        public static bool TryParse(ref ReadOnlyMemory<byte> data, out FrameParseResult result)
        {
            if (data.Length > 0)
            {
                TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
                if (header.Validate())
                {
                    data = data.Slice(ConstantsForTls.SIZE_RECORD);
                    switch (header.TypeRecord)
                    {
                        case ETlsTypeRecord.Handshake: result = FrameParseResult.FromHandshake(in header, ref data); return true;
                        case ETlsTypeRecord.ChangeCipherSpec: result = FrameParseResult.FromChangeCipherSpec(in header, ref data); return true;
                        case ETlsTypeRecord.AlertRecord: result = FrameParseResult.FromAlertRecord(in header, ref data); return true;
                        case ETlsTypeRecord.ApplicationData: result = FrameParseResult.FromApplicationData(in header, ref data); return true;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }
            result = default;
            return false;
        }
        public static void Parse(ref ReadOnlyMemory<byte> data, out FrameParseResult result)
        {
            TlsRecord header = BufferReader.ToStruct<TlsRecord>(in data);
            data = data.Slice(ConstantsForTls.SIZE_RECORD);
            switch (header.TypeRecord)
            {
                case ETlsTypeRecord.Handshake: result = FrameParseResult.FromHandshake(in header, ref data); return;
                case ETlsTypeRecord.ChangeCipherSpec: result = FrameParseResult.FromChangeCipherSpec(in header, ref data); return;
                case ETlsTypeRecord.AlertRecord: result = FrameParseResult.FromAlertRecord(in header, ref data); return;
                case ETlsTypeRecord.ApplicationData: result = FrameParseResult.FromApplicationData(in header, ref data); return;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// [1] Sender:Client
        /// </summary>
        [HandshakeStage(Numeric = 1, SideAccepting = ESide.Server, Stage = EHandshakeStage.ClientHello)]
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
        [HandshakeStage(Numeric = 2, SideAccepting = ESide.Client, Stage = EHandshakeStage.ServerHello)]
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
        [HandshakeStage(Numeric = 3, SideAccepting = ESide.Client, Stage = EHandshakeStage.ServerCertificate)]
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
        [HandshakeStage(Numeric = 4, SideAccepting = ESide.Client, Stage = EHandshakeStage.ServerKeyExchange)]
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
        [HandshakeStage(Numeric = 5, SideAccepting = ESide.Client, Stage = EHandshakeStage.ServerHelloDone)]
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
        [HandshakeStage(Numeric = 6, SideAccepting = ESide.Server, Stage = EHandshakeStage.ClientKeyExchange)]
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
        [HandshakeStage(Numeric = 7, SideAccepting = ESide.Server, Stage = EHandshakeStage.ClientChangeCipherSpec)]
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
        [HandshakeStage(Numeric = 8, SideAccepting = ESide.Server, Stage = EHandshakeStage.ClientHandshakeFinished)]
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
        [HandshakeStage(Numeric = 9, SideAccepting = ESide.Client, Stage = EHandshakeStage.ServerChangeCipherSpec)]
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
        [HandshakeStage(Numeric = 10, SideAccepting = ESide.Client, Stage = EHandshakeStage.ServerHandshakeFinished)]
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
        [HandshakeStage(Numeric = 11, SideAccepting = ESide.Server, Stage = EHandshakeStage.ClientApplicationData)]
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
        [HandshakeStage(Numeric = 12, SideAccepting = ESide.Server, Stage = EHandshakeStage.ServerApplicationData)]
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
        [HandshakeStage(Numeric = 13, SideAccepting = ESide.Server, Stage = EHandshakeStage.ClientCloseNotify)]
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
            var clientHello = ParseClientHello(ref sliceRequest, out ReadOnlyMemory<byte> payload0);
            var clientKeyExchange = ParseClientKeyExchange(ref sliceRequest, out ReadOnlyMemory<byte> payload2);
            var clientChangeCipherSpec = ParseClientChangeCipherSpec(ref sliceRequest, out ReadOnlyMemory<byte> payload3);
            var clientHandshakeFinished = ParseClientHandshakeFinished(ref sliceRequest, out ReadOnlyMemory<byte> payload4);
            var clientApplicationData = ParseClientApplicationData(ref sliceRequest, out ReadOnlyMemory<byte> payload5);
            var clientCloseNotify = ParseClientCloseNotify(ref sliceRequest, out ReadOnlyMemory<byte> payload6);

        }
        internal static void TestServer(in ReadOnlyMemory<byte> mem)
        {
            var sliceResponse = mem;
            var serverHello = ParseServerHello(ref sliceResponse, out ReadOnlyMemory<byte> payloadS0);
            var serverCertificate = ParseServerCertificate(ref sliceResponse, out ReadOnlyMemory<byte> payloadS1);
            var serverKeyExchange = ParseServerKeyExchange(ref sliceResponse, out ReadOnlyMemory<byte> payloadS2);
            var serverHelloDone = ParseServerHelloDone(ref sliceResponse, out ReadOnlyMemory<byte> payloadS3);
            var changeCipherSpec = ParseServerChangeCipherSpec(ref sliceResponse, out ReadOnlyMemory<byte> payloadS4);
            var serverHandshakeFinished = ParseServerHandshakeFinished(ref sliceResponse, out ReadOnlyMemory<byte> payloadS5);
            var serverApplicationData = ParseServerApplicationData(ref sliceResponse, out ReadOnlyMemory<byte> payloadS6);
        }
    }
}