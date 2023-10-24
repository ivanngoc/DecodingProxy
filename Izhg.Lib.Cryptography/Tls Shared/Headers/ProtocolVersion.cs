using System;
using System.Runtime.InteropServices;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    [Header]
    public struct ProtocolVersion
    {
        [FieldOffset(0)] public byte major;
        [FieldOffset(1)] public byte minor;
        public string Version => GetStringInfo(major, minor);

        public static string GetStringInfo(in ReadOnlyMemory<byte> x)
        {
            if (x.Length < 2) throw new ArgumentException();
            var span = x.Span;
            if (span[0] == 0x03)
            {
                if (span[1] == 0x01) return nameof(ConstantsForTls.CLIENT_VERSION_TLS10);
                if (span[1] == 0x02) return nameof(ConstantsForTls.CLIENT_VERSION_TLS11);
                if (span[1] == 0x03) return nameof(ConstantsForTls.CLIENT_VERSION_TLS12);
                if (span[1] == 0x04) return nameof(ConstantsForTls.CLIENT_VERSION_TLS13);
            }
            return $"Format error: {ParseByte.ByteToHexFormated(span[0])};{ParseByte.ByteToHexFormated(span[1])}";
        }
        public static string GetStringInfo(byte first, byte second)
        {
            if (first == 0x03)
            {
                if (first == 0x01) return nameof(ConstantsForTls.CLIENT_VERSION_TLS10);
                if (first == 0x02) return nameof(ConstantsForTls.CLIENT_VERSION_TLS11);
                if (first == 0x03) return nameof(ConstantsForTls.CLIENT_VERSION_TLS12);
                if (first == 0x04) return nameof(ConstantsForTls.CLIENT_VERSION_TLS13);
            }
            return $"Format error: {ParseByte.ByteToHexFormated(first)};{ParseByte.ByteToHexFormated(second)}";
        }
    }
}
