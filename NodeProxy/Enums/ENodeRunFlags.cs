using System;

namespace IziHardGames.NodeProxies.Nodes
{
    [Flags]
    internal enum ENodeRunFlags
    {
        ErrorNotOverrided = 0,
        Sync = 1 << 0,
        Async = 1 << 1,
        Sustainable = 1 << 2,
        Awaitable = 1 << 3,
        NoAdvancing = 1 << 4,
    }
}
