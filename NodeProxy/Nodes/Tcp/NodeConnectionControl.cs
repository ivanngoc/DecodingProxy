using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    /// <summary>
    /// Patterns "Observer"+"Report Collector"
    /// </summary>
    internal class NodeConnectionControl : Node, ISupportNode
    {
        private Socket? socket;
        private int timesZeroAvailable;
        /// <summary>
        /// сколько раз при проверке на доступность байтов нужно получить чтобы счить подключение как простаивающее и запустит проверку на форсированное отключение
        /// </summary>
        private const int THRESHHOLD = 15;
        private const int TIMEOUT = 200;
        private long totalBytesRead;
        private long totalBytesWrite;
        private TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

        public override ENodeRunFlags GetFlags()
        {
            return ENodeRunFlags.Async | ENodeRunFlags.Sustainable;
        }
        internal override async Task ExecuteAsync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                if (socket!.Available > 0)
                {
                    timesZeroAvailable = 0;
                }
                else
                {
                    timesZeroAvailable++;
                }
                if (THRESHHOLD < timesZeroAvailable)
                {
                    if (CheckToForceDisconnect())
                    {
                        Disconnect();
                    }
                }
                await Task.Delay(TIMEOUT).ConfigureAwait(false);
            }
        }

        private void Disconnect()
        {
            taskCompletionSource.SetResult(0);
        }

        private bool CheckToForceDisconnect()
        {
            return false;
        }

        internal void ReportRead(int readed)
        {
            totalBytesRead += readed;
        }
        internal void ReportWrite(int writed)
        {
            totalBytesWrite += writed;
        }
        public void SetSocket(Socket socket)
        {
            this.socket = socket;
        }
        internal Task AwaitDisconnect(CancellationToken ct)
        {
            if (!socket?.Connected ?? true)
            {
                return Task.CompletedTask;
            }
            return taskCompletionSource.Task;
        }

        internal override async Task AwaitCompletion(CancellationToken ct)
        {
            await AwaitDisconnect(ct).ConfigureAwait(false); ;
        }
    }
}
