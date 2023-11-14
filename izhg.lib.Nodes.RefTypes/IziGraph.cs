using System;
using System.Buffers;
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
        public readonly NodeAssociation associations = new NodeAssociation();
        private readonly IziNodeRelations relations = new IziNodeRelations();
        private readonly IdProvider idProvider = new IdProvider();

        private IIziNodesRegistry<IziNode> nodes;
        private INodeAdvancer? advancer;
        public INodeAdvancer Advancer => advancer ?? throw new NullReferenceException();

        public void SetRegistry(IIziNodesRegistry<IziNode> nodes)
        {
            this.nodes = nodes;
        }
        public void SetAdvancer<T>(T advancer) where T : INodeAdvancer
        {
            this.advancer = advancer;
            advancer.FromGraph(this);
            advancer.SetRelations(relations);
        }

        public static IziGraph GetNew<T>(T advancer, IIziNodesRegistry<IziNode> nodes) where T : INodeAdvancer
        {
            IziGraph iziGraph = PoolObjectsConcurent<IziGraph>.Shared.Rent();
            iziGraph.SetAdvancer(advancer);
            iziGraph.SetRegistry(nodes);
            return iziGraph;
        }
        public IziNode GetNewNode()
        {
            var node = IziNode.GetNew();
            node.id = idProvider.GetId();
            nodes.Add(node);
            return node;
        }
    }

    public sealed class IziNode : IIziNode
    {
        public int id;
        /// <summary>
        /// Index in array layout
        /// </summary>
        public int index;
        public static IziNode GetNew()
        {
            return new IziNode();
        }
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

    // см. izhg.Lib.Collections
    public sealed class RegistryIziNodesV2<T> : IIziNodesRegistry<T>
        where T : IIziNode
    {
        private T[] values;
        private int length;
        public T this[int id] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        private void EnsureIncrement()
        {
            if (length == values.Length)
            {
                values = ArrayPool<T>.Shared.Rent(length + 1);
            }
            throw new System.NotImplementedException();
        }
        public void Add(T node)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class RegistryIziNodes : IIziNodesRegistry<IziNode>
    {
        private readonly Dictionary<int, IziNode> nodes = new Dictionary<int, IziNode>();
        public IziNode this[int id] { get => nodes[id]; set => UpdateOrAdd(id, value); }
        private void UpdateOrAdd(int id, IziNode value)
        {
            if (nodes.TryGetValue(id, out var existed))
            {
                nodes[id] = value;
            }
            else
            {
                nodes.Add(id, value);
            }
        }
        public void Add(IziNode node)
        {
            nodes.Add(node.id, node);
        }
    }

    public sealed class IziNodeRelations : IIziNodesRelations
    {
        private readonly Dictionary<int, NodeRelations> relationsPerId = new Dictionary<int, NodeRelations>();
        public NodeRelations this[IziNode node] { get => GerOrCreateRelations(node); }
        public NodeRelations this[int id] { get => GerOrCreateRelations(id); }
        private NodeRelations GerOrCreateRelations(IziNode node)
        {
            int id = node.id;
            if (relationsPerId.TryGetValue(id, out var relations)) return relations;
            relations = new NodeRelations();
            relations.node = node;
            this.relationsPerId.Add(id, relations);
            return relations;
        }
        private NodeRelations GerOrCreateRelations(int id)
        {
            if (relationsPerId.TryGetValue(id, out var relations)) return relations;
            relations = new NodeRelations();
            this.relationsPerId.Add(id, relations);
            IziNode node = null;
            relations.node = node ?? throw new NotImplementedException();
            return relations;
        }

        public IziEdge CreateRelationship(IziNode a, IziNode b, int type)
        {
            throw new System.NotImplementedException();
        }
        public IziEdge CreateRelationshipTowards(IziNode a, IziNode b, int type)
        {
            throw new System.NotImplementedException();
        }
        public NodeRelations GetRelations(int id) => relationsPerId[id];
        public NodeRelations GetRelations(IziNode iziNode) => relationsPerId[iziNode.id];
    }

    public sealed class NodeRelations
    {
        public IziNode node;
        public readonly List<IziEdge> edges = new List<IziEdge>();
    }

    public sealed class NodeAssociation : IIziNodesAssociations
    {
        private readonly Dictionary<Type, Store> storesByType = new Dictionary<Type, Store>();
        public Store this[Type type] { get => storesByType[type]; set => AddOrUpdateStore(type, value); }
        public void AddOrUpdateStore(Type type, Store store)
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
    public sealed class StdStore<T> : Store
    {
        private Dictionary<int, T> values = new Dictionary<int, T>();
        public T this[IziNode node] { get => values[node.id]; }
        public T this[int id] { get => values[id]; }
        public void Associate(IziNode iziNode, T node)
        {
            int id = iziNode.id;
            values.Add(id, node);
        }
    }
    public class AssociatedDatas<T>
    {
        private IziGraph? graph;
        private T[]? datas;
    }
}
