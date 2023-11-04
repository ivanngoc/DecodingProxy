using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Buffers.Vectors;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct HeaderForCertificates
    {
        [FieldOffset(0)] private Bytes3 data;
        public int Length => data;
    }

    /// <summary>
    /// https://tls12.xargs.org/certificate.html#server-certificate-detail/annotated
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct HeaderForCertificate
    {
        [FieldOffset(0)] private Bytes3 data;
        public int Length => data;
    }

    /// <summary>
    /// https://tls12.xargs.org/certificate.html#server-certificate-detail/annotated
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct HeaderForCertificateExtensions
    {
        [FieldOffset(0)] private byte lengthMajor;
        [FieldOffset(1)] private byte lengthMinor;
        public ushort Length => BufferReader.ToUshort(lengthMajor, lengthMinor);
    }
}
