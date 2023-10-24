using System.Collections.Generic;
using IziHardGames.Lib.Collections.Contracts;

namespace IziHardGames.MappedFrameReader
{

    public abstract class Node : INodeBidirectional<Node>, IDisposable
    {
        internal ENodeType typeNode;

        /// <summary>
        /// Previous/Before
        /// </summary>
        public Node? Head { get; set; }

        /// <summary>
        /// Next/After
        /// </summary>
        public Node? Tail { get; set; }

        protected Action? actionExecute;
        protected Func<Task>? actionExecuteAsync;
        protected int Readed { get; set; }

        /// <summary>
        /// Uniq name for navigation. Doesn't contain '$'
        /// </summary>
        public string path;
        /// <summary>
        /// Нужно ли после считывания записывать результат в <see cref="TableOfResults"/>
        /// </summary>
        public bool isWriteToTable;
        /// <summary>
        /// эта <see cref="Node"/> производит чтение
        /// </summary>
        public bool isReadDefined;

        protected DefinedType definedType;
        /// <summary>
        /// Фиксированая длина которую <see cref="Node"/> должен прочитать если длина определена явно а не поиском со сравнением
        /// </summary>
        protected int lengthToRead;
        /// <summary>
        /// Длина считывания начиня с <see cref="offset"/>. Если это значение отлично от 0, то эта <see cref="Node"/> выполнила продвижение при считывании
        /// </summary>
        public int lengthConsumed;
        /// <summary>
        /// длина значения. Если по условию например не нужно в результат брать \r\n то значение будет меньше <see cref="lengthConsumed"/>
        /// </summary>
        public int lengthValue;
        /// <summary>
        /// фактическая длина <see cref="Node"/> между чтениями. Если нода не считывала и не является <see cref="IAdvancingNode"/> то значение будет равно 0
        /// </summary>
        public int lengthDiff;
        /// <summary>
        /// index c которого начинается полезная нагрузка. Если например берется значение без whitespace в начале то значение будет больше чем <see cref="offset"/>
        /// </summary>
        public int offsetValue;
        /// <summary>
        /// index с которого текущая <see cref="Node"/> производит чтение
        /// </summary>
        public int offset;
        protected bool isCasting;

        public readonly static TaskCanceledException exc = new TaskCanceledException();
        public readonly static TaskCanceledException excNotEnoguhLength = new TaskCanceledException();
        public readonly static TaskCanceledException excNotFounded = new TaskCanceledException();
        public readonly static Task taskCanceled;
        public readonly static Task taskSucceed;
        /// <summary>
        /// length was specified. But input data was less than expected
        /// </summary>
        public readonly static Task taskNotEnough;
        /// <summary>
        /// With current data can't find end. Need More Data
        /// </summary>
        public readonly static Task taskNotFounded;
        protected ReadOnlyMemory<byte> valueStripped;
        protected ReadOnlyMemory<byte> value;

        static Node()
        {
            taskCanceled = Task.FromException(exc);
            taskNotEnough = Task.FromException(excNotEnoguhLength);
            taskNotFounded = Task.FromException(excNotFounded);
            taskSucceed = Task.CompletedTask;
        }
        public Node()
        {

        }

        internal EnumeratorNodes<Node> GetEnumerator()
        {
            return new EnumeratorNodes<Node>(this);
        }

        internal virtual void SetThisAfter(ref Node? after)
        {
            this.Head = after;
            after.SetNext(this);
            after = this;
        }
        internal void SetNext(Node next)
        {
            this.Tail = next;
        }

        internal abstract void Execute(ReaderContext readerContext);
        internal virtual Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            if (this is IAdvancingNode advNode)
            {
                readerContext.Advance(lengthConsumed);
                return Task.CompletedTask;
            }
            else
            {
                throw new InvalidOperationException("Only advancing nodes can call this");
            }
        }

        internal SchemeGraph ToGraph()
        {
            SchemeGraph schemeGraph = new SchemeGraph(this as NodeBegin ?? throw new NullReferenceException());

            return schemeGraph;
        }

        /// <summary>
        /// Store result in <see cref="ReadResults"/>
        /// </summary>
        /// <param name="idName"></param>
        internal void WriteToResults(string idName)
        {
            isWriteToTable = true;
            SetPath(idName);
        }
        internal void SetPath(string idName)
        {
            this.path = idName;
        }
        internal void SetReadLength(string idName, DefinedType definedType)
        {
            this.definedType = definedType;
            if (definedType.sizeType == Scheme.ESizeType.Defined)
            {
                this.lengthToRead = definedType.size;
                this.isReadDefined = true;
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
        public void SetReadLength(int length, string idName, DefinedType type)
        {
            this.definedType = type;
            this.path = idName;
            this.lengthToRead = length;
            this.isReadDefined = true;
        }


        /// <summary>
        /// <see cref="DefinedType.size"/> is not equal to <see cref="Node.lengthToRead"/>
        /// </summary>
        internal void SetCast()
        {
            isCasting = true;
        }

        public virtual void Dispose()
        {

        }
        internal virtual ReadOnlyMemory<byte> GetResultStripped() => valueStripped;
        internal virtual ReadOnlyMemory<byte> GetResult() => value;

        internal virtual void SetResult(ReadOnlyMemory<byte> res)
        {
            this.value = res;
        }
        internal virtual void SetResultStripped(ReadOnlyMemory<byte> res)
        {
            valueStripped = res;
            lengthValue = res.Length;
        }
    }
}