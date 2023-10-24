using System.Runtime.InteropServices;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.4.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_HANDSHAKE_HEADER)]
    public struct HelloRequest
    {

    }
}
