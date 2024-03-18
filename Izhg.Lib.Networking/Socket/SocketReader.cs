using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public abstract class SocketReader : SocketProcessor
    {
        protected SocketReader? source;
        protected SocketReader? destination;
        protected List<SocketReaderInterceptor> interceptorsIn = new List<SocketReaderInterceptor>(8);
        protected List<SocketReaderInterceptor> interceptorsOut = new List<SocketReaderInterceptor>(8);
        protected readonly List<ISocketReaderBind> binds = new List<ISocketReaderBind>(4);

        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            if (this is SockerReaderRawStackable) return;

            source = wrap.Reader;

            if (source.destination != null)
            {
                this.destination = source.destination;
                source.destination.source = this;
            }
            else
            {
                wrap.SetReader(this);
            }
            source.destination = this;
        }

        public override void Dispose()
        {
            foreach (var item in binds)
            {
                item.SetReader(source!);
            }
            binds.Clear();

            if (destination != null)
            {
                destination.source = source;
                source!.destination = this.destination;
            }
            else
            {
                wrap.SetReader(source!);
                source!.destination = default;
            }
            base.Dispose();
            this.destination = default;
            source = default;
        }
        /// <summary>
        /// Разработан для конвеерной передачи от одного ридера к другому
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public abstract ValueTask<int> TransferToAsync(Memory<byte> mem, CancellationToken ct = default);
        public abstract int TransferTo(byte[] array, int offset, int length);

        /// <summary>
        /// Обаботчик данных до обработки данных ридером
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddInterceptorIn<T>() where T : SocketReaderInterceptor, new()
        {
            var pool = PoolObjectsConcurent<T>.Shared;
            T item = pool.Rent();
            interceptorsIn.Add(item);
            if (item is IPoolBind<T> bindable) bindable.BindToPool(pool);
            item.Initilize(this);
            return item;
        }
        /// <summary>
        /// Обаботчик данных после обработки данных ридером
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddInterceptorOut<T>() where T : SocketReaderInterceptor, new()
        {
            var pool = PoolObjectsConcurent<T>.Shared;
            T item = pool.Rent();
            interceptorsOut.Add(item);
            if (item is IPoolBind<T> bindable) bindable.BindToPool(pool);
            item.Initilize(this);
            return item;
        }

        public void Bind(ISocketReaderBind bind)
        {
            binds.Add(bind);
            bind.SetReader(this);
        }
    }
}