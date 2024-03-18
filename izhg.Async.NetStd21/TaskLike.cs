using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;

namespace IziHardGames.Libs.NonEngine.Async.TaskLikes
{
    public sealed class TaskLikeMethodBuilder
    {
        public TaskLikeMethodBuilder()
        {
            Console.WriteLine("TaskLikeMethodBuilder.ctor()");
        }

        public static TaskLikeMethodBuilder Create()
        {
            Console.WriteLine($"TaskLikeMethodBuilder.Static.Create()");
            return new TaskLikeMethodBuilder();
        }

        public void SetResult()
        {
            Console.WriteLine("TaskLikeMethodBuilder.SetResult()");
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            Console.WriteLine($"TaskLikeMethodBuilder.Start(). stateMachine:{stateMachine.GetType().FullName}");
            stateMachine.MoveNext();
        }

        public TaskLike Task
        {
            get
            {
                Console.WriteLine("Task Getter");
                return default(TaskLike);
            }
        }
        // AwaitOnCompleted, AwaitUnsafeOnCompleted, SetException 
        // and SetStateMachine are empty
        public void SetException(Exception e)
        {

        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
          where TAwaiter : INotifyCompletion
          where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
           where TAwaiter : ICriticalNotifyCompletion
           where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            Console.WriteLine($"TaskLikeMethodBuilder.SetStateMachine(). Type:{stateMachine.GetType().FullName}");
        }
    }

    [System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(TaskLikeMethodBuilder))]
    public struct TaskLike
    {
        public TaskLikeAwaiter GetAwaiter()
        {
            Console.WriteLine($"TaskLike.GetAwaiter()");
            return default(TaskLikeAwaiter);
        }
    }

    public struct TaskLikeAwaiter : INotifyCompletion
    {
        public void GetResult()
        {
            Console.WriteLine($"TaskLikeAwaiter.GetResult()");
        }

        public bool IsCompleted => true;

        public void OnCompleted(Action continuation)
        {
            Console.WriteLine($"TaskLikeAwaiter.OnCompleted()");
            Task.Run(async () =>
            {
                Console.WriteLine($"{DateTime.Now}\tSchedule Trigger of continuation from another thread after 2sec");
                await Task.Delay(2000);
                Console.WriteLine($"{DateTime.Now}\tCall continuation");
                continuation();
                Console.WriteLine($"Continuation Completed. Exit from scheduler thread");
            });
        }
    }

    public class TaskLikeStateMachine : IAsyncStateMachine
    {
        public void MoveNext()
        {
            throw new NotImplementedException();
        }
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotImplementedException();
        }
    }

    public class Test
    {
        public static async Task RunTest()
        {
            await ExecuteSomeAwait();
            Console.WriteLine("Test.RunTest() Completed 1");
            await ExecuteSomeAwait();
            Console.WriteLine("Test.RunTest() Completed 2");
        }
        private static async TaskLike ExecuteSomeAwait()
        {

        }
    }
}