// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IziHardGames
{
    public static class MyLogger
    {
        public static bool IsLogWithFilter { get; set; }
        public static string Filter { get; set; }

        private static readonly object lockLog = new object();
        public static Microsoft.Extensions.Logging.ILogger logger;

        public static void SetFilter(string filter)
        {
            Filter = filter;
            IsLogWithFilter = true;
        }
        public static void LogLine(string msg, string filter, ConsoleColor color = ConsoleColor.Gray)
        {
            if (IsLogWithFilter && filter != Filter)
            {
                return;
            }
            LogLine(msg, color);
        }
        public static void LogLine(string msg, ConsoleColor color = ConsoleColor.Gray)
        {
            if (logger != null)
            {
                logger.Log(LogLevel.Information, msg);
            }
            else
            {
                Task.Run(() =>
                {
                    lock (lockLog)
                    {
                        var cacheColor = Console.ForegroundColor;
                        Console.ForegroundColor = color;
                        Console.WriteLine(GetTimeStamp() + msg);
                        Console.ForegroundColor = cacheColor;
                    }
                });
            }
        }
        public static void LogException(Exception ex)
        {
            if (logger != null)
            {
                logger.LogError(ex.ToString());
            }
            else
            {
                lock (lockLog)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(GetTimeStamp() + ex.ToString());
                    Console.ForegroundColor = color;
                }
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
            if (logger != null)
            {
                logger.Log(LogLevel.Information, msg);
            }
            else
            {
                lock (lockLog)
                {
                    Console.Write(msg);
                }
            }
        }

        public static string GetTimeStamp()
        {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffffff    ");
        }

        public static void LogInformation(string msg)
        {
            if (logger != null)
            {
                logger.LogInformation(msg);
            }
            else
            {
                throw new NullReferenceException($"Logger is not set");
            }
        }
        public static void LogWarning(string msg)
        {
            if (logger != null)
            {
                logger.Log(LogLevel.Warning, msg);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}