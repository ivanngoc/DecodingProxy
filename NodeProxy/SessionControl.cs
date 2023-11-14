using System;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes.SOCKS5
{
    internal class SessionControl
    {
        private NodeConnectionControl? proxtToAgent;
        private NodeConnectionControl? proxtToOrigin;

        internal Task SessionCompleted(CancellationToken ct)
        {
            return Task.Run(async () =>
             {
                 await proxtToAgent!.AwaitCompletion(ct).ConfigureAwait(false);
                 await proxtToOrigin!.AwaitCompletion(ct).ConfigureAwait(false);
             });
        }

        internal void SetControls(NodeConnectionControl proxtToAgent, NodeConnectionControl proxtToOrigin)
        {
            this.proxtToAgent = proxtToAgent;
            this.proxtToOrigin = proxtToOrigin;
        }
    }
}
