using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    internal interface IFragGiver
    {
        Task<DataFragment> TakeFragAsync(CancellationToken ct);
    }
}
