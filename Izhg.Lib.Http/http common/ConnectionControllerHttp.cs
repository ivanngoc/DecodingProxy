using System;
using System.Net.Sockets;

namespace IziHardGames.Libs.ForHttp.Common
{
    /// <summary>
    /// Subscribe to events that might indicate for closing connection or make it stall
    /// </summary>
    public class ConnectionControllerHttp : IDisposable
    {
        protected Socket? socketOrigin;
        protected Socket? socketClient;

        public virtual void Initlize(Socket client, Socket origin)
        {
            this.socketClient = client;
            this.socketOrigin = origin;
        }

        public virtual void Dispose()
        {
            socketClient = default;
            socketOrigin = default;
        }
    }
}
