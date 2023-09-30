using System.Runtime.InteropServices;
using IziHardGames.Libs.Buffers.Vectors;
using IziHardGames.Proxy.TcpDecoder;

namespace IziHardGames.Libs.Cryptography.Tls12
{
    [StructLayout(LayoutKind.Explicit)]
    public struct TlsHelloFromClientData
    {
        // ============================ Record Header
        /// <summary>
        /// <see cref="ConstantsTls.HANDSHAKE_RECORD"/>
        /// </summary>
        [FieldOffset(0)] public byte contentType;                      //1
        /// <summary>
        /// <see cref="ConstantsTls.CLIENT_VERSION_TLS10"/> | <see cref="ConstantsTls.CLIENT_VERSION_TLS12"/>
        /// </summary>  
        [FieldOffset(1)] public short protocolVersion;          //2
        [FieldOffset(3)] public short msgSize;                  //2

        // ============================ HandshakeHeader
        [FieldOffset(5)] public byte handshakeMessageType;      //1
        [FieldOffset(6)] public Bytes3 sizeClientHello;         //3

        // ============================ Client Version
        /// <summary>
        /// <see cref="ConstantsTls.CLIENT_VERSION_TLS10"/> | <see cref="ConstantsTls.CLIENT_VERSION_TLS12"/>
        /// </summary>
        [FieldOffset(9)] public short clientVersion;            //2

        // ============================ Client Random
        [FieldOffset(11)] public Bytes32 clientRandom;          //32

        // ============================ Session ID
        /// <summary>
        /// <see cref="ConstantsTls.SESSION_ID_NOT_PROVIDED"/>
        /// </summary>
        [FieldOffset(43)] public byte SessionID;                //1

        // ============================ Cipher Suites
        [FieldOffset(44)] public short lengthCipherSuiteData;
    }
}