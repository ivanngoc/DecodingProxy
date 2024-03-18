using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Async.NetStd21
{
    public class IziSignaler : INotifyCompletion
    {
        internal int countCompletions;
        internal int countAwaits;
        internal Action? continuation;
        public bool IsCompleted => countCompletions > 0;

        public IziSignaler Await(int count)
        {
            if (countCompletions >= count) return this;
            throw new System.NotImplementedException();
        }
        public IziSignaler Await()
        {
            if (countCompletions > 0) return this;
            var result = Interlocked.Increment(ref countAwaits);
            if (result != 1) throw new NotSupportedException("There can be only 1 await MAX");
            return this;
        }

        public IziSignaler GetAwaiter()
        {
            return this;
        }

        public void SetCancel()
        {
            this.continuation = default; // there wont be any continuation but [Action continuation] is still a field on StateMachine.
            throw new System.NotImplementedException();
        }
        public void SetComplete()
        {
            if (countAwaits > 0)
            {
                var copy = continuation!;
                continuation = null;
                Interlocked.Decrement(ref countAwaits);
                copy.Invoke();
            }
            else
            {
                Interlocked.Increment(ref countCompletions);
            }
        }

#if DEBUG
        public static async Task Test()
        {
            IziSignaler signaler = new IziSignaler();

            var t1 = Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine("Begin await");
                    await signaler.Await();
                    Console.WriteLine("Await complete");
                }
            });

            var t2 = Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine($"await to send result Begin");
                    await Task.Delay(1000);
                    Console.WriteLine($"await to send result Complete");
                    signaler.SetComplete();
                }
            });
            await Task.WhenAll(t1, t2);
        }
#endif
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
        public void GetResult()
        {
            Interlocked.Decrement(ref countCompletions);
        }
    }
}
