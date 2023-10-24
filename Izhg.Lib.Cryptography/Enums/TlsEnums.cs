namespace IziHardGames.Libs.Cryptography.Tls12
{
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
        public enum ECipherSuite : ushort
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
        public enum ECompressionMethod : byte
        {
            Null = 0,
        }
    }
}
