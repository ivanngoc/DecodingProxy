using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IziHardGames.Libs.NonEngine.Enumerators;

namespace IziHardGames.Libs.NonEngine.Collections
{
    /// <summary>
    /// Очередь с фиксированным размером. Когда очередь заполнена, то каждый добавленный новый элемент будет заменять самый старый элемент.
    /// Принцип змеи поедающей свой хвост. Круговая запись
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class QueueCircled<T>
    {
        protected T[] items;
        protected int count;
        /// <summary>
        /// Capacity
        /// </summary>
        protected int capacity;
        /// <summary>
        /// which index of <see cref="items"/> is next to enqueue. if tail=head than count=size. Every next enqueu will override item at head
        /// </summary>
        protected int tail;
        /// <summary>
        /// which index is begining. item at that index will be returned in <see cref="Dequeue"/>
        /// </summary>
        protected int head;
        /// <summary>
        /// Index Of last item
        /// </summary>
        protected int last;

        public int Count => count;
        public int Head => head;
        public int Last => last;

        public T this[int index]
        {
            get => GetByIndex(index);
            set => SetByIndex(value, index);
        }

        public QueueCircled(int capacity)
        {
            items = new T[capacity];
            this.capacity = capacity;
        }

        /// <summary>
        /// Fill whole queue with specific item.
        /// </summary>
        /// <param name="item"></param>
        public void Fill(T item)
        {
            count = capacity;
            head = default;
            tail = default;
            last = capacity - 1;

            for (int i = 0; i < capacity; i++)
            {
                items[i] = item;
            }
        }

        /// <summary>
        /// Unsafe deque
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            T result = items[head];
#if DEBUG
            items[head] = default;
#endif
            head++;
            head = head % capacity;
            count--;
#if DEBUG

            if (count < 0) throw new ArgumentOutOfRangeException("Keep calling dequeue when no items left");
#endif
            return result;
        }
        /// <summary>
        /// Enqueue with overriding on overflow
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            items[tail] = item;

            count++;

