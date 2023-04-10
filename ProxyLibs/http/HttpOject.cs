using System.Net.Sockets;
using System.Text;

namespace HttpDecodingProxy.http
{
    [Serializable]
    public class HttpOject
    {
        public StringBuilder sb = new StringBuilder(8096);
        public readonly Http11Fields fields = new Http11Fields();
        public readonly HttpBody body = new HttpBody();
        public bool IsErrorReadMsg;
        [NonSerialized] public readonly HttpProxyMessage httpMessage;
        public virtual bool IsBodyPresented => throw new NotImplementedException();

        public HttpOject(HttpProxyMessage message)
        {
            this.httpMessage = message;
        }

        public void Write(Stream stream)
        {
            stream.Write(Encoding.UTF8.GetBytes(sb.ToString()));
            body.WriteTo(stream);
        }
        public void Dispose()
        {
            sb.Length = 0;
            IsErrorReadMsg = false;
            body.Dispose();
            fields.Dispose();
        }
    }
}