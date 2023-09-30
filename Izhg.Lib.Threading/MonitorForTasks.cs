using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IziHardGames.Libs.Concurrency
{
    public class MonitorForTasks
    {
        private readonly ILogger logger;
        private readonly List<Task> tasks = new List<Task>();

        public MonitorForTasks(ILogger logger)
        {
            this.logger = logger;
        }

        public void Watch(Task task)
        {
            lock (tasks)
            {
                tasks.Add(task);
            }
            task.ContinueWith(t =>
            {
                lock (tasks)
                {
                    tasks.Remove(t);
                }
                if (t.IsFaulted) logger.LogError(new EventId(t.GetHashCode()), t.Exception, "Error while obserbing task");
            });
        }
    }
}