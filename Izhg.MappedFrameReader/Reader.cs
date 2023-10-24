using System.Buffers;
using Func = System.Func<System.ReadOnlyMemory<byte>, int>;


namespace IziHardGames.MappedFrameReader
{
    internal class ReaderContext
    {
        private Reader reader;
        /// <summary>
        /// Current buffer
        /// </summary>
        internal ReadOnlyMemory<byte> bufferSingle;
        internal NodesQueue NodesQueue => reader.nodesQueue;
        public int position;
        internal TableOfResults tableOfResults;

        public ReaderContext(Reader reader)
        {
            this.reader = reader;
        }

        public ReadOnlyMemory<byte> Slice(int offset, int length)
        {
            return bufferSingle.Slice(offset, length);
        }

        public void Advance(int length)
        {
            position += length;
        }
    }

    // Совмещены идеи: JsonElement (Node-Reading); Mapping; 
    public class Reader
    {
        internal EReadType readType;
        internal ReadProgress Progress => throw new System.NotImplementedException();
        private int offset;
        private Node? Current { get; set; }

        private Adapter adapter;
        private ReportFunc resultHandler;
        public readonly Scheme scheme;
        internal readonly ReadResults results = new ReadResults();
        internal readonly NodesQueue nodesQueue = new NodesQueue();
        internal readonly ReaderContext readerContext;
        internal readonly TableOfResults tableOfResults = new TableOfResults();
        public Reader(Scheme scheme)
        {
            readerContext = new ReaderContext(this);
            readerContext.tableOfResults = tableOfResults;

            this.scheme = scheme;
            Initilize(scheme);
        }

        private void Initilize(Scheme scheme)
        {
            var nodes = scheme.Graph.nodes;

            foreach (var node in nodes)
            {
                if (node.value.isWriteToTable)
                {
                    results.AddSlot(node.value);
                }
            }
        }

        private bool MoveNext()
        {
            throw new System.NotImplementedException();
        }

        public async Task ReadAllAsync(byte[] data)
        {
            Console.WriteLine();
            Console.WriteLine();
            Current = scheme.Head;
            ReadOnlyMemory<byte> mem = new ReadOnlyMemory<byte>(data);
            ReadOnlyMemory<byte> slice = mem;
            readerContext.bufferSingle = slice;

            while (Current != null)
            {
                var node = Current;
                Console.WriteLine($"NodeType:{node.GetType()}\tpath:{node.path}. Position:{readerContext.position}");
                nodesQueue.Push(node);
                node.offset = readerContext.position;

                var t1 = node.ExecuteAsync(slice, readerContext);
                node.lengthDiff = readerContext.position - node.offset;
                //Console.WriteLine($"ExecuteAsync.\tDiff:{node.lengthDiff}\tType:[{node.GetType().Name}]\tPath:[{node.path}]\toffset:{node.offset}");
                if (t1.IsFaulted)
                    throw new System.NotImplementedException("Task Is Faulted");
                await t1.ConfigureAwait(false);

                int consumed = node.lengthConsumed;
                slice = slice.Slice(consumed);

                if (node is NodeResult nodeResult)
                {
                    var result = nodeResult.GetResult();

                    if (nodeResult.IsValid())
                    {
                        if (nodeResult.HandlerSync is not null)
                        {
                            nodeResult.HandlerSync.Invoke(result);
                            Console.WriteLine($"Invoke HandlerSync");
                        }
                        if (nodeResult.HandlerAsyncNonBlock is not null)
                        {
                            var task = nodeResult.HandlerAsyncNonBlock.Invoke(result);
                            Console.WriteLine($"Invoke HandlerAsyncNonBlock");
                        }
                        if (nodeResult.HandlerAsyncWithBlock is not null)
                        {
                            int status = await nodeResult.HandlerAsyncWithBlock.Invoke(result);
                            Console.WriteLine($"Invoke HandlerAsyncWithBlock");
                        }
                        tableOfResults.AddResult(nodeResult);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Result. length:[{nodeResult.Length}] for: {nodeResult.Target.path}\r\n" + nodeResult.ResultAsString());
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    {
                        throw new System.NotImplementedException("Task.Delay() or Error");
                    }
                }
                else if (node is NodeRepeat nodeRepeat)
                {

                }
                Current = node.Tail;
            }
        }

        public void OnResult(ReportFunc func)
        {
            this.resultHandler = func;
        }
    }

