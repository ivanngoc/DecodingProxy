using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using IziHardGames.Libs.Async.Contracts;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Async
{
    /// <summary>
    /// Семафор для паттерн производитель-потребитель. 1 производитель вызывает метод Set(), после чего потребитель завершает await и потребляет. 1к1. одновременно может быть только 1 await.    /// 
    /// Аналог <see cref=""/> c базой на <see cref="ValueTask{TResult}"/>
    /// </summary>

    [Guid("5994bba1-cc3a-4b7f-a2c6-dcdbabefa86e")]
    public sealed class AsyncSignaler : IValueTaskSource<bool>, IDisposable, IPoolBind<AsyncSignaler>, IValueTaskBased, IAwaitComntrol
    {
        public static readonly OperationCanceledException exception = new OperationCanceledException();

        private readonly Action actionSetException;

        private ManualResetValueTaskSourceCore<bool> manualReset;
        private IPoolReturn<AsyncSignaler>? pool;
        private int requests;
        private int responses;
        private CancellationToken cancellationToken;

        public AsyncSignaler()
        {
            actionSetException = SetException;
        }
        /// <summary>
        /// Await until <see cref="Set"/> is Called. Stackable result. But only 1 consumer can await
        /// </summary>
        /// <returns>
        /// <see langword="true"/> - succesfully awaited<br/>
        /// <see langword="false"/> - canceled
        /// </returns>
        public ValueTask<bool> Await(CancellationToken ct = default)
        {
            lock (actionSetException)
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
            lock (actionSetException)
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
            pool!.Return(this);
            if (responses > 0 || requests > 0) throw new InvalidOperationException("All tasks must be completed");
            pool = default;
            cancellationToken = default;
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
