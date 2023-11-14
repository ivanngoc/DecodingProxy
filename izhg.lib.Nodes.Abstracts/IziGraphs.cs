using System;
using System.Collections.Generic;
using System.Text;

namespace IziHardGames.Graphs.Abstractions.Lib
{
    public static class IziGraphs
    {
        public readonly static SelectorOfGraph select = new SelectorOfGraph();
        public readonly static SelectorOfGraphStorage storages = new SelectorOfGraphStorage();
    }

    public sealed class SelectorOfGraphStorage
    {

    }
    public sealed class SelectorOfGraph
    {
        public IIziGraph this[Type type] => throw new System.NotImplementedException();
    }

   
}
