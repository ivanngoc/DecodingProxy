using System.Text;
using Func = System.Func<System.ReadOnlyMemory<byte>, int>;
using FuncAsync = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.Task<int>>;
using Wrap = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.ValueTask<int>>;

namespace IziHardGames.MappedFrameReader
{
    /// <summary>
    /// Собирает результаты из предыдущих <see cref="Node"/> которые отнесены к этому результату.
    /// Создан для разделения события когда результат готов если например было чтение сложного объекта из последовательности <see cref="Node"/>
    /// </summary>
    public class NodeResult : Node
    {
        /// <summary>
        /// в отличие от <see cref="Node.offset"/> представляет собой смещение относительно входного аргумента <see cref="ReadOnlyMemory{T}"/>.
        /// </summary>
        public int offsetLocal;
        private Node begin;
        private Node end;
        private Node target;
        private Node source;
        private EResultCollectingMode mode;
        internal EResultCollectingMode Mode => mode;

        internal Node Target => target;
        public FuncAsync HandlerAsyncWithBlock { get; set; }
        public FuncAsync HandlerAsyncNonBlock { get; set; }
        public Func HandlerSync { get; set; }
        private Wrap wrapFunc;

        public ReadOnlyMemory<byte> ResultDeepCopy;
        private int length;
        public int Length => length;
        public readonly List<NodeResult> results = new List<NodeResult>();
        public string Debug => Encoding.UTF8.GetString(ResultDeepCopy.Span);

        public NodeResult() : base()
        {
            this.typeNode = ENodeType.Result;
        }
        internal override ReadOnlyMemory<byte> GetResultStripped()
        {
            return ResultDeepCopy;
        }

        internal override void Execute(ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }
        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext context)
        {
#if DEBUG
            string debug = Encoding.UTF8.GetString(mem.Slice(0, 500).Span);
#endif
            switch (mode)
            {
                case EResultCollectingMode.None: throw new System.NotImplementedException();
                case EResultCollectingMode.SingleTarget:
                    {
                        var slice = source.GetResult().ToArray().AsMemory();
                        this.SetResult(slice);
                        this.SetResultStripped(slice);
                        return taskSucceed;
                    }
                case EResultCollectingMode.FromTo:
                    {
                        this.offset = begin.offset;
                        var last = context.NodesQueue.nodes.Last();
                        var node = last;
                        while (node != begin)
                        {
                            node = node.Head;
                        }
                        int sumLength = default;
                        while (node != default)
                        {
                            if (node == end) break;
                            if (node is NodeResult result)
                            {
                                sumLength += result.length;
                                results.Add(result);
                            }
                            node = node.Tail;
                        }
                        if (results.Count > 0)
                        {
                            var first = results.First();
                            last = results.Last();
                            //this.offset = first.offset;
                        }
                        length = sumLength;
                        var slice = context.Slice(this.offset, length);
                        SetResult(slice);
                        SetResultStripped(slice);
                        return taskSucceed;
                    }
                case EResultCollectingMode.Repeated:
                    {
                        if (source is NodeRepeat repeatNode)
                        {
                            this.results.AddRange(repeatNode.results);
                            this.length = this.results.Sum(x => x.length);
                            this.ResultDeepCopy = repeatNode.GetResult().ToArray().AsMemory();
                            return taskSucceed;
                        }
                        throw new NullReferenceException();
                    }

                case EResultCollectingMode.Func:
                    {
                        var t1 = wrapFunc.Invoke(mem);
                        length = t1.Result;
                        this.ResultDeepCopy = mem.Slice(0, length).ToArray().AsMemory();
                        return taskSucceed;
                    }
                default: throw new System.NotImplementedException();
            }
        }
        internal void ForFrame(NodeFrameBegin begin, NodeFrameEnd end, DefinedType definedType)
        {
            FromTo(begin, end, definedType);
            this.target = begin;
        }
        internal void For(Node target)
        {
            this.target = target;
        }

        internal void From(NodeRepeat source, DefinedType definedType)
        {
            this.definedType = definedType;
            this.mode = EResultCollectingMode.Repeated;
            this.source = source;

        }
        internal void From(Node source, DefinedType definedType)
        {
            this.definedType = definedType;
            this.source = source;
            this.mode = EResultCollectingMode.SingleTarget;
        }
        internal void FromFunc(Wrap func, DefinedType definedType)
        {
            this.definedType = definedType;
            this.wrapFunc = func;
            this.mode = EResultCollectingMode.Func;
        }
        internal void FromTo(Node begin, Node end, DefinedType definedType)
        {
            this.definedType = definedType;
            this.mode = EResultCollectingMode.FromTo;
            this.begin = begin;
            this.end = end;
        }
        internal string ResultAsString()
        {
            if (ResultDeepCopy.Length > 0)
            {
                return Encoding.UTF8.GetString(ResultDeepCopy.Span);
            }
            return $"{GetType().Name}.{path}: Zero Length Result";
        }

        internal bool IsValid()
        {
            return true;
        }

        internal override void SetResult(ReadOnlyMemory<byte> readOnlyMemory)
        {
            base.SetResult(readOnlyMemory);
            this.length = readOnlyMemory.Length;
            this.ResultDeepCopy = readOnlyMemory.ToArray().AsMemory();
        }


        public override void Dispose()
        {
            base.Dispose();
            results.Clear();
            this.begin = default;
            this.end = default;
            this.target = default;
            this.source = default;
        }

    }

    internal enum EResultCollectingMode
    {
        None,
        SingleTarget,
        FromTo,
        Repeated,
        Func,
    }
}