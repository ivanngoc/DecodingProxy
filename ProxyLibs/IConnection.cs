// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace IziHardGames.Proxy
{
    public interface IConnection
    {
        object ConnectToProx { get; }

        void Connect();
        /// <summary>
        /// Create Connection with HTTP CONNECT METHOD
        /// </summary>
        void ConnectToProxy();
    }
}