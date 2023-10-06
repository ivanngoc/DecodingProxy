using System;
using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Buffers.Vectors;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Tls;

namespace IziHardGames.Libs.Cryptography.Tls12
{
    [HandshakeStage(Stage = EHandshakeStage.All, SideAccepting = EHandshakeSide.All)]
    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_RECORD)]
    public readonly struct TlsRecord
    {
        [FieldOffset(0)] private readonly byte type;
        [FieldOffset(1)] private readonly ProtocolVersion pv;
        [FieldOffset(1)] private readonly ushort protocolVersion;
        [FieldOffset(3)] private readonly ushort length0Follows;
        public ETlsTypeRecord TypeRecord => (ETlsTypeRecord)type;
        public ushort Length => BufferReader.ReverseEndians(length0Follows);
        public ushort ProtocolVersion => BufferReader.ReverseEndians(protocolVersion);
        public ETlsProtocolVersion Version => (ETlsProtocolVersion)ProtocolVersion;
        public bool Validate()
        {
            return TypeRecord == ETlsTypeRecord.Handshake || TypeRecord == ETlsTypeRecord.ChangeCipherSpec || TypeRecord == ETlsTypeRecord.ApplicationData || TypeRecord == ETlsTypeRecord.AlertRecord;
        }
        public bool ValidateAsChangeCipherSpec()
        {
            if (TypeRecord != ETlsTypeRecord.ChangeCipherSpec) return false;
            return true;
        }

        public string ToStringInfo()
        {
            return $"TYPE:{TypeRecord}; Version:{Version}; Length:{Length}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_RECORD_HANDSHAKE)]
    public readonly struct HandshakeRecord
    {
        [FieldOffset(0)] public readonly TlsRecord record;
        [FieldOffset(5)] public readonly HandshakeHeader handshakeHeader;

        public bool ValidateAsServerCertificate()
        {
            return handshakeHeader.ValidateAsServerCertificate();
        }

    }


    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_HANDSHAKE_HEADER)]
    public readonly struct HandshakeHeader
    {
        [FieldOffset(0)] public readonly byte messageType;
        [FieldOffset(1)] public readonly Bytes3 length1Follows;

        public int Length => (int)length1Follows;
        public ETlsTypeHandshakeMessage Type => (ETlsTypeHandshakeMessage)messageType;

        public bool Validate()
        {
            return true;
        }
        public bool ValidateAsServerCertificate()
        {
            if (messageType == (byte)ETlsTypeHandshakeMessage.Certificate)
            {
                return true;
            }
            return false;
        }

        public string ToStringInfo()
        {
            return $"HANDSHAKE_TYPE:{Type}; Length:{Length}";
        }
    }

    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.4.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_HANDSHAKE_HEADER)]
    public struct HelloRequest
    {

    }

    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.4.1
    /// </summary>

    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_RANDOM)]
    public struct TlsRandom
    {
        /// <summary>
        /// Not Tested for Correct
        /// </summary>
        [FieldOffset(0)] private uint gmt_unix_time;
        [FieldOffset(4)] public Bytes28 random;
        /// <summary>
        /// Not Tested for Correct
        /// </summary>
        public uint  Seconds => BufferReader.ReverseEndians(gmt_unix_time);
        /// <summary>
        /// Not Tested for Correct
        /// </summary>
        public DateTime DateTime => new DateTime().AddSeconds(Seconds);
    }

    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct ProtocolVersion
    {
        [FieldOffset(0)] public byte major;
        [FieldOffset(1)] public byte minor;
    }

    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-6.2.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 5)]
    public struct TLSPlaintext
    {
        [FieldOffset(0)] public TlsEnums.ContentType type;
        [FieldOffset(1)] public ProtocolVersion version;
        [FieldOffset(3)] private ushort length;
        // opaque fragment[TLSPlaintext.length];
    }
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
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
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct CipherSuiteServer
    {
        [FieldOffset(0)] public TlsEnums.CipherSuite CipherSuite;

        [FieldOffset(0)] private byte major;
        [FieldOffset(1)] private byte minor;
    }

    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.4.1
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct CompressionMethod
    {
        [FieldOffset(0)] public TlsEnums.CompressionMethod compression_method;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 1)]
    public struct VectorSizeInByte
    {
        [FieldOffset(0)] public byte length;
    }

    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct VectorSizeInUshort
    {
        [FieldOffset(0)] private byte major;
        [FieldOffset(1)] private byte minor;

        public ushort Length => BufferReader.ToUshort(major, minor);
    }


    /// <summary>
    /// <see cref="TlsEnums.ContentType"/>
    /// </summary>
    public enum ETlsTypeRecord : byte
    {
        ChangeCipherSpec = 0x14,
        AlertRecord = 0x15,
        Handshake = 0x16,
        ApplicationData = 0x17,
    }

    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7
    /// </summary>
    public enum ETlsTypeHandshakeMessage : byte
    {
        HelloRequest = 0x00,            //0
        ClientHello = 0x01,             //1
        ServerHello = 0x02,             //2
        Certificate = 0x0B,             //11
        ServerKeyExchange = 0x0c,       //12
        CertificateRequest = 0xd,       //13
        ServerHelloDone = 0x0e,         //14
        CertificateVerify = 0xf,        //15
        ClientKeyExchange = 0x10,       //16
        Finished = 0x14,                //20                      
    }
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3 - 6.1.  Connection States
    /// </summary>
    public class TlsEnums
    {
        public enum ContentType : byte
        {
            change_cipher_spec = 20,
            alert = 21,
            handshake = 22,
            application_data = 23,
        }

        /// <summary>
        /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
        /// </summary>
        /// Value there presented in network order. order Must be reversed to match actual because ushort is Big Endian
        public enum CipherSuite : ushort
        {
            //                                          // ACtual network order
            TLS_RSA_WITH_NULL_MD5 = 0x01_00,            //{ 0x00,0x01 }
            TLS_RSA_WITH_NULL_SHA = 0x02_00,            //{ 0x00,0x02 };
            TLS_RSA_WITH_NULL_SHA256 = 0x3B_00,         //{ 0x00,0x3B }
            TLS_RSA_WITH_RC4_128_MD5 = 0x00_00,         // 0x00, 0x04 };
            TLS_RSA_WITH_RC4_128_SHA = 0x00_00,         // 0x00, 0x05 };
            TLS_RSA_WITH_3DES_EDE_CBC_SHA = 0x00_00,    // 0x00, 0x0A };
            TLS_RSA_WITH_AES_128_CBC_SHA = 0x00_00,     // 0x00, 0x2F };
            TLS_RSA_WITH_AES_256_CBC_SHA = 0x00_00,     // 0x00, 0x35 };
            TLS_RSA_WITH_AES_128_CBC_SHA256 = 0x00_00,  // 0x00, 0x3C };
            TLS_RSA_WITH_AES_256_CBC_SHA256 = 0x00_00,  // 0x00, 0x3D };

            TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA = 0x0D_00, //{ 0x00,0x0D };
            TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA = 0x10_00, //{ 0x00, 0x10 };
            TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA = 0x13_00, //{ 0x00, 0x13 };
            TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA = 0x16_00, //{ 0x00, 0x16 };
            TLS_DH_DSS_WITH_AES_128_CBC_SHA = 0x30_00, //{ 0x00, 0x30 };
            TLS_DH_RSA_WITH_AES_128_CBC_SHA = 0x31_00, //{ 0x00, 0x31 };
            TLS_DHE_DSS_WITH_AES_128_CBC_SHA = 0x32_00, //{ 0x00, 0x32 };
            TLS_DHE_RSA_WITH_AES_128_CBC_SHA = 0x33_00, //{ 0x00, 0x33 };
            TLS_DH_DSS_WITH_AES_256_CBC_SHA = 0x36_00, //{ 0x00, 0x36 };
            TLS_DH_RSA_WITH_AES_256_CBC_SHA = 0x37_00, //{ 0x00, 0x37 };
            TLS_DHE_DSS_WITH_AES_256_CBC_SHA = 0x38_00, //{ 0x00, 0x38 };
            TLS_DHE_RSA_WITH_AES_256_CBC_SHA = 0x39_00, //{ 0x00, 0x39 };
            TLS_DH_DSS_WITH_AES_128_CBC_SHA256 = 0x3E_00, //{ 0x00, 0x3E };
            TLS_DH_RSA_WITH_AES_128_CBC_SHA256 = 0x3F_00, //{ 0x00, 0x3F };
            TLS_DHE_DSS_WITH_AES_128_CBC_SHA256 = 0x40_00, //{ 0x00, 0x40 };
            TLS_DHE_RSA_WITH_AES_128_CBC_SHA256 = 0x67_00, //{ 0x00, 0x67 };
            TLS_DH_DSS_WITH_AES_256_CBC_SHA256 = 0x68_00, //{ 0x00, 0x68 };
            TLS_DH_RSA_WITH_AES_256_CBC_SHA256 = 0x69_00, //{ 0x00, 0x69 };
            TLS_DHE_DSS_WITH_AES_256_CBC_SHA256 = 0x6A_00, //{ 0x00, 0x6A };
            TLS_DHE_RSA_WITH_AES_256_CBC_SHA256 = 0x6B_00, //{ 0x00, 0x6B };

            TLS_DH_anon_WITH_RC4_128_MD5 = 0x18_00, //{ 0x00, 0x18 };
            TLS_DH_anon_WITH_3DES_EDE_CBC_SHA = 0x1B_00, //{ 0x00, 0x1B };
            TLS_DH_anon_WITH_AES_128_CBC_SHA = 0x34_00, //{ 0x00, 0x34 };
            TLS_DH_anon_WITH_AES_256_CBC_SHA = 0x3A_00, //{ 0x00, 0x3A };
            TLS_DH_anon_WITH_AES_128_CBC_SHA256 = 0x6C_00, //{ 0x00, 0x6C };
            TLS_DH_anon_WITH_AES_256_CBC_SHA256 = 0x6D_00, //{ 0x00, 0x6D };
        }

        // enum { null(0), (255) } CompressionMethod;
        public enum CompressionMethod : byte
        {
            Null = 0,
        }
    }
}
