using System;
using System.Net.Sockets;

namespace IziHardGames.Socks5
{
    /// <summary>
    /// Объект синхронизации
    /// </summary>
    public class ConnectionTunnel
    {
        private Socket? a;
        private Socket? b;
        private int zeroReadsAB;
        private int zeroReadsBA;
        private readonly object locker = new object();

        internal void Bind(TcpClient a, TcpClient b)
        {
            this.a = a.Client;
            this.b = b.Client;
        }

        internal void ZeroReadResetAB()
        {
            lock (locker)
            {
                zeroReadsAB = 0;
            }
        }
        internal void ZeroReadResetBA()
        {
            lock (locker)
            {
                zeroReadsBA = 0;
            }
        }

        /// <summary>
        /// if transfer is NOT reversed (Agent To Origin/Client To Server) call this method to report zero read
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal bool ZeroReadAB()
        {
            lock (locker)
            {
                zeroReadsAB++;
                if (zeroReadsAB > 0 && zeroReadsBA > 0) return true;
            }
            return false;
        }
        /// <summary>
        /// if transfer is Reversed (Origin to Agent/Server To Client) than call this method
        /// </summary>
        /// <returns></returns>
        internal bool ZeroReadBA()
        {
            lock (locker)
            {
                zeroReadsBA++;
                if (zeroReadsAB > 0 && zeroReadsBA > 0) return true;
            }
            return false;
        }

        internal string ToStringInfo()
        {
            return $"zero AB:{zeroReadsAB}\tzeroBA:{zeroReadsBA}";
        }
    }
}
