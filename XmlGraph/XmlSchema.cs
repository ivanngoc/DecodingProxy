using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using IziHardGames.Libs.XmlGraph.Enums;
using IziHardGames.Libs.XmlGraph.Graphs;
using static IziHardGames.Libs.XmlGraph.Schemas.ConstantsForXmlGraph;
using Func = IziHardGames.Libs.XmlGraph.Schemas.TypeSearch;

namespace IziHardGames.Libs.XmlGraph.Schemas
{
    public delegate Type TypeSearch(string name);
    internal class BuildContext
    {
        public int currentLevel;
        public int currentSiblind;
        public TypeSearch typeSearch;

        public void DecrementLevel()
        {
            currentLevel--;
        }
        public void IncrementLevel()
        {
            currentLevel++;
        }
        public void IncrementSibling()
        {
            currentSiblind++;
        }
        public void DecrementSibling()
        {
            currentSiblind--;
        }
    }

    internal static class ConstantsForXmlGraph
    {
        public const string NAMESPACE_NODES = "IziHardGames.Libs.XmlGraph.Graphs";
    }
    public class XmlSchema
    {

        private List<xGraph> pipes = new List<xGraph>();
        public static async Task<XmlSchema> ParseFromFile(string path, Func func)
        {
            string s = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            return ParseFromString(s, func);
        }
        public static XmlSchema Parse(byte[] bytes, Func func)
        {
            var str = Encoding.UTF8.GetString(bytes);
            return ParseFromString(str, func);
        }
        public static XmlSchema ParseFromString(string value, Func func)
        {
            var doc = XDocument.Parse(value, LoadOptions.SetLineInfo);
            var root = doc.Element("NodeProxy");
            var schema = root.Element("Schema");
            var pipes = schema.Elements("Pipe");
            var result = new XmlSchema();

            foreach (var pipe in pipes)
            {
                var title = pipe.Attribute("Title")?.Value ?? "[No Title]";
                Console.WriteLine($"Pipe with title:{title}");
                xGraph graph = BuildPipes(pipe, func);
                result.AddPipe(graph);
            }
            return result;
        }

        private void AddPipe(xGraph graph)
        {
            pipes.Add(graph);
        }

        internal static XmlSchema ParseFromStringV1(string value)
        {
            var doc = XDocument.Parse(value);
            var nodes = doc.Nodes();

            foreach (var node in nodes)
            {
                Console.WriteLine(node.NodeType);
                var reader = node.CreateReader();
                Console.WriteLine(reader.Name);
            }

            throw new NotImplementedException();
        }

        internal static xGraph BuildPipes(XElement pipe, Func typeSearch)
        {
            xGraph xGraph = new xGraph();
            BuildContext context = new BuildContext();
            context.typeSearch = typeSearch;

            var head = pipe.Element("Head")!;
            var headType = head.Attribute("Type")!.Value;
            xNode headNode = new xNode();
            //Type? type = Assembly.GetExecutingAssembly().GetType($"{NAMESPACE_NODES}.{headType}");
            Type type = typeSearch(headType);
            headNode.type = type;
            int idHead = xGraph.Add(headNode);
            BuildNextNodeRecursive(context, xGraph, head, headNode);
            return xGraph;
        }
        internal static void BuildNextNodeRecursive(BuildContext context, xGraph xGraph, XElement parentEl, xNode parentNode)
        {
            var elements = parentEl.Elements();
            int sibling = context.currentSiblind;
            context.currentSiblind = default;

            foreach (var element in elements)
            {
                var info = (IXmlLineInfo)element;
                Console.WriteLine($"Element:{element.Name.ToString()}. line:{info.LineNumber}. Pos:{info.LinePosition}");
                string name = element.Name.ToString();

                switch (name)
                {
                    case nameof(EElement.Demux):
                        {
                            BuildDemux(context, xGraph, element, parentNode);
                            break;
                        }
                    case nameof(EElement.Node):
                        {
                            context.IncrementSibling();
                            context.IncrementLevel();
                            xNode node = xGraph.NewNode();
                            node.level = context.currentLevel;
                            string headType = element.Attribute("Type")!.Value;
                            Type type = context.typeSearch(headType) ?? throw new NullReferenceException($"Not Founded Type:{headType}");
                            node.type = type;
                            var advanceMode = element.Attribute("AdvanceMode");
                            if (advanceMode != null)
                            {
                                string advanceModeValue = advanceMode.Value!;
                                var mode = Enum.Parse<EAdvanceMode>(advanceModeValue);
                                xEdge xEdge = xGraph.NewEdge();
                                xEdge.From(parentNode);
                                xEdge.To(node);
                                xEdge.transition = mode;
                            }
                            BuildNextNodeRecursive(context, xGraph, element, node);
                            context.DecrementLevel();
                            break;
                        }
                    case nameof(EElement.Switch):
                        {
                            BuildNextNodeRecursive(context, xGraph, element, parentNode);
                            break;
                        }
                    case nameof(EElement.Case):
                        {
                            BuildNextNodeRecursive(context, xGraph, element, parentNode);
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException(name);
                        }
                }
            }
            context.currentSiblind = sibling;
        }

        private static void BuildDemux(BuildContext context, xGraph xGraph, XElement element, xNode parentNode)
        {
            BuildNextNodeRecursive(context, xGraph, element, parentNode);
        }
    }
}
