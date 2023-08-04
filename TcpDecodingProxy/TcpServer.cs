using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Servers;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class TcpServer<T> : ServerBase<T> where T : TcpWrap, IPoolBind<T>
    {
        private IPoolObjects<T> pool;

        public TcpServer(int port, IPoolObjects<T> pool, IClientHandlerAsync<T> handler)
        {
            this.pool = pool;
            listener = new AdapterServer(port, pool);
            clientHandler = new AdapterClientTcp(handler);
        }

        private class AdapterClientTcp : AdapterClient
        {
            private IClientHandlerAsync<T> handler;
            public AdapterClientTcp(IClientHandlerAsync<T> handler)
            {
                this.handler = handler;
            }

            public async override Task<T> HandleClientAsync(T client, CancellationToken token = default)
            {
                await handler.HandleClientAsync(client);
                return client;
            }
        }

        private class AdapterServer : AdapterListener
        {
            private TcpListener listener;
            private int port;
            private IPoolObjects<T> pool;

            public AdapterServer(int port, IPoolObjects<T> pool)
            {
                this.pool = pool;
                this.port = port;
                this.listener = new TcpListener(port);
            }

            public async override Task<T> AcceptClientAsync(CancellationToken token = default)
            {
                var client = await listener.AcceptTcpClientAsync(token);
                //System.Net.Sockets.SocketException: 'An attempt was made to access a socket in a way forbidden by its access permissions.' - порт занят?

                var pool = this.pool;
                var rent = pool.Rent();
                rent.Wrap(client);
                rent.Initilize("Client");
                rent.BindToPool(pool);
                return rent;
            }

            public override void Initilize()
            {
                listener.Start();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}