using System.Threading.Tasks;
using IziHardGames.Libs.Networking.Pipelines;

namespace HttpDecodingProxy.ForHttp
{
    public class HttpClientPiped<TObj, TReq, TResp> : TcpClientPiped
        where TObj : IHttpObject
        where TReq : IHttpRequest
        where TResp : IHttpResponse
    {
        public async Task<TResp> SendAsync(TReq requst)
        {
            throw new System.NotImplementedException();
        }
    }

    public class HttpClientStandart : HttpClientPiped<HttpObject, HttpRequest, HttpResponse>
    {

    }

    /// <summary>
    /// При создании и запуске автоматически читает сообщения по мере поступленияи помещает их в двусвязный список
    /// </summary>
    public class PipedHttpReader
    {

    }
}