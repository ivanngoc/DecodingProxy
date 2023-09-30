namespace System.Threading.Tasks
{
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