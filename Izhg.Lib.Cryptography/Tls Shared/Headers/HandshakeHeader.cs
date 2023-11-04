using System;
using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Buffers.Vectors;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_HANDSHAKE_HEADER)]
    [Header]
    public readonly struct HandshakeHeader
    {
        /// <summary>
        /// <see cref="ETlsTypeHandshakeMessage"/>
        /// </summary>
        [FieldOffset(0)] public readonly byte messageType;
        [FieldOffset(1)] private readonly Bytes3 length1Follows;

        public int Length => (int)length1Follows;
        public ETlsTypeHandshakeMessage Type => (ETlsTypeHandshakeMessage)messageType;

        public bool Validate()
        {
            return true;
        }
        public bool ValidateAsServerCertificate()
        {
            if (messageType == (byte)ETlsTypeHandshakeMessage.Certificate)
            {
                return true;
            }
            return false;
        }

        public string ToStringInfo()
        {
            return $"HANDSHAKE_TYPE:{Type}; Length:{Length}";
        }

        public static HandshakeHeader FromMemory(in ReadOnlyMemory<byte> x) => BufferReader.ToStruct<HandshakeHeader>(in x);
        public static string GetStringInfo(in ReadOnlyMemory<byte> x)
        {
            return FromMemory(in x).ToStringInfo();
        }
    }
}
