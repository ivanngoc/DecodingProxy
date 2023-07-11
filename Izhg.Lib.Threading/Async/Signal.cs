using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Async
{
    public class Signal
    {

        public void SignalRaise()
        {


        }
        public void SignalConsume()
        {

        }

        public void Block()
        {

        }

        public ValueAwaiter<int> GetAwaiter()
        {
            return new ValueAwaiter<int>();
        }
        /// <summary>
        ///  <see cref=" System.Runtime.CompilerServices.TaskAwaiter"/>
        /// </summary>
        /// <returns></returns>
        public static async Task Test()
        {
            Console.WriteLine("Test Async Begin");
            Signal signal = new Signal();
            await signal;
            //TaskCompletionSource
            Console.WriteLine("Await Ended");
        }
    }

    public class ValueAwaiter<T> : INotifyCompletion
    {
        private readonly T value;
        // auto reset event doesn't integrated to threadpool Api? and make thread just to stall?
        private AutoResetEvent are = new AutoResetEvent(false);

        public bool IsCompleted { get => CheckCompleted(); }

        public void OnCompleted(Action continuation)
        {
            Console.WriteLine("OnCompleted executed");

            if (continuation != null)
            {
                Task.Run(continuation);
            }
        }
        public T GetResult()
        {
            are.WaitOne();
            Console.WriteLine("Get result executed");
            return value;
        }

        public bool CheckCompleted()
        {
            Console.WriteLine("Check completed called");
            return false;
        }
    }
}
