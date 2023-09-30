using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Async
{
    public static class Awaiting
    {
        public static async ValueTask<(T1, T2)> WhenAll<T1, T2>(ValueTask<T1> t1, ValueTask<T2> t2)
        {
            T1 r1 = await t1.ConfigureAwait(false);
            T2 r2 = await t2.ConfigureAwait(false);
            return (r1, r2);
        }
        public static async Task WhenAll(ValueTask t1, ValueTask t2)
        {
            throw new System.NotImplementedException();
        }
        public static async Task WhenAll(ReadOnlyMemory<Task> tasks)
        {
            using (AsyncCounter counter = AsyncCounter.Rent())
            {
                counter.Init(tasks.Length);
                Regist(counter, in tasks);
                await counter.Await(default);
            }

            void Regist(AsyncCounter counter, in ReadOnlyMemory<Task> tasks)
            {
                var span = tasks.Span;
                for (int i = 0; i < span.Length; i++)
                {
                    counter.Decrement(span[i]);
                }
            }
        }
        public static async Task WhenAll(IEnumerable<Task> tasks)
        {
            using (AsyncCounter counter = AsyncCounter.Rent())
            {
                counter.Init(tasks.Count());

                foreach (var task in tasks)
                {
                    counter.Decrement(task);
                }
                await counter.Await(default);
            }
        }

        public static async Task WhenAll<TArg>(IEnumerable<Func<TArg, Task>> funcs, TArg arg)
        {
            using (AsyncCounter counter = AsyncCounter.Rent())
            {
                counter.Init(funcs.Count());
                Regist(counter, funcs, arg);
                await counter.Await(default);
            }
            void Regist(AsyncCounter counter, IEnumerable<Func<TArg, Task>> funcs, TArg arg)
            {
                foreach (var func in funcs)
                {
                    var task = func.Invoke(arg);
                    counter.Decrement(task);
                }
            }
        }
        public static async ValueTask<AwaitingResult> WhenResults<TArg>(IEnumerable<Func<TArg, Task>> funcs, TArg arg)
        {
            using (AsyncCounter counter = AsyncCounter.Rent())
            {
                counter.Init(funcs.Count());
                var result = Regist(counter, funcs, arg);
                await counter.Await(default);
                return result;
            }

            AwaitingResult Regist(AsyncCounter counter, IEnumerable<Func<TArg, Task>> funcs, TArg arg)
            {
                AwaitingResult result = new AwaitingResult(funcs.Count());
                int index = default;

                foreach (var func in funcs)
                {
                    var task = func.Invoke(arg);
                    counter.Decrement(task);
                    result[index] = task;
                    index++;
                }
                return result;
            }
        }

        public static async ValueTask<AwaitingResult> WhenResults<TArg, T>(IEnumerable<Func<TArg, Task<T>>> funcs, TArg arg)
        {
            using (AsyncCounter counter = AsyncCounter.Rent())
            {
                counter.Init(funcs.Count());
                var result = Regist(counter, funcs, arg);
                await counter.Await(default);
                return result;
            }

            AwaitingResult Regist(AsyncCounter counter, IEnumerable<Func<TArg, Task>> funcs, TArg arg)
            {
                AwaitingResult result = new AwaitingResult(funcs.Count());
                int index = default;

                foreach (var func in funcs)
                {
                    var task = func.Invoke(arg);
                    counter.Decrement(task);
                    result[index] = task;
                    index++;
                }
                return result;
            }
        }

        public static async ValueTask<AwaitingResultAsyncOperations<T>> WhenResults<TArg, T>(IEnumerable<AsyncOperation<T>> funcs, TArg arg) where T : Task
        {
            var count = funcs.Count();
            if (count > 0)
            {
                using (AsyncCounter counter = AsyncCounter.Rent())
                {
                    counter.Init(count);
                    var result = Regist(counter, funcs, arg);
                    await counter.Await(default);
                    return result;
                }
            }
            return default;

            AwaitingResultAsyncOperations<T> Regist(AsyncCounter counter, IEnumerable<AsyncOperation<T>> operations, TArg arg)
            {
                AwaitingResultAsyncOperations<T> result = new AwaitingResultAsyncOperations<T>(operations.Count());
                int index = default;

                foreach (var operation in operations)
                {
                    counter.Decrement(operation.operation);
                    result[index] = operation;
                    index++;
                }
                return result;
            }
        }


        public readonly struct AwaitingResultAsyncOperations<T> : IDisposable
        {
            public readonly int count;
            public readonly AsyncOperation<T>[] operations;
            public AsyncOperation<T> this[int index] { get => operations[index]; set => operations[index] = value; }

            public AwaitingResultAsyncOperations(int count)
            {
                this.count = count;
                if (count > 0) operations = ArrayPool<AsyncOperation<T>>.Shared.Rent(count);
                else operations = Array.Empty<AsyncOperation<T>>();
            }

            public void Dispose()
            {
                if (count > 0) ArrayPool<AsyncOperation<T>>.Shared.Return(operations);
            }
        }

        public readonly struct AwaitingResultValueTasks : IDisposable
        {
            public readonly ValueTask[] tasks;
            public readonly int count;

            public ValueTask this[int index] { get => tasks[index]; set => tasks[index] = value; }

            public AwaitingResultValueTasks(int count)
            {
                this.count = count;
                if (count > 0)
                    tasks = ArrayPool<ValueTask>.Shared.Rent(count);
                else tasks = Array.Empty<ValueTask>();
            }

            public void Dispose()
            {
                if (count > 0) ArrayPool<ValueTask>.Shared.Return(tasks);
            }
        }

        public readonly struct AwaitingResult : IDisposable
        {
            public readonly Task[] tasks;
            public readonly int count;

            public Task this[int index] { get => tasks[index]; set => tasks[index] = value; }

            public AwaitingResult(int count)
            {
                this.count = count;
                if (count > 0)
                    tasks = ArrayPool<Task>.Shared.Rent(count);
                else tasks = Array.Empty<Task>();
            }

            public void Dispose()
            {
                if (count > 0) ArrayPool<Task>.Shared.Return(tasks);
            }
        }
    }
}
