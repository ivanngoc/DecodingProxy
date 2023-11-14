using System;
using IziHardGames.NodeProxies.Nodes;
using IziHardGames.NodeProxies.Graphs;

namespace IziHardGames.NodeProxies
{
    public class SchemeBuilderMonada
    {
        ProxyGraph proxyGraph = new ProxyGraph();
        internal SchemeBuilderMonada Append<T>() where T : Node
        {
            //proxyGraph.Append();
            throw new NotImplementedException();
        }

        internal void AppendDemux<T>() where T : NodeDemuxBlocking
        {
            throw new NotImplementedException();
        }
    }
}