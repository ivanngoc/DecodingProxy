using System;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IClient : IPerfTracker, IApplyControl, ICheckConnection
    {
        string Title { get; }
        int Available { get; }
        void ConsumeLife();
        Task SendAsync(ReadOnlyMemory<byte> mem, CancellationToken token);
        Task RunWriterLoop();
        Task StopWriteLoop();
        /// <summary>
        /// Connection Got Data To Handle
        /// </summary>
        /// <returns></returns>
        bool CheckData();
    }
}