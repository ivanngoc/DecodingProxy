using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    /// <summary>
    /// Позволяет забрать фрагменты
    /// <see cref="IFragTaker"/>
    /// </summary>
    internal interface IFragTakable
    {
        ValueTask<DataFragment> TakeFragAsync(CancellationToken ct);
    }
}
