using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using IziHardGames.Libs.Async.Contracts;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Async
{
    /// <summary>
    /// same as <see cref="AsyncSignaler"/> but struct
    /// </summary>
    [Obsolete("Not Imlemented")]
    public struct ValueTaskSource : IValueTaskSource, IValueTaskBased, IAwaitComntrol
    {
        private ManualResetValueTaskSourceCore<bool> cts;

        public void GetResult(short token)
        {
            cts.GetResult(token);
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return cts.GetStatus(token);
        }

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            cts.OnCompleted(continuation, state, token, flags);
        }
        public ValueTask Await(CancellationToken token = default)
        {
            if (token != default) token.Register(() => throw new System.NotImplementedException());
            return new ValueTask(this, cts.Version);
        }

        internal void Set()
        {
            cts.SetResult(true);
        }

        public static void Test()
        {
            ThreadPool.QueueUserWorkItem((x) => { });
        }
    }

    /// <summary>
    /// Семафор для паттерн производитель-потребитель. 1 производитель вызывает метод Set(), после чего потребитель завершает await и потребляет. 1к1. одновременно может быть только 1 await.    /// 
    /// Аналог <see cref=""/> c базой на <see cref="ValueTask{TResult}"/>
    /// </summary>
    public sealed class AsyncSignaler : IValueTaskSource<bool>, IDisposable, IPoolBind<AsyncSignaler>, IValueTaskBased, IAwaitComntrol
    {
        public static readonly OperationCanceledException exception = new OperationCanceledException();

        private readonly Action actionSetException;

        private ManualResetValueTaskSourceCore<bool> manualReset;
        private IPoolReturn<AsyncSignaler>? pool;
        private int requests;
        private int responses;
        private CancellationToken cancellationToken;
        private static readonly object lockShared = new object();

        public AsyncSignaler()
        {
            actionSetException = SetException;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// <see langword="true"/> - succesfully awaited<br/>
        /// <see langword="false"/> - canceled
        /// </returns>
        public ValueTask<bool> Await(CancellationToken ct = default)
        {
            lock (lockShared)
            {
                if (responses > 0)
                {
                    if (ct.IsCancellationRequested) throw exception;
                    responses--;
                    return ValueTask.FromResult(true);
                }
                else
                {
                    if (ct != default && ct != cancellationToken)
                    {
                        this.cancellationToken = ct;
                        ct.Register(actionSetException);
                    }
                    requests++;
                    var task = new ValueTask<bool>(this, manualReset.Version);
                    return task;
                }
            }
        }

        public bool GetResult(short token)
        {
            var result = manualReset.GetResult(token);
            manualReset.Reset();
            return result;
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return manualReset.GetStatus(token);
        }

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            manualReset.OnCompleted(continuation, state, token, flags);
        }

        public void Set()
        {
            lock (lockShared)
            {
                if (requests > 0)
                {
                    requests--;
                    manualReset.SetResult(true);
                }
                else
                {
                    responses++;
                }
            }
        }

        private void SetException()
        {
            manualReset.SetException(exception);
            manualReset.Reset();
        }
        public void Dispose()
        {
            pool.Return(this);
            if (responses > 0 || requests > 0) throw new InvalidOperationException("All tasks must be completed");
            pool = default;

        }

        public static AsyncSignaler Rent()
        {
            var pool = PoolObjectsConcurent<AsyncSignaler>.Shared;
            var rent = pool.Rent();
            rent.BindToPool(pool);
            return rent;
        }

        public void BindToPool(IPoolReturn<AsyncSignaler> pool)
        {
            this.pool = pool;
        }
#if DEBUG
        public static async Task Test1()
        {
            ValueTaskSource valueTaskSource = new ValueTaskSource();

            var t1 = Task.Run(async () =>
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(3000);
                    Console.WriteLine("Set");
                    valueTaskSource.Set();
                }
            });

            while (true)
            {
                Console.WriteLine("Begin Await");
                await valueTaskSource.Await(default);
                Console.WriteLine($"Awaited {DateTime.Now}");
                await Task.Delay(1000);
            }
        }
        public static async Task Test()
        {
            AsyncSignaler control = new AsyncSignaler();

            var t1 = Task.Run(async () =>
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(3000);
                    Console.WriteLine("Set");
                    control.Set();
                }
            });

            while (true)
            {
                Console.WriteLine("Begin Await");
                await control.Await(default);
                Console.WriteLine($"Awaited {DateTime.Now}");
                await Task.Delay(1000);
            }
        }

#endif       
    }
}
