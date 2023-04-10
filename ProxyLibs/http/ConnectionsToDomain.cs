namespace IziHardGames.Proxy.Sniffing.Http
{
    public class ConnectionsToDomain
    {
        public List<HttpClientForIntercepting> Clients { get; set; }
        public List<HttpClientForInterceptingSsl> ClientsSsl { get; set; }
    }
}