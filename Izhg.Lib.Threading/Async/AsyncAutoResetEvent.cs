using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Async
{
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
        private static readonly InvalidOperationException invalidOperationException = new InvalidOperationException($"There is still waits in queue");

        public void Reset()
        {
            cachedCanceled = default;
            cancellationToken = default;

            isCanceled = false;

            Exception exception = null;
            lock (waits)
            {
                if (waits.Count > 0 || signals > 0) exception = invalidOperationException;
            }
            signals = 0;
            if (exception != null) throw exception;
        }

        public Task WaitAsync(CancellationToken token = default)
        {
            try
            {
                if (token != default)   // regist uniq cancelation once
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
    public class AsyncAutoResetEvent<T>
    {
        private static readonly InvalidOperationException exIncompleted = new InvalidOperationException($"Event is not completed. You must call Complete() explicitly to notify about ending");
        private static readonly InvalidOperationException exCompleted = new InvalidOperationException($"Event is completed. Use Reset to be able await again");
        private static readonly InvalidOperationException exInvalidOperation = new InvalidOperationException($"There is still waits in queue");
        private static readonly NotSupportedException exNotSupported = new NotSupportedException($"Tokens must be from one source");

        private readonly Queue<TaskCompletionSource<T>> waits = new Queue<TaskCompletionSource<T>>();
        private bool isCanceled;
        /// <summary>
        /// <see langword="true"/> - there won't be additional waits. After last wait is consumed Reset() must be called
        /// </summary>
        private bool isCompleted;
        private CancellationToken? cancellationToken;
        public int Count => waits.Count;
        public bool IsComplete => isCompleted;
        public void Reset()
        {
            if (!isCompleted) throw exIncompleted;
            isCompleted = false;
            cancellationToken = default;
            isCanceled = false;
            isCompleted = false;
            Exception exception = null;
            lock (waits)
            {
                if (waits.Count > 0) exception = exInvalidOperation;
            }
            if (exception != null) throw exception;
        }

        public Task<T> WaitAsync(CancellationToken token = default)
        {
#if DEBUG
            //Console.WriteLine($"Wait");
#endif
            lock (waits)
            {
                if (token != default)
                {
                    if (cancellationToken == null)
                    {
                        this.cancellationToken = token;
                        token.Register(Cancel);
                    }
                    if (this.cancellationToken != token) throw exNotSupported;
                }

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

        /// <summary>
        /// Call Complete() and Set. In that order its guaranted that awaiter on set result would gain IsCompleted=true in check at any time.
        /// Возможна ситуация когда TaskCompletionSource.SetResult(result) вызовет выполнение awaiter'а который выполнится раньше чем завершится Set(value). 
        /// Поэтому сперва нужно выставить IsComplete=true а потом вызвать Set(value)
        /// </summary>
        /// <param name="value"></param>
        public void SetLast(T value)
        {
            Complete();
            SetUnsafe(value);
        }

        public void Set(T value, bool isComplete)
        {
            Set(value);
            if (isComplete) Complete();
        }
        private void SetUnsafe(T value)
        {
#if DEBUG
            //Console.WriteLine($"Set");
#endif
            lock (waits)
            {
                //Console.WriteLine($"ARE. Set. waits:{waits.Count}");
                if (waits.Count > 0)
                {
                    TaskCompletionSource<T> toRelease = waits.Dequeue();
                    if (isCanceled)
                    {
                        toRelease.SetCanceled(cancellationToken.GetValueOrDefault());
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
        public void Set(T value)
        {
            if (isCompleted) throw exCompleted;
            SetUnsafe(value);
        }

        public void Complete()
        {
            if (isCompleted) throw exCompleted;
            isCompleted = true;
        }
    }
}
