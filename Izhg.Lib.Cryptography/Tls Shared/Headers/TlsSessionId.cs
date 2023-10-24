using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.2
    /// </summary>
    [Header]
    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_SESSION_ID)]
    public struct TlsSessionId
    {
        [FieldOffset(0)] public byte lengthFollowed;
    }
}
