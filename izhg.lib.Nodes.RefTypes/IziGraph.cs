using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.Pools.Abstractions.NetStd21;

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
            var result = Interlocked.Increment(ref counter);
            return result;
        }
    }
    public sealed class IziGraph : IIziGraph, IDisposable
    {
        public readonly IndexatorSelector indexators = new IndexatorSelector();
        public readonly NodeAssociation associations = new NodeAssociation();
        public readonly IziNodeNavigator navigator;
        public readonly IziNodeRelations relations;
        internal readonly IdProvider idProvider = new IdProvider();

        private IIziNodesRegistry<IziNode>? nodes;
        private INodeAdvancer? advancer;

        public INodeAdvancer Advancer => advancer ?? throw new NullReferenceException();
        public IIziNodesRegistry<IziNode> Nodes => nodes!;

        public IziGraph()
        {
            relations = new IziNodeRelations(this);
            navigator = new IziNodeNavigator(relations, associations);
        }

        public void SetRegistry(IIziNodesRegistry<IziNode> nodes)
        {
            this.nodes = nodes;
            navigator.SetRegistry(nodes);
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

        public void Dispose(EDisposeOptions options)
        {
            if (options.HasFlag(EDisposeOptions.Indexers))
            {
                indexators.Clear();
            }
            else if (options.HasFlag(EDisposeOptions.IndexersContent))
            {
                indexators.ClearContent();
            }
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(EDisposeOptions.All);
        }
    }

    [Flags]
    public enum EDisposeOptions : int
    {
        All = -1,
        None = 0,
        Indexers,
        IndexersContent,
    }

    public sealed class IziNode : IziGraphItem, IIziNode
    {
        public static IziNode GetNew()
        {
            return new IziNode();
        }
    }
    public sealed class IziEdge : IziGraphItem, IIziEdge
    {
        /// <summary>
        /// Any Value from <see cref="IziNodeRelations.relationsTypesRegistry"/>
        /// </summary>
        public int relationType;

        public IziNode? a;
        public IziNode? b;
    }

    public abstract class IziGraphItem
    {
        public int id;
        /// <summary>
        ///  Index in array layout
        /// </summary>
        public int index;
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

    public sealed class IziNodeNavigator : IIziNodesNavigator
    {
        private readonly IziNodeRelations relations;
        private readonly NodeAssociation assiciations;
        private IIziNodesRegistry<IziNode>? nodes;

        internal IziNodeNavigator(IziNodeRelations relations, NodeAssociation associations)
        {
            this.relations = relations;
            this.assiciations = associations;
        }

        public T FindDescendant<T>(int id)
        {
            throw new NotImplementedException();
        }

        public T FindDescendant<T>(int id, Func<T, bool> predictate)
        {
            throw new System.NotImplementedException();
        }
        public T FromSurround<T>(int id, Func<T, bool> predictate)
        {
            var store = assiciations[typeof(T)] as StdStore<T> ?? throw new NullReferenceException();
            var realtions = relations.GetRelations(id);

            foreach (var edge in realtions.Edges)
            {
                var data = store[edge.b!];
                if (predictate(data)) return data;
            }
            throw new NullReferenceException($"Not founed id:{id}. typeof({typeof(T).FullName})");
        }
        public T DescendantsAt<T>(int offset, IziNode startPoint) where T : class, IEnumerable<IziNode>, new()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Еврестический рекурсивный поиск
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public T First<T>(int id, Func<T, bool> predictate)
        {
            var store = assiciations[typeof(T)] as StdStore<T> ?? throw new NullReferenceException();
            var realtions = relations.GetRelations(id);

            foreach (var edge in realtions.Edges)
            {
                var data = store[edge.b!];
                if (predictate(data)) return data;
                if (TryFirst<T>(edge.b!.id, predictate, store, out var result))
                {
                    return result!;
                }
            }
            throw new NullReferenceException($"Not Founded for id:{id}. type:{typeof(T).FullName}");
        }
        public bool TryFirst<T>(int id, Func<T, bool> predictate, StdStore<T> store, out T result)
        {
            var realtions = relations.GetRelations(id);

            foreach (var edge in realtions.Edges)
            {
                var data = store[edge.b!];
                if (predictate(data))
                {
                    result = data;
                    return true;
                }
                if (TryFirst<T>(edge.b!.id, predictate, store, out result))
                {
                    return true;
                }
            }
            result = default;
            return false;
        }

        internal void SetRegistry(IIziNodesRegistry<IziNode> nodes)
        {
            this.nodes = nodes;
        }
    }

    public sealed class IziNodeRelations : IIziNodesRelations
    {
        private readonly Dictionary<int, NodeRelations> relationsPerId = new Dictionary<int, NodeRelations>();
        public NodeRelations this[IziNode node] { get => GerOrCreateRelations(node); }
        public NodeRelations this[int id] { get => GerOrCreateRelations(id); }

        private readonly IziGraph graph;
        public IziNodeRelations(IziGraph graph)
        {
            this.graph = graph;
        }

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
            IziEdge edge = IziPool.GetConcurrent<IziEdge>();
            edge.a = a;
            edge.b = b;
            edge.id = graph.idProvider.GetId();
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
        public IziNode? node;
        private readonly List<IziEdge> edges = new List<IziEdge>();
        public IEnumerable<IziEdge> Edges => edges;

        public void AddEdge(IziEdge iziEdge)
        {
            edges.Add(iziEdge);
        }
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
        public IEnumerable<T> GetValues() { return values.Values; }
    }
    public class AssociatedDatas<T>
    {
        private IziGraph? graph;
        private T[]? datas;
    }

    public sealed class IndexatorSelector
    {
        private readonly Dictionary<Type, Indexator> pairs = new Dictionary<Type, Indexator>();
        public Indexator this[Type ket, Type type] => throw new System.NotImplementedException();
        public Indexator this[Type type]
        {
            get => pairs[type];
            set => AddOrUpdate(type, value);
        }
        internal IndexatorSelector() { }
        private void AddOrUpdate(Type type, Indexator value)
        {
            if (pairs.TryGetValue(type, out var existed))
            {
                pairs[type] = value;
            }
            else
            {
                pairs.Add(type, value);
            }
        }
        internal void ClearContent()
        {
            foreach (var item in pairs.Values)
            {
                item.Clear();
            }
        }
        internal void Clear()
        {
            pairs.Clear();
        }
    }

    public abstract class Indexator
    {
        public T As<T>() where T : Indexator
        {
            return this as T ?? throw new InvalidCastException($"Current type:{GetType().FullName}. targetType:{typeof(T).FullName}");
        }
        internal abstract void Clear();
    }

    public class Indexator<TKey, TValue> : Indexator
    {
        private readonly Dictionary<TKey, TValue> pairs = new Dictionary<TKey, TValue>();
        public TValue this[TKey type]
        {
            get => pairs[type];
            set => AddOrUpdate(type, value);
        }
        private void AddOrUpdate(TKey type, TValue value)
        {
            if (pairs.TryGetValue(type, out var existed))
            {
                pairs[type] = value;
            }
            else
            {
                pairs.Add(type, value);
            }
        }
        internal override void Clear()
        {
            pairs.Clear();
        }
        public void Indexing(TKey key, TValue value)
        {
            pairs.Add(key, value);
        }
        public TValue Get(TKey key) => pairs[key];
    }

}
