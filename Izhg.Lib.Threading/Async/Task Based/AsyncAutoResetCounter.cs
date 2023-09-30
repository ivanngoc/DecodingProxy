using System;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Async.Contracts;

namespace IziHardGames.Libs.Async
{
    /// <summary>
    /// Задача - Дождаться значения в счетчике. Когда значение достигает необходимого, то await завершается
    /// </summary>
    public class AsyncAutoResetCounter : IDisposable, ITaskBased, IAwaitComntrol
    {
        public int counter;
        private int value;
        private TaskCompletionSource? tcs;

        public AsyncAutoResetCounter()
        {
            InitReset();
        }

        public void InitReset()
        {
            tcs = new TaskCompletionSource();
            counter = 0;
            value = -1;
        }
        public void Increment()
        {
            Interlocked.Increment(ref counter);
            if (counter == value)
            {
                tcs.SetResult();
                InitReset();
            }
        }
        public void Decrement()
        {
            Interlocked.Decrement(ref counter);
            if (counter == value)
            {
                tcs.SetResult();
                InitReset();
            }
        }
        public Task Await(int val, CancellationToken token = default)
        {
            if (token != default)
                token.Register(() =>
                {
                    tcs.SetCanceled(token);
                });

            if (counter == val) return Task.CompletedTask;
            this.value = val;
            return tcs.Task;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
