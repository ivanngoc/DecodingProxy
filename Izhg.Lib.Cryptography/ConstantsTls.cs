using System;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class ConstantsTls
    {
        public const ushort CLIENT_VERSION10 = 0x0301;          //769 bytes decimical: 3;1      
        public const ushort CLIENT_VERSION11 = 0x0302;          //770
        public const ushort CLIENT_VERSION12 = 0x0303;          //771
        public const ushort CLIENT_VERSION30 = 0x0304;          //772

        public const byte HANDSHAKE_RECORD = 0x16;              //22
        public const byte SESSION_ID_NOT_PROVIDED = 0x00;


        public const byte HANDSHAKE_MESSAGE_TYPE_CLIENT_HELLO = 0x01;

        public class ALPN
        {
            public readonly static byte[] h3 = new byte[] { 0x68, 0x33 };
            public readonly static byte[] h2 = new byte[] { 0x68, 0x32 };
            public readonly static byte[] http11 = new byte[] { 0x68, 0x74, 0x74, 0x70, 0x2f, 0x31, 0x2e, 0x31 };
        }
    }
}