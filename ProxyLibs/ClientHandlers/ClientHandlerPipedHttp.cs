using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.Networking.Pipelines.Wraps;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
//using ManagerConnectionsToDomain = IziHardGames.Libs.ObjectsManagment.ManagerBase<string, IziHardGames.Proxy.Tcp.ConnectionsToDomain<IziHardGames.Libs.Networking.Pipelines.TcpClientPiped>, (string, int)>;
//using HttpPipedIntermediary = IziHardGames.Proxy.Sniffing.ForHttp.HttpPipedIntermediary<IziHardGames.Proxy.Tcp.ConnectionsToDomain<IziHardGames.Libs.Networking.Pipelines.TcpClientPiped>, IziHardGames.Libs.Networking.Pipelines.TcpClientPiped>;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Core;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    [Obsolete]
    public class ClientHandlerPipedHttp : IClientHandlerAsync<TcpClientPiped>
    {
        private ConsumingProvider consumingProvider;
        //private ManagerConnectionsToDomain manager;
        private readonly IChangeNotifier<IConnectionData> monitor;
        //public ClientHandlerPipedHttp(ConsumingProvider consumingProvider, IChangeNotifier<IConnectionData> monitorForConnections, ManagerConnectionsToDomain manager)
        //{
        //    this.consumingProvider = consumingProvider;
        //    this.monitor = monitorForConnections;
        //    this.manager = manager;
        //}
        public async Task<TcpClientPiped> HandleClientAsync(TcpClientPiped agent, CancellationToken token = default)
        {
            //var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            //var pool = PoolObjectsConcurent<HttpPipedIntermediary>.Shared;
            //using (HttpPipedIntermediary intermediary = pool.Rent().Init(consumingProvider, manager, pool, monitor))
            //{
            //    await intermediary.Run(agent, cts).ConfigureAwait(false);
            //}
            //Console.WriteLine($"{nameof(ClientHandlerPipedHttp)} AcceptClient loop ended");
            //return agent;
            throw new System.NotImplementedException();
        }
    }
}