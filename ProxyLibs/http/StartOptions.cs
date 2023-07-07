// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;

namespace HttpDecodingProxy.ForHttp
{
    /// <summary>
    /// Session for Tunnel
    /// </summary>
    public class StartOptions : IDisposable
    {
        public string HostAndPort { get; internal set; }
        public string Host { get; internal set; }
        public bool IsHttps { get; internal set; }
        /// <summary>
        /// Message got method CONNECT
        /// </summary>
        public bool IsConnectRequired { get; internal set; }
        public int HostPort { get; internal set; }
        public string HostAddress { get; internal set; }

        public CancellationTokenSource cts;
        private IPoolReturn<StartOptions> pool;
        public ConsumingProvider consumingProvider;

        public void Init(IPoolReturn<StartOptions> pool)
        {
            this.pool = pool;
        }

        public void Dispose()
        {
            cts = default;
            HostAndPort = default;
            Host = default;
            IsHttps = default;
            IsConnectRequired = default;
            HostPort = default;
            HostAddress = default;

            pool.Return(this);
            pool = default;
            consumingProvider = default;
        }
    }
}