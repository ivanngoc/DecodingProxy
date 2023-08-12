using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Tls;
using System.Security.Cryptography.X509Certificates;
//using ManagerConnectionsToDomainSsl = IziHardGames.Libs.ObjectsManagment.ManagerBase<string, IziHardGames.Proxy.Tcp.Tls.ConnectionsToDomainTls<IziHardGames.Libs.Networking.Pipelines.TcpClientPipedSsl, IziHardGames.Libs.Networking.Pipelines.TcpClientPipedSsl>, (string, int)>;
//using HttpPipedIntermediarySsl = IziHardGames.Proxy.Sniffing.ForHttp.HttpPipedIntermediary<IziHardGames.Proxy.Tcp.Tls.ConnectionsToDomainTls<IziHardGames.Libs.Networking.Pipelines.TcpClientPipedSsl, IziHardGames.Libs.Networking.Pipelines.TcpClientPipedSsl>, IziHardGames.Libs.Networking.Pipelines.TcpClientPipedSsl>;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Core;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class ClientHandlerPipedHttps : IClientHandlerAsync<TcpClientPipedSsl>
    {
        private ConsumingProvider consumingProvider;
        //private ManagerConnectionsToDomainSsl managerSsl;
        private CertManager certManager;
        private X509Certificate2 caRootCert;
        private readonly IChangeNotifier<IConnectionData> monitor;

        //public ClientHandlerPipedHttps(ConsumingProvider consumingProvider, IChangeNotifier<IConnectionData> monitor, ManagerConnectionsToDomainSsl manager, CertManager certManager, X509Certificate2 caRootCert)
        //{
        //    this.monitor = monitor;
        //    this.consumingProvider = consumingProvider;
        //    this.managerSsl = manager;
        //    this.certManager = certManager;
        //    this.caRootCert = caRootCert;
        //}

        public async Task<TcpClientPipedSsl> HandleClientAsync(TcpClientPipedSsl agent, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
//            var provider = consumingProvider;
//            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

//            var pool = PoolObjectsConcurent<HttpPipedIntermediarySsl>.Shared;
//            var pool2 = PoolObjectsConcurent<TcpClientPipedSsl>.Shared;
//            // не запускать или приостанавливать чтение до рукопожатия TLS. так как используется базовый метод чтения. или заменить базовые методы на pipelines
//            var taskFill = agent.RunWriterLoop();

//            TcpClientPipedSsl origin;

//            using (HttpPipedIntermediarySsl parserRequest = pool.Rent().Init(consumingProvider, managerSsl, pool, monitor))
//            {
//                using (var req = await parserRequest.AwaitMsg(HttpLibConstants.TYPE_REQUEST, agent, cts, PoolObjectsConcurent<HttpBinaryMapped>.Shared).ConfigureAwait(false))
//                {
//                    var pair = req.FindHostAndPortFromField();
//                    var hub = managerSsl.GetOrCreate($"{pair.Item1}:{pair.Item2}", pair);
//                    var t1 = hub.GetOrCreateAsync("Origin Ssl", pool2);
//                    Task t2 = Task.CompletedTask;

//                    if (req.FindMethod() == EHttpMethod.CONNECT)
//                    {
//                        t2 = agent.SendAsync(HttpProxyMessage.response200, cts.Token);

//                        if (hub.TryGetForgedCert(out X509Certificate2 forgedOrigin))
//                        {
//                            origin = await t1.ConfigureAwait(false);
//                        }
//                        else
//                        {
//                            origin = await t1.ConfigureAwait(false);
//                            forgedOrigin = await certManager.ForgedGetOrCreateCertFromCacheAsync(origin.GetRemoteCert(), caRootCert).ConfigureAwait(false);
//                            hub.SetForgedCert(forgedOrigin);
//                        }
//                        await t2.ConfigureAwait(false);
//                        var t3 = agent.AuthAsSrverAsync(forgedOrigin);
//                        await t3.ConfigureAwait(false);
//                    }
//                    else
//                    {
//                        origin = await t1.ConfigureAwait(false);
//                        await origin.SendAsync(req.GetMemory(), default);

//                        using (var response = await parserRequest.AwaitMsg(HttpLibConstants.TYPE_REQUEST, agent, cts, PoolObjectsConcurent<HttpBinaryMapped>.Shared).ConfigureAwait(false))
//                        {
//                            await agent.SendAsync(response.GetMemory(), default);
//                            if (response.IsCloseRequired)
//                            {
//                                throw new System.NotImplementedException();
//                            }
//                        }
//                    }
//                    var t5 = parserRequest.MaintainMessagingV2(HttpLibConstants.TYPE_REQUEST, agent, origin, cts, provider.consumeBinaryRequest);
//                    var t6 = parserRequest.MaintainMessagingV2(HttpLibConstants.TYPE_RESPONSE, origin, agent, cts, provider.consumeBinaryResponse);
//                    await Task.WhenAll(t5, t6).ConfigureAwait(false);
//                }
//            }
//            await Task.WhenAll(taskFill).ConfigureAwait(false);
//#if DEBUG
//            Console.WriteLine($"{nameof(ClientHandlerPipedHttps)} {nameof(HandleClientAsync)} loop ended");
//#endif   
//            return agent;
        }

    }
}