using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.SocketLevel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace IziHardGames.Proxy.Tcp
{
    public class SocketWrapLogger : ILogger, IPerfTracker
    {
        private List<string> logs = new List<string>();
        private List<object> objs = new List<object>();
        private readonly SocketWrap socketWrap;
        public bool isEnabled;
        private LogLevel logLevel;

        public SocketWrapLogger(SocketWrap socketWrap)
        {
            this.socketWrap = socketWrap;
        }

        public void Log(string log)
        {
            if (isEnabled)
            {
                var text = $"{socketWrap.InfoPrefix} {log}.";
                logs.Add(text);
#if DEBUG
                //Console.WriteLine(text);
#endif
            }
        }     

        public void ReportTime(string msg)
        {
            Log(msg);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            throw new NotImplementedException();
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return isEnabled & this.logLevel == logLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public void PutMsg(object msg)
        {
            objs.Add(msg);
        }
    }
}