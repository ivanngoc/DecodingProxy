using System;
using System.Text;
using Func = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.ValueTask<int>>;

namespace IziHardGames.MappedFrameReader
{
    internal class NodeRepeat : Node, IAdvancingNode
    {
        private ERepeatMode mode;
        private ValuePromise timesAsPromise;
        private Node repeatNode;
        private TableOfResults tableOfResults;
        private NodeResult[] resultsFixed;
        private int times;
        private int i;
        private bool isSeparatorSet;
        private bool isEncloseSet;
        private bool isNodeSet;
        private ReadOnlyMemory<byte> separator;
        private ReadOnlyMemory<byte> enclose;
        private Func func;
        public readonly List<NodeResult> results = new List<NodeResult>();

        public NodeRepeat(TableOfResults tableOfResults) : base()
        {
            this.tableOfResults = tableOfResults;
            this.typeNode = ENodeType.Repeat;
        }
        internal override void Execute(ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
#if DEBUG
            string debug = Encoding.UTF8.GetString(mem.Slice(0, 500).Span);
#endif
            var copy = mem;
            switch (mode)
            {
                case ERepeatMode.None: throw new System.InvalidOperationException();
                case ERepeatMode.Once: throw new System.NotImplementedException();
                case ERepeatMode.WhileCondition:
                    {
                        if (isSeparatorSet)
                        {
                            if (enclose.Length > mem.Length) return taskNotEnough;
                            var span = mem.Span;
                            var slice = span;
                            NodeResult nodeResult = new NodeResult();
                            int offset = default;
                            int vectorCounter = default;
                            string path = this.path;
                            for (int i = 0; i < span.Length; i++)
                            {
                                slice = span.Slice(i);
                                if (slice.StartsWith(enclose.Span))
                                {
                                    int indexEnd = i + enclose.Length;
                                    var result = mem.Slice(0, indexEnd);
                                    this.SetResultStripped(result);
                                    this.SetResult(result);
                                    this.lengthConsumed = indexEnd;

                                    results.Add(nodeResult);
                                    var memResult = mem.Slice(offset, indexEnd - offset);
                                    nodeResult.SetResult(memResult);
                                    nodeResult.SetResultStripped(memResult.Slice(0, memResult.Length - enclose.Length));
                                    tableOfResults.AddDynamic(vectorCounter, path, nodeResult);
                                    goto END;
                                }
                                else if (slice.StartsWith(separator.Span))
                                {
                                    int indexEnd = i + separator.Length;
                                    int lengthValue = indexEnd - offset;
                                    var memResult = mem.Slice(offset, lengthValue);
                                    nodeResult.SetResult(memResult);
                                    nodeResult.SetResultStripped(memResult.Slice(0, memResult.Length - separator.Length));
                                    tableOfResults.AddDynamic(vectorCounter, path, nodeResult);
                                    results.Add(nodeResult);
                                    var newResult = new NodeResult();
                                    newResult.offsetLocal = indexEnd;
                                    offset = indexEnd;
                                    nodeResult = newResult;
                                    vectorCounter++;
                                    i = indexEnd;
                                }
                            }
                            END:
                            break;
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                    }
                case ERepeatMode.Fixed:
                    {
                        if (!isNodeSet) throw new InvalidOperationException("Fixed repeat must be with ");
                        for (; i < times; i++)
                        {
                            var task = repeatNode.ExecuteAsync(copy, readerContext);

                            if (task == taskCanceled)
                            {
                                return taskCanceled;
                            }
                            else if (task == taskNotEnough)
                            {
                                return taskNotEnough;
                            }
                            int consumedNow = repeatNode.lengthConsumed;
                            this.lengthConsumed += consumedNow;
                            copy = copy.Slice(consumedNow);
                            NodeResult nodeResult = new NodeResult();
                            nodeResult.SetResult(repeatNode.GetResult());
                            resultsFixed[i] = nodeResult;
                        }
                        break;
                    }
                case ERepeatMode.FixedFromSource:
                    {
                        if (repeatNode is NodeReadFromSource node)
                        {
                            int length = default;
                            for (int i = 0; i < length; i++)
                            {

                            }
                        }
                        else
                        {
                            throw new System.NotImplementedException();
                        }
                        break;
                    }
                case ERepeatMode.Func:
                    {
                        var t1 = func(mem);
                        lengthConsumed = t1.Result;
                        break;
                    }
                default: throw new System.NotImplementedException();
            }
            return base.ExecuteAsync(mem, readerContext);
        }

        internal void SetRepeatWithNode(ERepeatMode mode, Node node, ValuePromise times)
        {
            this.mode = mode;
            this.timesAsPromise = times;
            this.repeatNode = node;
        }
        internal void SetRepeatWithNode(ERepeatMode mode, Node node)
        {
            this.mode = mode;
            this.repeatNode = node;
            isNodeSet = true;
        }

        internal void SetEnclose(byte[] bytes)
        {
            isEncloseSet = true;
            enclose = bytes.AsMemory();
        }

        internal void SetRepeatWithSeparator(byte[] bytes)
        {
            isSeparatorSet = true;
            separator = bytes.AsMemory();
            mode = ERepeatMode.WhileCondition;
        }

        internal void SetFunc(Func func)
        {
            this.func = func;
            this.mode = ERepeatMode.Func;
        }
    }
}