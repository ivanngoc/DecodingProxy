﻿using System;
using System.Buffers;
using Izhg.Lib.Collections.Contracts;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.ForHttp
{
    public class HttpBinary : ISingleEdgeNode<HttpBinary>, IDisposable, IPoolBind<HttpBinary>
    {
        protected byte[] datas = Array.Empty<byte>();
        protected int length;
        private IPoolReturn<HttpBinary>? pool;
        public HttpBinary? Next { get; set; }

        public void Initilize(byte[] buffer, int offset, int length)
        {
            this.length = length;
            if (datas.Length < length)
            {
                if (datas.Length > 0)
                {
                    ArrayPool<byte>.Shared.Return(datas);
                }
                datas = ArrayPool<byte>.Shared.Rent(length);
            }
            Array.Copy(buffer, offset, datas, 0, length);
        }
        public virtual void Dispose()
        {
            if (datas.Length > 0) ArrayPool<byte>.Shared.Return(datas);
            datas = Array.Empty<byte>();
            if (pool != null) pool.Return(this);
            pool = default;
            length = default;
        }

        public void BindToPool(IPoolReturn<HttpBinary> pool)
        {
            this.pool = pool!;
        }

        public void After(HttpBinary data)
        {
            data.Next = this;
        }
        public void Before(HttpBinary data)
        {
            this.Next = data;
        }
    }
}