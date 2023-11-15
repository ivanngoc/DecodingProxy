using System;

namespace IziHardGames.NodeProxies.Nodes
{
    /// <summary>
    /// Relation indicate Relation [From]=>[To]
    /// </summary>
    [Flags]
    internal enum ERelations : int
    {
        All = -1,

        None = 0,
        /// <summary>
        /// One Node Can Have Multiple Next nodes. If that occurs [From] node will be treated as Demultiplexor (Demux). <see cref="EDynamicStates.Demux"/>
        /// </summary>
        Next = 1 << 0,
        /// <summary>
        /// One Node Can Have Multiple Previous Nodes. If that occurs [From] node will be treated as Multiplexor (Mux) <see cref="EDynamicStates.Mux"/>
        /// </summary>
        Previous = 1 << 1,
        /// <summary>
        /// Create Copy Of <see cref="DataFragment"/> without ownership
        /// </summary>
        CopyFragment = 1 << 2,
        /// <summary>
        /// Change ownership of <see cref="DataFragment"/> and transfer object itself
        /// </summary>
        GiveFragment = 1 << 3,
        /// <summary>
        /// Pullout <see cref="DataFragment"/> and change ownership to self
        /// </summary>
        TakeFragment = 1 << 4,
        /// <summary>
        /// [From]=>[To]. [From] Required [To]. When [From] Running it also Start [To]
        /// </summary>
        Required = 1 << 5,
        /// <summary>
        /// After Execution 
        /// </summary>
        TransitionAfterComplete = 1 << 6,
        /// <summary>
        /// Not Consuming <see cref="DataFragment"/> But reading 
        /// <see cref="IFragsShowing"/>
        /// </summary>
        FragPeek = 1 << 7,
        /// <summary>
        /// [To] get information from [From] and can stop execution of [From]
        /// </summary>
        ControllingObserver = 1 << 8,
        /// <summary>
        /// [From] contain references that [To] use
        /// </summary>
        RefSource = 1 << 9,
    }
}
