// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Net.Sockets;

namespace HttpDecodingProxy.http
{
    [Serializable]
    public class HttpRequest : HttpOject
    {
        // https://httpwg.org/specs/rfc9112.html#message.body
        // The presence of a message body in a request is signaled by a Content-Length or Transfer-Encoding header field.
        // Request message framing is independent of method semantics.

        public HttpRequest(HttpProxyMessage message) : base(message)
        {
            fields.IsRequest = true;
        }       
    }
}