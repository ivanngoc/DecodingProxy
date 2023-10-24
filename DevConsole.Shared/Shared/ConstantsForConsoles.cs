using System.Net;

namespace DevConsole.Shared.Consoles
{
    public static class ConstantsForConsoles
    {
        public const int SIZE_LOG_HEADER = 32;

        public const int MAGIC_NUMBER_HEADER = 0x3F_7B_0E_A1;
        public const int MAGIC_NUMBER_LOG_HEADER = 0x1F_AE_82_3C;


        public const int SERVER_PORT = 53401;
        public const int TYPE_ID = 5;
        public readonly static IPAddress local = IPAddress.Parse("127.0.0.1");
        public const int MAX_LOGS = 1000;
    }
}