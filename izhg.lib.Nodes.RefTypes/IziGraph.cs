using System.Collections.Generic;

namespace IziHardGames.Graphs.Abstractions.Lib.ValueTypes
{
    public sealed class IziGraph : IIziGraph
    {
        private IziNodeRelations? relations;
        private RegistryIziNodes nodes;
        private int counter;
    }

    public sealed class IziNode : IIziNode
    {
        public int id;
        /// <summary>
        /// Index in array layout
        /// </summary>
        public int index;
    }
    public sealed class IziEdge : IIziEdge
    {
        public int id;
        /// <summary>
        ///  Index in array layout
        /// </summary>
        public int index;
    }

    public sealed class RegistryIziNodes
    {

    }

    public sealed class IziNodeRelations
    {
        public readonly int[] relationsTypesRegistry;
        public IziNodeRelations(int[] relations)
        {
            relationsTypesRegistry = relations;
        }

        public IziEdge CreateRelationship(IziNode from, IziNode to, int type)
        {
            throw new System.NotImplementedException();
        }
        public IziEdge CreateRelationshipTowards(IziNode from, IziNode to, int type)
        {
            throw new System.NotImplementedException();
        }
    }

    public class AssociatedDatas<T> : IIziNodesAssociations
    {
        private IziGraph? graph;
        private T[]? datas;
    }
}
