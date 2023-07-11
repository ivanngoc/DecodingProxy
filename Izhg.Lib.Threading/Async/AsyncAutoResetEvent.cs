using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Async
{
    public class AsyncAutoResetCounter
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
    }

    public class AsyncAutoResetEvent<T>
    {
        private readonly Queue<TaskCompletionSource<T>> waits = new Queue<TaskCompletionSource<T>>();
        private bool isCanceled;
        private CancellationToken? cancellationToken;

        public void Reset()
        {
            cancellationToken = default;

            isCanceled = false;

            Exception exception = null;
            lock (waits)
            {
                if (waits.Count > 0) exception = new InvalidOperationException($"There is still waits in queue");
            }
            if (exception != null) throw exception;
        }

        public Task<T> WaitAsync(CancellationToken token = default)
        {
            if (token != default)
            {
                if (cancellationToken == null)
                {
                    this.cancellationToken = token;
                    token.Register(Cancel);
                }
                if (this.cancellationToken != token) throw new NotSupportedException($"Tokens must be from one source");
            }

            lock (waits)
            {
                //Console.WriteLine($"ARE. wait. waits:{waits.Count}");
                if (waits.Count > 0)
                {
                    return waits.Dequeue().Task;
                }
                else
                {
                    var tcs = new TaskCompletionSource<T>();
                    if (isCanceled) { tcs.SetCanceled(); return tcs.Task; }
                    waits.Enqueue(tcs);
                    return tcs.Task;
                }
            }

        }

        public void Cancel()
        {
            lock (waits)
            {   // do not dequeue for consumers to consume awaits
                isCanceled = true;
                foreach (var item in waits)
                {
                    item.SetCanceled();
                }
            }
        }

        public void Set(T value)
        {
            lock (waits)
            {
                //Console.WriteLine($"ARE. Set. waits:{waits.Count}");
                if (waits.Count > 0)
                {
                    TaskCompletionSource<T> toRelease = waits.Dequeue();
                    if (isCanceled)
                    {
                        return;
                    }
                    toRelease.SetResult(value);
                }
                else
                {
                    var res = new TaskCompletionSource<T>();
                    if (isCanceled)
                    {
                        res.SetCanceled(cancellationToken.GetValueOrDefault());
                    }
                    else
                    {
                        res.SetResult(value);
                    }
                    waits.Enqueue(res);
                }
            }
        }

    }


    /// <summary>
    /// Поддерживает множественный вызов ожиданий подряд: например было вызвано первое ожидание и не дождавшись его заверешния вызывается следом второе ожидание
    /// </summary>
    public class AsyncAutoResetEvent
    {
        private readonly Queue<TaskCompletionSource> waits = new Queue<TaskCompletionSource>();
        private int signals;
        private bool isCanceled;
        private CancellationToken? cancellationToken;
        private Task cachedCanceled;

        private Exception exception;
        private Exception exception2;

        public void Reset()
        {
            cachedCanceled = default;
            cancellationToken = default;

            isCanceled = false;

            Exception exception = null;
            lock (waits)
            {
                if (waits.Count > 0 || signals > 0) exception = new InvalidOperationException($"There is still waits in queue");
            }
            signals = 0;
            if (exception != null) throw exception;
        }

        public Task WaitAsync(CancellationToken token = default)
        {
            try
            {
                if (token != default)
                {
                    if (cancellationToken == null)
                    {
                        this.cancellationToken = token;

                        token.Register(() =>
                        {
                            lock (waits)
                            {   // do not dequeue for consumers to consume awaits
                                isCanceled = true;
                                this.cachedCanceled = Task.FromCanceled(token);
                                foreach (var item in waits)
                                {
                                    item.SetCanceled();
                                }
                            }
                        });
                    }
                    if (this.cancellationToken != token) throw new NotSupportedException($"Tokens must be from one source");
                }

                lock (waits)
                {
                    Console.WriteLine($"ARE. wait. signals:{signals} waits:{waits.Count}");

                    if (signals > 0)
                    {
                        signals--;
                        return isCanceled ? cachedCanceled : Task.CompletedTask;
                    }
                    else
                    {
                        var tcs = new TaskCompletionSource();
                        waits.Enqueue(tcs);
                        return tcs.Task;
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
        }
        public void Set()
        {
            lock (waits)
            {
                try
                {
                    Console.WriteLine($"ARE. Set. signals:{signals} waits:{waits.Count}");
                    if (isCanceled)
                    {
                        signals++; return;
                    }
                    if (waits.Count > 0)
                    {
                        TaskCompletionSource toRelease = waits.Dequeue();
                        if (isCanceled)
                        {
                            return;
                        }
                        toRelease.SetResult();
                    }
                    else
                    {
                        signals++;
                    }
                }
                catch (Exception ex)
                {
                    exception2 = ex;
                }
            }
        }
    }
}
