using System;
using System.Collections.Generic;
using System.Xml;
using IziHardGames.Libs.XmlGraph.Enums;

namespace IziHardGames.Libs.XmlGraph.Graphs
{
    public class xGraph : IXmlElement
    {
        private string id = string.Empty;
        private Dictionary<int, xGraphElement> elements = new Dictionary<int, xGraphElement>();
        private int counter;

        internal int Add(xGraphElement el)
        {
            counter++;
            int id = counter;
            el.id = id;
            elements.Add(id, el);
            return id;
        }
        internal int Add(xNode node)
        {
            return Add(node as xGraphElement);
        }

        internal xNode NewNode()
        {
            xNode node = new xNode();
            int id = Add(node);
            return node;
        }
        internal xEdge NewEdge()
        {
            var edge = new xEdge();
            int id = Add(edge);
            return edge;
        }
    }


    internal abstract class xGraphElement
    {
        public int id;
        internal int level;
        internal int sibling;
        internal int xmlLine;
    }

    internal class xEdge : xGraphElement
    {
        internal string Action;
        internal xNode? from;
        internal xNode? to;
        internal EAdvanceMode transition;

        internal void From(xNode xNode)
        {
            this.from = xNode;
        }
        internal void To(xNode xNode)
        {
            this.to = xNode;
        }
    }
    internal class xNode : xGraphElement
    {
        public Type? type;
        public string Name => type?.Name ?? "null";
        public string NameFull => type?.FullName ?? "null";
    }
}