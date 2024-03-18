using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Async.NetStd21;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography;
using IziHardGames.Libs.Cryptography.Shared.Headers;

namespace IziHardGames.NodeProxies.Nodes.Tls
{
    internal class NodeTlsFramesReader : NodeTls, IFrameProducer, IFragTaker
    {
        private readonly Queue<DataFrame> frames = new Queue<DataFrame>();
        private IziSignaler signaler = new IziSignaler();
        private DataFrame? currentFrame;
        private bool isFragmented;
        private int lengthLeft;
        public IFragTakable? SourceToTakeFrom { get; set; }

        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Sustainable | ENodeRunFlags.Async;
        }

        internal override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var frag = await SourceToTakeFrom!.TakeFragAsync(ct);
                var mem = frag.ReadOnly;

                while (mem.Length > 0)
                {
                    if (currentFrame == null)
                    {
                        TlsRecord record = BufferReader.ToStruct<TlsRecord>(mem);
                        int totalLength = record.Length + ConstantsForTls.SIZE_RECORD;
                        currentFrame = DataFrame.Allocate(totalLength);
                        currentFrame.type = EFrameType.Tls;
                        bool isFragmented = mem.Length < totalLength;
                        this.isFragmented = isFragmented;
                        int lengthToCopy = isFragmented ? mem.Length : totalLength;
                        var data = mem.Slice(0, lengthToCopy);
                        currentFrame.CopySet(data);
                        if (isFragmented)
                        {
                            lengthLeft = totalLength - lengthToCopy;
                        }
                        mem = mem.Slice(lengthToCopy);
                    }
                    else
                    {
                        int lengthToCopy = lengthLeft > mem.Length ? mem.Length : lengthLeft;
                        var data = mem.Slice(0, lengthToCopy);
                        currentFrame.CopyAppend(data);
                        lengthLeft -= lengthToCopy;
                        mem = mem.Slice(lengthToCopy);
                    }
                    if (lengthLeft == 0)
                    {
                        frames.Enqueue(currentFrame);
                        signaler.SetComplete();
                        currentFrame = null;
                    }
                }
            }
        }

        public override bool Validate()
        {
            if (SourceToTakeFrom == null) throw new NullReferenceException($"{nameof(SourceToTakeFrom)} is not set!");
            return true;
        }
    }
}
