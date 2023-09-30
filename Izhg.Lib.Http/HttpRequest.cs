using System;

namespace HttpDecodingProxy.ForHttp
{
    [Serializable]
    public class HttpRequest : HttpObject, IHttpRequest
    {
        public HttpRequest() : base()
        {
            this.type = ConstantsForHttp.TYPE_REQUEST;
            this.fields.IsRequest = true;
        }
    }
}