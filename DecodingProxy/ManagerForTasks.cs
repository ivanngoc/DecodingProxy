// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Diagnostics;

namespace IziHardGames.Proxy
{
    public static class ManagerForTasks
    {
        private static List<Task> tasks = new List<Task>();             

        public static void Watch(Task task)
        {
            lock (tasks)
            {
                tasks.Add(task);
            }
        }

        public static void CkeckErrors(Task task)
        {
            if (task.Exception != null)
            {
                Logger.LogException(task.Exception);
            }
        }
        public static void CkeckErrors()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                if (stopwatch.ElapsedMilliseconds > 10000)
                {
                    lock (tasks)
                    {
                        for (int i = 0; i < tasks.Count; i++)
                        {
                            var task = tasks[i];

                            if (task.Exception != null)
                            {
                                Logger.LogException(task.Exception);
                                tasks.RemoveAt(i);
                                i--;
                                continue;
                            }
                            if (task.IsCompleted)
                            {
                                tasks.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    stopwatch.Restart();
                }
            }
        }

        internal static void Watch(object value)
        {
            throw new NotImplementedException();
        }
    }
}