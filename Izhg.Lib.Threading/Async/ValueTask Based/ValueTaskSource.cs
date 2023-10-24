using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using IziHardGames.Libs.Async.Contracts;

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
}
