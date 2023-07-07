using IziHardGames.Proxy.Consuming;
using HttpResult = HttpDecodingProxy.ForHttp.HttpObject;

namespace IziHardGames.Proxy.Recoreder
{
    public class RequestBlockRecorer : BlockRecorder
    {
        internal void OnRequest(Action<DataSource, HttpResult> recieveRequest)
        {
            base.action = recieveRequest;
        }
    }
}