using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Pipelines.Wraps
{
    public class TcpWrapPiped : IDisposable
    {
        protected TcpClient client;
        protected PipeReader reader;
        protected string title;
        protected string host;
        protected int port;

        public async Task Connect(string host, int port)
        {
            this.host = host;
            this.port = port;
            client = new TcpClient();
            var t1 = client.ConnectAsync(host, port);
            await t1.ConfigureAwait(false);
        }

        public void AsPipe()
        {
            reader = PipeReader.Create(client.GetStream());
        }

        public async Task SendAsync(byte[] bytes, int offset, int count, CancellationToken token)
        {
            await client.GetStream().WriteAsync(bytes, offset, count, token);
        }

        public virtual void Dispose()
        {
            client.Dispose();
            client = default;
            title = default;
            host = default;
            port = default;
        }
    }
}
