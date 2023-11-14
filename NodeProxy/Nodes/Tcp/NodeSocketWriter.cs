using System;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeSocketWriter : NodeSocket, IFragReciever
    {
        public NodeSocketWriter(NodeConnectionControl control) : base(control)
        {

        }
        public override ENodeRunFlags GetFlags()
        {
            return ENodeRunFlags.Sustainable | ENodeRunFlags.Async;
        }
        internal override async Task ExecuteAsync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                DataFragment? frag = null;
                lock (fragments)
                {
                    if (!fragments.TryDequeue(out frag))
                    {
                        goto AWAIT;
                    }
                }
                int writed = await socket.SendAsync(frag!.buffer).ConfigureAwait(false);
                if (writed != frag.Length) throw new InvalidOperationException($"writed:{writed}. Frag length:{frag.Length}");
                control.ReportWrite(writed);
                continue;
                AWAIT:
                await Task.Delay(200).ConfigureAwait(false);
            }
        }
        public void RecieveFragment(DataFragment fragment)
        {
            lock (this)
            {
                fragments.Enqueue(fragment);
            }
        }
    }
}