    internal class ReadResults
    {
        public readonly Dictionary<string, ResultItem> values = new Dictionary<string, ResultItem>();

        internal void AddSlot(Node value)
        {
            ResultItem resultItem = new ResultItem();
            resultItem.node = value;
            resultItem.idName = value.path;
        }
    }

    internal class ResultItem
    {
        public string idName = string.Empty;
        public ReadOnlyMemory<byte> data;
        public Node node;
    }

    /// <summary>
    /// serilized data of <see cref="Scheme"/>
    /// </summary>
    [Serializable]
    public class SchemeBinary
    {
        public SchemeBinary(Scheme scheme)
        {
            throw new System.NotImplementedException();
        }
    }

    public class FrameMapped
    {
        /// <summary>
        /// Определяет в одном ли буфере находятся все данные логической структуры фрейма. Если фрейм уместился в 1 неразделенный бафер (например весь фрейм уместился полностью в массив byte[]) то значение <see langword="true"/>.<br/>
        /// аналогично <see cref="System.Buffers.ReadOnlySequence{T}.IsSingleSegment"/> только наоборот
        /// </summary>
        public bool isFragmented;
    }

    public class FrameScheme
    {
        /// <summary>
        /// Последовательно добавляет разметку указанной длины
        /// </summary>
        /// <param name="length"></param>
        /// <param name="id"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal int AddMapLengthSource(int length, string id, EMapType type)
        {
            throw new NotImplementedException();
        }

        internal void AddSelector(int key0, Selector selector)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Selector
    {

    }

    public class ByteSelector : Selector
    {
        public ByteSelector(params byte[] bytes) : base()
        {
            throw new System.NotImplementedException();
        }
        internal int Case(byte val, Action value)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// After full read and validation this object is returned by <see cref="Reader"/>. 
    /// It provides <see cref="FrameMap"/> for stripping range from source or 
    /// <see cref="FrameValue"/> to get copy of payload
    /// </summary>
    public class FrameReadResult
    {
        public FrameReadResultNavigation Navigation => throw new System.NotImplementedException();
        public FrameMap this[int key] => throw new System.NotImplementedException();
    }

    public readonly ref struct FrameValue
    {

    }

    public readonly ref struct FrameMap
    {
        public readonly int offset;
        public readonly int length;
    }
    /// <summary>
    /// Объект для доступа к значениям на нижних уровнях. Например Header=>Value; или Frame=>Typeof(T1)=>
    /// </summary>
    public class FrameReadResultNavigation
    {
        /*
        Типы навигации:
        -Прямой
        -Разветвленный. На случай если например заголовк определяет вид послежующего содержимого. Таким образом можно контролировать правильность навигации на случай если у указанного типа внутри точно нет поля или точно должно быть
        */
    }

    public abstract class Separator
    {

    }

    public class CharSequenceSeparator : Separator
    {
        private ReadOnlyMemory<char> readOnlySpan;
        public CharSequenceSeparator(in ReadOnlyMemory<char> mem)
        {
            this.readOnlySpan = mem;
        }
    }

    internal readonly ref struct ReadProgress
    {
        /// <summary>
        /// What is read by <see cref="Reader"/>
        /// </summary>
        public readonly int id;
        /// <summary>
        /// Start position
        /// </summary>
        public readonly int offset;
        /// <summary>
        /// 
        /// </summary>
        public readonly int length;
    }
}