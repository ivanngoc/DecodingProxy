using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    internal interface IFrameTakable
    {
        public ValueTask<DataFrame> GiveFrameAsync(CancellationToken ct);
    }

    internal interface IFrameTaker
    {
        public IFrameTakable? SourceOfFrames { get; set; }
    }

    internal interface IFrameProducer
    {

    }
}
