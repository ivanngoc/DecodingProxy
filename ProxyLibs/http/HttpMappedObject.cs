// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Libs.IO;

namespace HttpDecodingProxy.ForHttp
{

    /// <summary>
    /// Request/Response
    /// </summary>
    public class HttpMappedObject
    {
        public HttpBinary binary;
        public List<HttpItemMap> maps = new List<HttpItemMap>(16);

        public int Add(byte[] bytes, int start, int length)
        {
            StringHelper.TryFindLineEndingUtf8(bytes, start, length);
            throw new System.NotImplementedException();
        }
    }
}