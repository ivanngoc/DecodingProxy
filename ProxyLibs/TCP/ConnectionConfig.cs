using HttpDecodingProxy.ForHttp;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class ConnectionConfig
    {
        public int timeout;
        /// <summary>
        ///  An integer that is the maximum number of requests that can be sent on this connection before closing it. 
        ///  Unless 0, this value is ignored for non-pipelined connections as another request will be sent in the next response. 
        ///  An HTTP pipeline can use it to limit the pipelining.
        /// </summary>
        public int max;
    }
}