using System.Runtime.InteropServices;
using IziHardGames.Libs.Cryptography.Shared.Headers;

namespace IziHardGames.Libs.Cryptography.Tls12
{
    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_RECORD_HANDSHAKE)]
    [Header]
    public readonly struct HandshakeRecord
    {
        [FieldOffset(0)] public readonly TlsRecord record;
        [FieldOffset(5)] public readonly HandshakeHeader handshakeHeader;

        public bool ValidateAsServerCertificate()
        {
            return handshakeHeader.ValidateAsServerCertificate();
        }

    }
}
