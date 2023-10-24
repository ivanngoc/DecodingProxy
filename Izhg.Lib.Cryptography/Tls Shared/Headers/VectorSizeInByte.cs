using System.Runtime.InteropServices;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    [Header]
    public struct VectorSizeInByte
    {
        [FieldOffset(0)] public byte length;
    }
}
