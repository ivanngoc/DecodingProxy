// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace IziHardGames
{
    public static class Logger
    {
        public static bool IsLogWithFilter { get; set; }
        public static string Filter { get; set; }

        private static readonly object lockLog = new object();

        public static void SetFilter(string filter)
        {
            Filter = filter;
            IsLogWithFilter = true;
        }
        public static void LogLine(string msg, string filter, ConsoleColor color = ConsoleColor.White)
        {
            if (IsLogWithFilter && filter != Filter)
            {
                return;
            }
            LogLine(msg, color);
        }
        public static void LogLine(string msg, ConsoleColor color = ConsoleColor.White)
        {
            lock (lockLog)
            {
                var cacheColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(GetTimeStamp() + msg);
                Console.ForegroundColor = cacheColor;
            }
        }
        public static void LogException(Exception ex)
        {
            lock (lockLog)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(GetTimeStamp() + ex.ToString());
                Console.ForegroundColor = color;
            }
        }
        public static void Log(char c)
        {
            lock (lockLog)
            {
                Console.Write(c);
            }
        }
        public static void Log(string msg)
        {
            lock (lockLog)
            {
                Console.Write(msg);
            }
        }

        public static string GetTimeStamp()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff    ");
        }
    }
}