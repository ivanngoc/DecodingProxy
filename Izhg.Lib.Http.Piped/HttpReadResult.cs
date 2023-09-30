using HttpDecodingProxy.ForHttp;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public readonly struct HttpReadResult
    {
        public const int STATUS_ERROR = -1;
        public const int STATUS_COMPLETE = 1;

        public readonly HttpBinaryMapped value;
        public readonly int status;

        public HttpReadResult(HttpBinaryMapped item, int status)
        {
            this.value = item;
            this.status = status;
        }
    }
}