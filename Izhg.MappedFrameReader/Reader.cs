using System.Buffers;
using Func = System.Func<System.ReadOnlyMemory<byte>, int>;

namespace IziHardGames.MappedFrameReader
{
    // Совмещены идеи: JsonElement (Node-Reading); Mapping; 
    public class Reader
    {
        internal EReadType readType;
        internal ReadProgress Progress => throw new System.NotImplementedException();
        private int offset;
        private Node Curent { get; set; }

        private Adapter adapter;
        private ReadOperation? current;

        public Reader(Scheme scheme)
        {
            throw new System.NotImplementedException();
        }
        private bool MoveNext()
        {
            throw new System.NotImplementedException();
        }

        internal void RegistHandlers(string id, Func value)
        {
            throw new NotImplementedException();
        }

        internal async Task ReadAsync(byte[] testData)
        {
            while (current != null)
            {
                await current.ReadAsync();
                current = current.Next;
            }
        }
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

    internal readonly struct Node
    {

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