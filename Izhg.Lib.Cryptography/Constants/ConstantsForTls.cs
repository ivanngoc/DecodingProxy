using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Cryptography
{
    public static class ConstantsForTls
    {
        public const int SIZE_READ_ONLY_MEMORY = 16;
        public const int SIZE_RECORD = 5;
        public const int SIZE_HANDSHAKE_HEADER = 4;
        public const int SIZE_RECORD_HANDSHAKE = SIZE_RECORD + SIZE_HANDSHAKE_HEADER;
        public const int SIZE_RANDOM = 32;
        public const int SIZE_SESSION_ID = 1;
        public const int SIZE_HANDSHAKE_COMPRESSION_METHOD = 1;

        /// <summary>
        /// SSL 3.1 - это TLS 1.0
        /// </summary>
        public const ushort CLIENT_VERSION_TLS10 = 0x0301;          //769 bytes decimical: 3;1      
        /// <summary>
        /// SSL 3.2 - это TLS 1.1
        /// </summary>
        public const ushort CLIENT_VERSION_TLS11 = 0x0302;          //770
        /// <summary>
        /// SSL 3.3 - это TLS 1.2
        /// </summary>
        public const ushort CLIENT_VERSION_TLS12 = 0x0303;          //771
        /// <summary>
        /// 3.4 - это TLS 1.3
        /// </summary>
        public const ushort CLIENT_VERSION_TLS13 = 0x0304;          //772

        public const byte HANDSHAKE_RECORD = 0x16;              //22
        public const byte SESSION_ID_NOT_PROVIDED = 0x00;


        public const byte HANDSHAKE_MESSAGE_TYPE_CLIENT_HELLO = 0x01;

        public class ALPN
        {
            public readonly static byte[] h3 = new byte[] { 0x68, 0x33 };
            public readonly static byte[] h2 = new byte[] { 0x68, 0x32 };
            public readonly static byte[] http11 = new byte[] { 0x68, 0x74, 0x74, 0x70, 0x2f, 0x31, 0x2e, 0x31 };
        }

        /// <summary>
        ///  The server replies with the data "pong". 
        ///  https://tls12.xargs.org/#server-application-data
        /// </summary>
        public static readonly byte[] ServerApplicationData = new byte[] { 0x17, 0x03, 0x03 };
    }
}
