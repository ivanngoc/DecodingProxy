using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Tasking.NetStd21.Abstractions;

namespace IziHardGames.NodeProxies.Nodes
{
    internal interface IFragsPushToPeekerer
    {
        public Monada<bool> GiveToPeek();
    }
    /// <summary>
    /// <see cref="IFragsShowing"/>
    /// <see cref="ERelations.FragPeek"/>
    /// </summary>
    internal interface IFragsPeeker
    {
        internal void SetSourceForShowing(IFragsShowing source);
    }

    internal interface IFragsShowing
    {
        Task<T> ShowFragsAsync<T>(int countToPeek, CancellationToken ct) where T : IEnumerable<DataFragment>;
    }
}
