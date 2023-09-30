namespace System.Threading.Tasks
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
}