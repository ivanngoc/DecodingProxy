// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace IziHardGames.Proxy.Http
{
    public struct Utf8
    {
        public const byte LEADING_1_BYTE = 0;                  //0
        public const byte LEADING_2_BYTE = 0b1100_0000;        //110
        public const byte LEADING_3_BYTE = 0b1110_0000;        //1110
        public const byte LEADING_4_BYTE = 0b1111_0000;        //11110
    }
}