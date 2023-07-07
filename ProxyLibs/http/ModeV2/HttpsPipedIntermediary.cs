using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Sniffing.ForHttp;
using IziHardGames.Proxy.Tcp.Tls;
using IziHardGames.Tls;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Proxy.Sniffing.ForHttps
{
    public class HttpsPipedIntermediary : HttpPipedIntermediary, IDisposable
    {
        private CertManager certManager;
        private X509Certificate2 forgedOriginCert;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(CertManager certManager)
        {
            this.certManager = certManager;
        }

        public async Task Run(TcpClientPiped agent, CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            using (agent)
            {
                Task t1 = agent.FillPipeAsync(cts);
                var reader = agent.reader;

                using (var initMsg = await AwaitMsg(HttpLibConstants.TYPE_REQUEST, agent, cts, PoolObjectsConcurent<HttpBinary>.Shared).ConfigureAwait(false))
                {
                    using (TcpClientPiped origin = PoolObjectsConcurent<TcpClientPiped>.Shared.Rent())
                    {
                        var pair = initMsg.GetHostAndPortFromField();
                        var t2 = origin.ConnectAsync(pair.Item1, pair.Item2);

                        var t3 = agent.SendAsync(HttpLibConstants.Responses.bytesOk11, cts.Token);

                        /// <see cref="ConnectionToOriginTls.ConnectSsl(ConnectionsToDomainTls, string, int)"/>
                        using (SslStream sslAgent = new SslStream(agent))
                        {
                            X509Certificate2 forgedOriginCert = null;
                            /// <see cref="ConnectionToAgentTls.ForgeSslConnection"/>
                            var t4 = sslAgent.AuthenticateAsServerAsync(forgedOriginCert);

                            //SslStream sslOrigin = new SslStream();
                        }
                    }
                }
            }
        }
    }
}