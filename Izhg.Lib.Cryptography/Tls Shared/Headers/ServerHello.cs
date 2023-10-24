using System.Runtime.InteropServices;
using IziHardGames.Libs.Cryptography.Attributes;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct ServerHello
    {
        [FieldOffset(0)] public ProtocolVersion version;
        [FieldOffset(2)] public TlsRandom random;
        [ByteLength]
        [FieldOffset(2)] public VectorSizeInByte session_id;

        /*
         struct {
          ProtocolVersion server_version;
          Random random;
          SessionID session_id;
          CipherSuite cipher_suite;
          CompressionMethod compression_method;
          select (extensions_present) {
              case false:
                  struct {};
              case true:
                  Extension extensions<0..2^16-1>;
              };
         } ServerHello;
         */
    }
}
