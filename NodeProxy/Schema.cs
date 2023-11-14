using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using IziHardGames.Libs.XmlGraph.Schemas;
using IziHardGames.NodeProxies.Nodes;

namespace IziHardGames.NodeProxies
{
    internal class NodeIterator : INodeIterator
    {
        public List<Node> nodes = new List<Node>();

        public void IterateNext(Node from)
        {

        }
        public void IterateFromTo(Node from, Node to)
        {
            throw new System.NotImplementedException();
        }

        internal async Task IterateNextAsync(Node node, CancellationToken ct)
        {
            nodes.Add(node);
            await node.ExecuteAsync(ct).ConfigureAwait(false);
        }
    }
    internal class Schema
    {
        internal static async Task<Schema> FromFileXML(string path)
        {
            var xmlSchema = await XmlSchema.ParseFromFile(path, (name) =>
            {
                return Assembly.GetExecutingAssembly().GetType($"IziHardGames.NodeProxy.Nodes.{name}")!;
            }).ConfigureAwait(false);
            return Schema.CreateSchema(xmlSchema);
        }

        private static Schema CreateSchema(XmlSchema xmlSchema)
        {
            throw new NotImplementedException();
        }

        internal Node GetNextNode(Node node)
        {
            throw new NotImplementedException();
        }
    }
}
