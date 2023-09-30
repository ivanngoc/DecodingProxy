// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System;
using System.Net.Sockets;

namespace HttpDecodingProxy.ForHttp
{
    public class HttpResponse<T>
    {
        private T item;
    }

    [Serializable]
    public class HttpResponse : HttpObject, IHttpResponse
    {
        public string Status { get => throw new System.NotImplementedException(); }
        public HttpResponse() : base()
        {
            this.type = ConstantsForHttp.TYPE_RESPONSE;
            this.fields.IsResponse = true;
        }
    }

    public interface IHttpRequest
    {

    }

    public interface IHttpResponse
    {
        public string Status { get; }
    }

    public interface IHttpObject
    {

    }
}