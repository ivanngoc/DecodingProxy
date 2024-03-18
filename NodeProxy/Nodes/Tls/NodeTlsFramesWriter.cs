using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Async.NetStd21;

namespace IziHardGames.NodeProxies.Nodes.Tls
{
    internal class NodeTlsFramesWriter : NodeTls, IFragTakable, IFrameTaker
    {
        private readonly Queue<DataFragment> frags = new Queue<DataFragment>();
        private readonly IziSignaler signalerFrags = new IziSignaler();
        public IFrameTakable? SourceOfFrames { get; set; }

        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Async | ENodeRunFlags.Sustainable;
        }
        internal override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var frame = await SourceOfFrames!.GiveFrameAsync(ct);
                DataFragment fragment = DataFragment.Get(frame.Length);
                fragment.From(frame);
                signalerFrags.SetComplete();
            }
        }

        public async ValueTask<DataFragment> TakeFragAsync(CancellationToken ct)
        {
            await signalerFrags.Await();
            var frag = frags.Dequeue();
            return frag;
        }

        public override bool Validate()
        {
            if (SourceOfFrames == null) throw new NullReferenceException($"{nameof(SourceOfFrames)} is not set!");
            return true;
        }
    }
}
