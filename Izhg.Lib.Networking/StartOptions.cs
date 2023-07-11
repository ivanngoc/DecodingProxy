using IziHardGames.Libs.NonEngine.Memory;

namespace HttpDecodingProxy.ForHttp
{

    /// <summary>
    /// Session for Tunnel
    /// </summary>
    public class StartOptions : IDisposable
    {
        public string HostAndPort { get;  set; }
        public string Host { get;  set; }
        public bool IsHttps { get; set; }
        /// <summary>
        /// Message got method CONNECT
        /// </summary>
        public bool IsConnectRequired { get; set; }
        public int HostPort { get;  set; }
        public string HostAddress { get; set; }

        public CancellationTokenSource cts;
        private IPoolReturn<StartOptions> pool;
        public object consumingProvider;

        public T ProviderAs<T>() where T : class
        {
            return consumingProvider as T;
        }

        public void Init(IPoolReturn<StartOptions> pool)
        {
            this.pool = pool;
        }

        public bool ValidateHttps()
        {
            return IsHttps;
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