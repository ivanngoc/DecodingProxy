using System;
using IziHardGames.Graphs.Abstractions.Lib;

namespace IziHardGames.NodeProxies.Nodes
{
    [Flags]
    internal enum ETraits : int
    {
        All = -1,
        ErrorNotOverrided = 0,
        CreateFragment = 1 << 0,
        CreateFrame = 1 << 1,
        /// <summary>
        /// Run on another thread and live until kill
        /// </summary>
        Sustainable = 1 << 2,
        Async = 1 << 3,
        sync = 1 << 4,
        /// <summary>
        /// Need To call <see cref="IIziNodeAdvancingAdapter"/>
        /// </summary>
        Advancing = 1 << 5,
    }

    [Flags]
    internal enum EDynamicStates : int
    {
        All = -1,
        None = 0,
        Demux = 1 << 0,
        Mux = 1 << 1,
    }
}
