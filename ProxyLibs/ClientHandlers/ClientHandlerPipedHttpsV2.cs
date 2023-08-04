using HttpDecodingProxy.ForHttp;
using IziHardGames.Core;
using IziHardGames.Lib.Networking.Exceptions;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Libs.ObjectsManagment;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Tcp.Tls;
using IziHardGames.Tls;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class ClientHandlerPipedHttpsV2 : IClientHandlerAsync<TcpWrapSsl>
    {
        private ConsumingProvider consumingProvider;
        private X509Certificate2 caRootCert;
        private CertManager certManager;
        private ManagerBase<string, ConnectionsToDomainSsl<TcpWrapSsl>, (string, int)> managerSsl;
        private DataSource dataSource;
        private readonly IChangeNotifier<IConnectionData> monitor;

        public ClientHandlerPipedHttpsV2(ConsumingProvider consumingProvider, Core.IChangeNotifier<IConnectionData> monitorForConnections, Libs.ObjectsManagment.ManagerBase<string, ConnectionsToDomainSsl<TcpWrapSsl>, (string, int)> managerSsl, System.Security.Cryptography.X509Certificates.X509Certificate2 caRootCert, Tls.CertManager certManager)
        {
            this.consumingProvider = consumingProvider;
            this.monitor = monitorForConnections;
            this.caRootCert = caRootCert;
            this.certManager = certManager;
            this.managerSsl = managerSsl;
            this.dataSource = new DataSource($"{nameof(ClientHandlerPipedHttpsV2)}");
        }

        public async Task<TcpWrapSsl> HandleClientAsync(TcpWrapSsl client, CancellationToken token = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            token = cts.Token;

            var pool = PoolObjectsConcurent<HttpPipedIntermediary<ConnectionsToDomainSsl<TcpWrapSsl>, TcpWrapSsl>>.Shared;
            var pool2 = PoolObjectsConcurent<HttpBinaryMapped>.Shared;
            var pool3 = PoolObjectsConcurent<TcpWrapSsl>.Shared;

            client.CreateDefaultPipe();

            using (HttpPipedIntermediary<ConnectionsToDomainSsl<TcpWrapSsl>, TcpWrapSsl> processor = pool.Rent().Init(consumingProvider, managerSsl, pool, monitor))
            {
                using (var msg = await processor.AwaitMsg(HttpLibConstants.TYPE_REQUEST, client, cts, pool2).ConfigureAwait(false))
                {
                    if (msg.FindMethod() == EHttpMethod.CONNECT)
                    {
                        var t1 = client.SendAsync(HttpProxyMessage.response200);
                        var pair = msg.FindHostAndPortFromField();
                        var hub = managerSsl.GetOrCreate($"{pair.Item1}:{pair.Item2}", pair);
                        var t2 = hub.GetOrCreateAsync("Origin", pool3);
                        try
                        {
                            var origin = await t2.ConfigureAwait(false);
                            try
                            {
                                monitor.OnUpdate(origin);
                                var certOrigin = origin.GetRemoteCert();
                                await certManager.OriginTryUpdateAsync(certOrigin).ConfigureAwait(false);
                                var certForged = await certManager.ForgedGetOrCreateCertFromCacheAsync(certOrigin, caRootCert).ConfigureAwait(false);
                                await t1.ConfigureAwait(false);
                                var t3 = client.AuthAsServerAsync(certForged, certOrigin, caRootCert);
                                await t3.ConfigureAwait(false);
                                // at least 1 msg must be proceeded
                                var t4 = processor.MaintainMessagingV2(HttpLibConstants.TYPE_REQUEST, client, origin, cts, consumingProvider.consumeBinaryRequest);
                                var t5 = processor.MaintainMessagingV2(HttpLibConstants.TYPE_RESPONSE, origin, client, cts, consumingProvider.consumeBinaryResponse);
                                await Task.WhenAll(t4, t5).ConfigureAwait(false);
                            }
                            catch (SuddenBreakException)
                            {

                            }
                            finally
                            {
                                if (!origin.Flags.HasFlag(Libs.Networking.Clients.EConnectionFlags.AuthenticatedSslClient)) throw new System.NotImplementedException("Can't be reused in domain's pool");
                                hub.Return(origin);
                            }
                        }
                        catch (StalledConnection ex)
                        {
                            throw ex;
                        }
                    }
                    else
                    {
                        throw new System.NotImplementedException("Нужно ввести для передачи как MITM то есть без прокси и метода Connect");
                    }
                }
            }
            return client;
        }
    }
}