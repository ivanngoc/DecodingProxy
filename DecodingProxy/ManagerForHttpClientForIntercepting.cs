using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Sniffing.Http;
using IziHardGames.Proxy.TcpDecoder;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Proxy
{

    public class ManagerForHttpClientForIntercepting
    {
        /// <summary>
        /// Key = HostAddress + HostPort
        /// </summary>
        public ConcurrentBag<HttpClientForIntercepting> items = new ConcurrentBag<HttpClientForIntercepting>();

        [Obsolete]
        public HttpClientForInterceptingSsl GetOrCreateV1(string host)
        {
            throw new System.NotImplementedException();

            Logger.LogLine($"Require connection to: {host}");
            ConcurrentDictionary<string, HttpClientForIntercepting> items = new ConcurrentDictionary<string, HttpClientForIntercepting>();

            HttpClientForInterceptingSsl item = PoolObjects<HttpClientForInterceptingSsl>.Shared.Rent();
            var added = items.GetOrAdd(host, item);

            if (added != item)
            {
                PoolObjects<HttpClientForInterceptingSsl>.Shared.Return(item);
            }
            return (HttpClientForInterceptingSsl)added;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="caRootCert"></param>
        /// <param name="tcpDecodingClient"></param>
        /// <param name="msg"></param>
        /// <param name="httpClient"></param>
        /// <returns>
        /// <see langword="true"/> - if client were created <br/>
        /// <see langword="false"/> if existed client were found
        /// </returns>
        [Obsolete]
        public bool GetOrCreateV1(DecodingProxyServer server, X509Certificate2 caRootCert, TcpDecodingClient tcpDecodingClient, HttpDecodingProxy.http.HttpProxyMessage msg, out HttpClientForIntercepting httpClient)
        {
            throw new System.NotImplementedException();

            ConcurrentDictionary<string, HttpClientForIntercepting> items = new ConcurrentDictionary<string, HttpClientForIntercepting>();

            string host = msg.request.fields.Host;

            if (items.TryGetValue(host, out HttpClientForIntercepting client))
            {
                httpClient = client;
                return false;
            }
            return CreateV1(server, caRootCert, tcpDecodingClient, msg, out httpClient);
        }

        [Obsolete]
        public bool CreateV1(DecodingProxyServer server, X509Certificate2 caRootCert, TcpDecodingClient tcpDecodingClient, HttpDecodingProxy.http.HttpProxyMessage msg, out HttpClientForIntercepting httpClient)
        {
            throw new System.NotImplementedException();

            ConcurrentDictionary<string, HttpClientForIntercepting> items = new ConcurrentDictionary<string, HttpClientForIntercepting>();

            string host = msg.request.fields.Host;

            if (msg.IsHttps)
            {
                var item = PoolObjects<HttpClientForInterceptingSsl>.Shared.Rent();
                var result = (HttpClientForInterceptingSsl)items.AddOrUpdate(host, item, (x, y) => y);

                if (item != result)
                {
                    PoolObjects<HttpClientForInterceptingSsl>.Shared.Return(item);
                }
                result.Init(caRootCert);
                result.AddressAndPort = host;
                httpClient = item;
                return true;
            }
            else
            {
                var item = PoolObjects<HttpClientForIntercepting>.Shared.Rent();
                var result = items.AddOrUpdate(host, item, (x, y) => y);

                if (item != result)
                {
                    PoolObjects<HttpClientForIntercepting>.Shared.Return(item);
                }
                result.AddressAndPort = host;
                httpClient = item;
                return true;
            }
        }


        public void Update()
        {
            foreach (var item in items)
            {
                CheckAlive(item);
            }
        }

        public void DisposeV1(HttpClientForIntercepting connection)
        {
            if (connection is HttpClientForInterceptingSsl c)
            {
                DisposeV1(c);
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
        [Obsolete]
        public void DisposeV1(HttpClientForInterceptingSsl item)
        {
            ConcurrentDictionary<string, HttpClientForIntercepting> items = new ConcurrentDictionary<string, HttpClientForIntercepting>();

            if (items.TryRemove(item.AddressAndPort, out var result))
            {
                if (result is HttpClientForInterceptingSsl https)
                {
                    PoolObjects<HttpClientForInterceptingSsl>.Shared.Return(https);
                }
                else
                {
                    PoolObjects<HttpClientForIntercepting>.Shared.Return(result);
                }
                result.Dispose();
            }

            if (result != item)
            {
                if (result is HttpClientForInterceptingSsl https)
                {
                    PoolObjects<HttpClientForInterceptingSsl>.Shared.Return(https);
                }
                else
                {
                    PoolObjects<HttpClientForIntercepting>.Shared.Return(result);
                }
                item.Dispose();
            }
        }

        private void CheckAlive(HttpClientForIntercepting item)
        {
            ManagerForTasks.CkeckErrors(item.RunTask);
        }
    }
}