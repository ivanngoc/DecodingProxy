using System;
using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography.Tls;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{

    [HandshakeStage(Stage = EHandshakeStage.All, SideAccepting = ESide.All)]
    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_RECORD)]
    [Header]
    public readonly struct TlsRecord
    {
        [FieldOffset(0)] private readonly byte type;
        [FieldOffset(1)] private readonly ProtocolVersion pv;
        [FieldOffset(1)] private readonly ushort protocolVersion;
        [FieldOffset(3)] private readonly ushort lengthFollows;
        public ushort Length => BufferReader.ReverseEndians(lengthFollows);
        public ushort ProtocolVersion => BufferReader.ReverseEndians(protocolVersion);
        public ETlsProtocol TypeProtocol => (ETlsProtocol)type;
        public ETlsProtocolVersion Version => (ETlsProtocolVersion)ProtocolVersion;

        public bool IsTls()
        {
            return Validate()
                && (Version == ETlsProtocolVersion.Tls13 || Version == ETlsProtocolVersion.Tls12 || Version == ETlsProtocolVersion.Tls11 || Version == ETlsProtocolVersion.Tls10)
                && Length > 0;
        }
        public bool Validate()
        {
            return TypeProtocol == ETlsProtocol.Handshake || TypeProtocol == ETlsProtocol.ChangeCipherSpec || TypeProtocol == ETlsProtocol.ApplicationData || TypeProtocol == ETlsProtocol.AlertRecord;
        }
        public bool ValidateAsChangeCipherSpec()
        {
            if (TypeProtocol != ETlsProtocol.ChangeCipherSpec) return false;
            return true;
        }
        public string ToStringInfo()
        {
            return $"Protocol:{TypeProtocol};\tVersion:{Version};\tLength:{Length}\t";
        }
        public static ETlsProtocol GetType(in ReadOnlyMemory<byte> mem)
        {
            return (ETlsProtocol)mem.Span[0];
        }
        public static ETlsProtocol GetType(byte b)
        {
            return (ETlsProtocol)b;
        }
        public static TlsRecord FromMemory(in ReadOnlyMemory<byte> x) => BufferReader.ToStruct<TlsRecord>(in x);
        public static string GetStringInfo(in ReadOnlyMemory<byte> x)
        {
            return FromMemory(in x).ToStringInfo();
        }
    }
}

namespace IziHardGames.Libs.Cryptography.Tls12
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-6.2.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 5)]
    public struct TLSPlaintext
    {
        [FieldOffset(0)] public TlsEnums.ContentType type;
        [FieldOffset(1)] public ProtocolVersion version;
        [FieldOffset(3)] private ushort length;
    }
}
