using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    /// <summary>
    /// Позволяет забрать фрагменты
    /// </summary>
    internal interface IFragTakable
    {
        ValueTask<DataFragment> TakeFragAsync(CancellationToken ct);
    }
}
