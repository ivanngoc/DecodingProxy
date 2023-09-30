using System;
using System.Net.Sockets;

namespace IziHardGames.Libs.Proxy.Pairing
{
    public class ConnectionBase : IDisposable
    {
        protected Socket? socket;

        public virtual void Dispose()
        {
            socket = default;
        }
    }
}
