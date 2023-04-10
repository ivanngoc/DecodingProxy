// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace IziHardGames.Proxy
{
    public class ProxySettings
    {
        public bool IsDefined { get; set; }
        /// <summary>
        /// HostName Or IP Address without port
        /// </summary>
        public string Address { get; set; }
        public string Port { get; set; }

    }
}