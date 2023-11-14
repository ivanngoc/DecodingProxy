using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Async;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeSocketReader : NodeSocket, IFragGiver, IFragFlowNode, IFragProducer
    {
        private readonly AsyncSignaler signaler = new AsyncSignaler();
        public event Action<DataFragment> OnReadedEvent;
        public NodeSocketReader(NodeConnectionControl control) : base(control)
        {

        }

        internal override async Task ExecuteAsync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                if (socket.Available > 0)
                {
                    DataFragment fragment = DataFragment.Get(socket.Available);
                    int readed = await socket.ReceiveAsync(fragment.buffer, SocketFlags.None).ConfigureAwait(false);
                    fragment.SetLength(readed);
                    lock (fragments)
                    {
                        fragments.Enqueue(fragment);
                    }
                    control.ReportRead(readed);
                    OnReadedEvent?.Invoke(fragment);
                    Console.WriteLine($"Readed buffer:{readed}");
                    signaler.Set();
                }
                else
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }
        }
        public override ENodeRunFlags GetFlags()
        {
            return ENodeRunFlags.Async | ENodeRunFlags.Sustainable;
        }

        public async Task<DataFragment> TakeFragAsync(CancellationToken ct)
        {
            if (await signaler.Await())
            {
                lock (fragments)
                {
                    return fragments.Dequeue();
                }
            }
            else
            {
                throw new TaskCanceledException($"Task cancelled inside {nameof(AsyncSignaler)}");
            }
        }


    }
}
