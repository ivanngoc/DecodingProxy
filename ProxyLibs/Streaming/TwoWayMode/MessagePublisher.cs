using HttpDecodingProxy.ForHttp;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class MessagePublisher
    {
        public event Action<HttpProxyMessage> OnMessageEvent;
        public void Subscribe(SubscibeOptions options)
        {
            throw new System.NotImplementedException();
        }
    }
}