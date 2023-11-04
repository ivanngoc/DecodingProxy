using System.Net.Security;
using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.5
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    [Header]
    public struct CipherSuiteValue
    {
        [FieldOffset(0)] public TlsEnums.ECipherSuite CipherSuite;
        [FieldOffset(0)] private byte major;
        [FieldOffset(1)] private byte minor;

        public System.Net.Security.TlsCipherSuite TlsCipherSuite => (TlsCipherSuite)BufferReader.ToUshort(major, minor);
    }
}
