namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class HttpChannel : IDisposable
    {
        private HttpPipedIntermediary reader;
        private HttpWriter writer;
        private int bytes;

        public void Dispose()
        {
            bytes = default;
            reader = default;
            writer = default;
        }
    }
    public class HttpWriter
    {

    }
}