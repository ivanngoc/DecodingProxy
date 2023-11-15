using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Async;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeSocketReader : NodeSocket, IFragTakable, IFragFlowNode, IFragProducer, IFragsShowing
    {
        private readonly AsyncSignaler signaler = new AsyncSignaler();
        private readonly Queue<DataFragment> fragments = new Queue<DataFragment>();
        private NodeConnectionControl? control;
        public event Action<DataFragment>? OnReadedEvent;

        internal void SetControl(NodeConnectionControl control)
        {
            this.control = control;
        }

        internal override async Task ExecuteAsync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                if (socket!.Available > 0)
                {
                    DataFragment fragment = DataFragment.Get(socket.Available);
                    int readed = await socket.ReceiveAsync(fragment.buffer, SocketFlags.None).ConfigureAwait(false);
                    fragment.SetLength(readed);
                    lock (fragments)
                    {
                        fragments.Enqueue(fragment);
                    }
                    control!.ReportRead(readed);
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
        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Async | ENodeRunFlags.Sustainable;
        }

        public override ETraits GetTraits()
        {
            return ETraits.FragmentCreating | ETraits.FragmentShowing | ETraits.FragmentGiving;
        }

        public async ValueTask<DataFragment> TakeFragAsync(CancellationToken ct)
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

        public Task<T> ShowFragsAsync<T>(int countToPeek, CancellationToken ct) where T : IEnumerable<DataFragment>
        {
            throw new NotImplementedException();
        }
    }
}
