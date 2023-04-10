// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Net.Sockets;

namespace HttpDecodingProxy.http
{
    [Serializable]
    public class HttpResponse : HttpOject
    {

        // https://httpwg.org/specs/rfc9112.html#message.body
        // The presence of a message body in a response, as detailed in Section 6.3,
        // depends on both the request method to which it is responding and the response status code.
        // This corresponds to when response content is allowed by HTTP semantics (Section 6.4.1 of [HTTP]).
        public HttpResponse(HttpProxyMessage httpMessage) : base(httpMessage)
        {
            fields.IsResponse = true;
        }
    }
}