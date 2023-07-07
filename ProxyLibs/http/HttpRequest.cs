// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Libs.NonEngine.Memory;
using System.Net.Sockets;

namespace HttpDecodingProxy.ForHttp
{
    [Serializable]
    public class HttpRequest : HttpObject, IHttpRequest
    {
        public HttpRequest() : base()
        {
            this.type = HttpLibConstants.TYPE_REQUEST;
            this.fields.IsRequest = true;
        }
    }
}