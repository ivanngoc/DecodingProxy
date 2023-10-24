using System.Runtime.InteropServices;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.4.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct CompressionMethodValue
    {
        [FieldOffset(0)] public TlsEnums.ECompressionMethod compression_method;
    }
    
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.4.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct CompressionMethod
    {
        [FieldOffset(0)] public byte lengthFollowed;
    }
}
