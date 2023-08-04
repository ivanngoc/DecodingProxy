using System.Threading;
using System.Threading.Tasks;

namespace System
{
    public static class ExtensionsTaskMonads
    {
        public static async Task<R> Bind<T, R>(this Task<T> task, Func<T, Task<R>> cont)
        => await cont(await task.ConfigureAwait(false)).ConfigureAwait(false);

        public static async Task<R> Map<T, R>(this Task<T> task, Func<T, R> map)
        => map(await task.ConfigureAwait(false));     

        public static Task<T> Otherwise<T>(this Task<T> task, Func<Task<T>> otherTask)
        {
            return task.ContinueWith(async innerTask =>
            {
                if (innerTask.Status == TaskStatus.Faulted)
                {
                    return await otherTask();
                }
                return innerTask.Result;
            }
            ).Unwrap();
        }
    }

    public static class HelperTasks
    {
        public static Task<T> Return<T>(T task) => Task.FromResult(task);

        public static async Task<T> Retry<T>(Func<Task<T>> task, int retries, TimeSpan delay, CancellationToken cts = default(CancellationToken))
        {
            return await task().ContinueWith(async innerTask =>
            {
                cts.ThrowIfCancellationRequested();

                if (innerTask.Status != TaskStatus.Faulted)
                {
                    return innerTask.Result;
                }
                if (retries == 0)
                {
                    throw innerTask.Exception ?? throw new Exception();
                }
                await Task.Delay(delay, cts);

                return await Retry(task, retries - 1, delay, cts);
            }).Unwrap();
        }
    }
}