            if (count > capacity)
            {
                last = head;
                head++;
                head = head % capacity;
                tail = head;
                count = capacity;
            }
            else
            {
                last = tail;
                tail++;
                tail = tail % capacity;
            }
        }

        public T Peak()
        {
            return items[head];
        }
        public T PeakFromEnd()
        {
            return items[last];
        }
        public (T, T) PeakDouble()
        {
#if DEBUG
            if (count < 2) throw new ArgumentOutOfRangeException($"For peak double at least 2 items must be in queue");
#endif
            return (items[head], items[(head + 1) % capacity]);
        }
        /// <summary>
        /// Посмотреть предпоследний и последний элемент
        /// </summary>
        /// <returns></returns>
        public (T, T) PeakDoubleFromEnd()
        {
#if DEBUG
            if (count < 2) throw new ArgumentOutOfRangeException($"For peak double at least 2 items must be in queue");
#endif
            int indexOfPenultimate = last - 1;
            indexOfPenultimate = indexOfPenultimate < 0 ? capacity + indexOfPenultimate : indexOfPenultimate;
            return (items[indexOfPenultimate], items[last]);
        }

        public T GetByIndex(int index)
        {
            return items[SequenceIndexToActualIndex(index)];
        }
        public void SetByIndex(T item, int index)
        {
            items[SequenceIndexToActualIndex(index)] = item;
        }

        public virtual void SetByActualIndex(T item, int index)
        {
            items[index] = item;
        }

        public int SequenceIndexToActualIndex(int sequenceIndex)
        {
            if (sequenceIndex < capacity)
            {
                int index = head + sequenceIndex;

                if (index < capacity)
                {
                    return index;
                }
                return index % capacity;
            }
            throw new ArgumentOutOfRangeException($"Given index {sequenceIndex}");
        }

        /// <summary>
        /// Сдвиг через параллельный перенос в сторону увеличения индекса.
        /// shift both head and tail like add same amount to hours and minutes arrows on a clock.
        /// </summary>
        /// <param name="countToShift"></param>
        public void ShiftForward(int countToShift)
        {
            this.head = (head + countToShift) % capacity;
            this.tail = (tail + countToShift) % capacity;
            this.last = (last + countToShift) % capacity;
        }
        /// <summary>
        /// Reverse to <see cref="ShiftForward(int)"/>
        /// </summary>
        /// <param name="countToShift"></param>
        public void ShiftBackward(int countToShift)
        {
            int shift = countToShift % capacity;
            this.head = head - shift;
            if (head < 0)
            {
                head = capacity + head;
            }
            this.tail = tail - shift;
            if (tail < 0)
            {
                tail = capacity + tail;
            }
            this.last = last - shift;
            if (last < 0)
            {
                last = capacity + shift;
            }
        }
        /// <summary>
        /// Increment taken area after last element (expansion from left to right)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ref T ExtendAfterLast()
        {
            if (count < capacity)
            {
                last = tail;
                tail = (tail + 1) % capacity;
                count++;
                return ref items[last];
            }
            throw new OverflowException($"Full capacity={capacity}");
        }
        /// <summary>
        /// Increment taken area before head (expansion from right to left)
        /// </summary>
        /// <returns></returns>
        public ref T ExtendBeforeHead()
        {
            if (count < capacity)
            {
                head = head - 1;
                if (head < 0)
                {
                    head = capacity + head;
                }
                count++;
                return ref items[head];
            }
            throw new ArgumentOutOfRangeException($"Full capacity");
        }

        public ref T ShrinkLast()
        {
            if (count > 0)
            {
                tail = last;
                last--;
                if (last < 0)
                {
                    last += capacity;
                }
                count--;
                return ref items[tail];
            }
            throw new ArgumentOutOfRangeException("Collection is Empty");
        }
        public ref T ShrinkHead()
        {
            if (count > 0)
            {
                int prevHead = head;
                head = (head + 1) % capacity;
                count--;
                return ref items[prevHead];
            }
            throw new ArgumentOutOfRangeException("Collection is empty");
        }
        public int IndexOf(T item)
        {
            for (int i = 0; i < count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(this[i], item)) return i;
            }
            return -1;
        }
        public int IndexActual(T item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(items[i], item)) return i;
            }
            return -1;
        }
        public int IndexOf(T item, Func<T, T, bool> funcEqual)
        {
            for (int i = 0; i < count; i++)
            {
                if (funcEqual(this[i], item)) return i;
            }
            return -1;
        }
        /// <summary>
        /// By Default removed element will be copied to tail.
        /// Items after "Hole" will be shifted to left to fill the gap.
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveAt(int index)
        {
            int actualIndex = SequenceIndexToActualIndex(index);
            T preserve = items[actualIndex];

            if (tail < head)
            {
                if (tail <= actualIndex && actualIndex < head)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (actualIndex < head)
                {
                    goto SIMPLE_SLIDING_COPY;
                }
                else
                {
                    // right part
                    int indexToCopyAtrightPart = actualIndex + 1;
                    int lastElement = capacity - 1;
                    if (indexToCopyAtrightPart < capacity)
                    {
                        Array.Copy(items, indexToCopyAtrightPart, items, actualIndex, lastElement - actualIndex);
                    }
                    items[lastElement] = items[0];
                    // left part
                    if (1 < last)
                    {
                        Array.Copy(items, 1, items, 0, tail - 1);
                    }
                    items[last] = preserve;
                    tail = last;
                    last--;
                    if (last < 0)
                    {
                        last += capacity;
                    }
                    count--;
                }
            }
            else
            {
                if (head < tail)
                {
                    if (actualIndex < head || tail <= actualIndex)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
                goto SIMPLE_SLIDING_COPY;
            }
            return;

            SIMPLE_SLIDING_COPY:
            {
                int indexToStartCopy = actualIndex + 1;
                if (indexToStartCopy < last)
                {
                    Array.Copy(items, indexToStartCopy, items, actualIndex, last - actualIndex);
                }
                items[last] = preserve;
                tail = last;
                last = last - 1;
                if (last < 0)
                {
                    last += capacity;
                }
                count--;
                return;
            }
        }
        public EnumeratorCircled<T> GetEnumerator()
        {
            return new EnumeratorCircled<T>(items, head, tail);
        }
        public void Reset()
        {
            head = default;
            tail = default;
            last = default;
            count = default;
        }
        public void Clear()
        {
            Reset();
            items?.Clear();
        }

        public string ItemsToStringInActualOrder(string separator = "\t")
        {
            string s = default;

            for (int i = 0; i < items.Length; i++)
            {
                if (head == i)
                {
                    s += "<" + items[i].ToString() + separator;
                    continue;
                }
                if (last == i)
                {
                    s += items[i].ToString() + ">" + separator;
                    continue;
                }
                s += items[i].ToString() + separator;
            }
            return s;
        }
        public string ItemsToStringInSequenceOrder(string separator = "\t")
        {
            string s = default;

            for (int i = 0; i < count; i++)
            {
                s += this[i].ToString() + separator;
            }
            return s;
        }


#if DEBUG
        public static void TestRemoveAt()
        {
            QueueCircled<int> fixedQueue = new QueueCircled<int>(10);
            for (int i = 0; i < 10; i++)
            {
                fixedQueue.Enqueue(i + 100);
                Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y));
            }
            Console.WriteLine(fixedQueue.ItemsToStringInActualOrder());
            Console.WriteLine(fixedQueue.ItemsToStringInSequenceOrder());
            fixedQueue.RemoveAt(2);
            Console.WriteLine("After RemoveAt Case1");
            Console.WriteLine(fixedQueue.ItemsToStringInActualOrder());
            Console.WriteLine(fixedQueue.ItemsToStringInSequenceOrder());
            fixedQueue.ExtendAfterLast();
            fixedQueue.RemoveAt(9);
            Console.WriteLine("After RemoveAt Case2");
            Console.WriteLine(fixedQueue.ItemsToStringInActualOrder());
            Console.WriteLine(fixedQueue.ItemsToStringInSequenceOrder());
            fixedQueue.ExtendAfterLast();
            fixedQueue.RemoveAt(0);
            Console.WriteLine("After RemoveAt Case3");
            Console.WriteLine(fixedQueue.ItemsToStringInActualOrder());
            Console.WriteLine(fixedQueue.ItemsToStringInSequenceOrder());

            fixedQueue.ShrinkHead();
            fixedQueue.ShrinkHead();
            fixedQueue.ShrinkHead();

            Console.WriteLine("After Shrink head");
            Console.WriteLine(fixedQueue.ItemsToStringInActualOrder());
            Console.WriteLine(fixedQueue.ItemsToStringInSequenceOrder());

            fixedQueue.ShrinkLast();
            fixedQueue.ShrinkLast();

            Console.WriteLine("After Shrink last");
            Console.WriteLine(fixedQueue.ItemsToStringInActualOrder());
            Console.WriteLine(fixedQueue.ItemsToStringInSequenceOrder());

            fixedQueue.Enqueue(559);
            fixedQueue.Enqueue(560);
            fixedQueue.Enqueue(561);

            Console.WriteLine("After enqueue");
            Console.WriteLine(fixedQueue.ItemsToStringInActualOrder());
            Console.WriteLine(fixedQueue.ItemsToStringInSequenceOrder());
        }
        public static void Test()
        {
            QueueCircled<int> fixedQueue = new QueueCircled<int>(5);

            for (int i = 0; i < 10; i++)
            {
                fixedQueue.Enqueue(i + 100);
                Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y));
            }

            var d0 = fixedQueue.Dequeue();
            Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y) + $"|	{d0}");
            var d1 = fixedQueue.Dequeue();
            Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y) + $"|	{d1}");
            var d2 = fixedQueue.Dequeue();
            Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y) + $"|	{d2}");
            fixedQueue.Enqueue(999);
            Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y));
            var d3 = fixedQueue.Dequeue();
            Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y) + $"|	{d3}");
            var d4 = fixedQueue.Dequeue();
            Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y) + $"|	{d4}");
            fixedQueue.Enqueue(777);
            Console.WriteLine(fixedQueue.items.Select(x => x.ToString()).Aggregate((x, y) => x + "; " + y));
        }

#endif
    }
}