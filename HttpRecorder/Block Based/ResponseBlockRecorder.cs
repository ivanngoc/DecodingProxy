using IziHardGames.Proxy.Consuming;
using HttpResult = HttpDecodingProxy.ForHttp.HttpObject;

namespace IziHardGames.Proxy.Recoreder
{
    public class ResponseBlockRecorder : BlockRecorder
    {
        internal void OnResponse(Action<DataSource, HttpResult> recieveResponse)
        {
            base.action = recieveResponse;
        }
    }
}