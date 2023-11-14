using System;
using System.Collections.Generic;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Graphs.Abstractions.Lib.ValueTypes
{
    /// <summary>
    /// Создает Id для <see cref="IziNode.id"/> и <see cref="IziEdge.id"/>
    /// </summary>
    public sealed class IdProvider : IIdProvider
    {
        private int counter;
        public int GetId()
        {
            return counter++;
        }
    }
    public sealed class IziGraph : IIziGraph
    {
        private IziNodeRelations relations = new IziNodeRelations();
        private NodeAssociation associations = new NodeAssociation();
        private RegistryIziNodes nodes = new RegistryIziNodes();
        private IdProvider idProvider = new IdProvider();
        private INodeAdvancer? advancer;
        public INodeAdvancer Advancer => advancer ?? throw new NullReferenceException();

        public void SetAdvancer<T>(T advancer) where T : INodeAdvancer
        {
            this.advancer = advancer;
            advancer.SetRelations(relations);
        }

        public static IziGraph GetNew<T>(T advancer) where T : INodeAdvancer
        {
            IziGraph iziGraph = PoolObjectsConcurent<IziGraph>.Shared.Rent();
            iziGraph.SetAdvancer(advancer);
            return iziGraph;
        }
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
        /// <summary>
        /// Any Value from <see cref="IziNodeRelations.relationsTypesRegistry"/>
        /// </summary>
        public int relationType;

        public IziNode? a;
        public IziNode? b;
    }

    public sealed class RegistryIziNodes
    {

    }

    public sealed class IziNodeRelations : IIziNodesRelations
    {
        public readonly Dictionary<int, NodeRelations> relationsPerId = new Dictionary<int, NodeRelations>();

        public IziEdge CreateRelationship(IziNode a, IziNode b, int type)
        {
            throw new System.NotImplementedException();
        }
        public IziEdge CreateRelationshipTowards(IziNode a, IziNode b, int type)
        {
            throw new System.NotImplementedException();
        }
    }

    public sealed class NodeRelations
    {
        public IziNode node;
        public readonly List<IziEdge> edges = new List<IziEdge>();
    }

    public sealed class NodeAssociation : IIziNodesAssociations
    {
        private readonly Dictionary<Type, Store> storesByType = new Dictionary<Type, Store>();
        public Store this[Type type] { get => storesByType[type]; set => CreateOrUpdateStore(type, value); }
        public void CreateOrUpdateStore(Type type, Store store)
        {
            if (storesByType.TryGetValue(type, out Store existed))
            {
                storesByType[type] = store;
            }
            else
            {
                RegistStore(type, store);
            }
        }
        public void RegistStore(Type type, Store store)
        {
            storesByType.Add(type, store);
        }
        public void RegistStore<T>(Store store)
        {
            storesByType.Add(typeof(T), store);
        }
    }

    public abstract class Store : IIziNodesAssociationsStore
    {

    }

    public class AssociatedDatas<T>
    {
        private IziGraph? graph;
        private T[]? datas;
    }
}
