// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using HttpDecodingProxy.http;
using IziHardGames.Libs.NonEngine.Memory;
using System.Net.Sockets;

namespace IziHardGames.Proxy
{
    public class ManagerForHttpMessages
    {
        public HttpProxyMessage Initiate(NetworkStream stream)
        {
            HttpProxyMessage message = PoolObjects<HttpProxyMessage>.Shared.Rent();
            message.manager = this;
            message.ReadMsgInto(stream, stream, message.request);
            return message;
        }

        internal HttpProxyMessage GetNew()
        {
            HttpProxyMessage message = PoolObjects<HttpProxyMessage>.Shared.Rent();
            message.manager = this;
            return message;
        }

        public void Return(HttpProxyMessage msg)
        {
            PoolObjects<HttpProxyMessage>.Shared.Return(msg);
        }
    }
}