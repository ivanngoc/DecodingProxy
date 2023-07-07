// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace IziHardGames.Proxy
{
    [Flags]
    public enum EProxyBehaviour : int
    {
        All = -1,

        None = 0,
        /// <summary>
        /// Direct Copy from stream to stream (am i even need as proxy?)
        /// </summary>
        Blind,
        /// <summary>
        /// MITM with fake certs approved with fake Root CA cert. Can change content of http requests/response
        /// </summary>
        MitmInterceptor,
        /// <summary>
        /// MITM. Only recording data
        /// </summary>
        MitmSpy,
        /// <summary>
        /// Response As Origin Server with predefined data. For each request there must be a predefined response
        /// </summary>
        FullFakeOrigin,
        /// <summary>
        /// for predefined requests answer as origin server. But if request is unknown than send it to origin server
        /// </summary>
        HalfFakeOrigin,
    }

    public enum EProxyProtocol
    {
        None,
        Http,
        Https,
        gRPC,
    }
}