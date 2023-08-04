// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace IziHardGames.Libs.Collections.Adaptive
{
    public class MorphArray<T> : IDisposable
    {
        private MorphArray morphArray;
        private Memory<T> memory;
     
        public T this[int index]
        {
            get => memory.Span[index];
            set => memory.Span[index] = value;
        }

        public void Asign(MorphArray morphArray)
        {
            throw new System.NotImplementedException();
        }
        public void Dispose()
        {
            morphArray.Dispose();
        }
    }

    /// <summary>
    /// string builder + Morph Array
    /// </summary>
    public class MorphArray : IDisposable
    {
        protected byte[] buffer;
        protected int offset;
        protected int length;
        protected int position;

        // Morphed
        private int count;
        private int unitSize;
        private Type type;


        public void Init(int bytesCapacity)
        {
            buffer = ArrayPool<byte>.Shared.Rent(bytesCapacity);
            offset = 0;
            length = 0;
            position = 0;
        }

        public void MorphTo<T>() where T : unmanaged
        {
            this.type = typeof(T);
            unitSize = Marshal.SizeOf<T>();
        }

        public void Append<T>(Span<T> value) where T : unmanaged
        {
            EnsureAdditive<T>(value.Length);
            throw new System.NotImplementedException();
        }

        public void AppendUnsafe<T>(Span<T> value) where T : unmanaged
        {
            throw new System.NotImplementedException();
        }

        public void AppendUnsafe<T>(T c) where T : unmanaged
        {
            As<T>()[position] = c;
            length = position;
            position += Marshal.SizeOf<T>();
        }
        public void Append<T>(T c) where T : unmanaged
        {
            EnsureAdditive<T>(1);
            throw new System.NotImplementedException();
        }
        public void Append(char c)
        {
            EnsureAdditive<char>(1);
            AppendUnsafe(c);
            throw new System.NotImplementedException();
        }
        public void AppendUnsafe(char c)
        {
            throw new System.NotImplementedException();
        }

        public void Append(int value)
        {
            EnsureAdditive<int>(1);
            AppendUnsafe(value);
        }
        public void AppendUnsafe(int value)
        {
            throw new System.NotImplementedException();
        }

        private bool CheckCapacity<T>(int count, out int requiredBytes) where T : unmanaged
        {
            int needed = Marshal.SizeOf<T>() * count;
            int left = buffer.Length - (offset + length);

            if (needed > left)
            {
                requiredBytes = needed - left;
                return false;
            }
            else
            {
                requiredBytes = 0;
                return true;
            }
        }
        /// <summary>
        /// Дать гарантии что <see cref="buffer"/> будет как минимум заданного размера
        /// </summary>
        /// <param name="bytes"></param>
        private void EnsureMaxCapacity(int bytes)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Дать гарантии что возможно добавить байтов
        /// </summary>
        /// <param name="bytes"></param>
        private void EnsureAdditive(int bytes)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Дать гарантии что возможно добавить количество объектов
        /// </summary>
        /// <param name="bytes"></param>
        private void EnsureAdditive<T>(int count) where T : unmanaged
        {
            throw new System.NotImplementedException();
        }

        public int IndexOf<T>(ReadOnlySpan<T> subsequence) where T : unmanaged
        {
            throw new System.NotImplementedException();
        }

        public Span<char> AsChars() => MemoryMarshal.Cast<byte, char>(buffer);
        public Span<int> AsInts() => MemoryMarshal.Cast<byte, int>(buffer);
        public Span<T> As<T>() where T : unmanaged => MemoryMarshal.Cast<byte, T>(buffer);

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = null;
        }
    }
}