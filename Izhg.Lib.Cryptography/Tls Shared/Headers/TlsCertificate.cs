using System.Runtime.InteropServices;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public readonly struct TlsCertificate
    {
        [FieldOffset(0)] public readonly byte length;
    }
}
