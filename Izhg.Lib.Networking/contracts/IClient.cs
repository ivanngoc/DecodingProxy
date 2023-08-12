using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IGetLogger
    {
        ILogger Logger { get; }
    }

    public interface IClient<out TReader, out TWriter> : IApplyControl, ICheckConnection, IGetLogger
    {
        string Title { get; }
        /// <summary>
        /// <see cref="Socket.Available"/>
        /// </summary>
        int Available { get; }
        void ConsumeLife();
        TReader Reader { get; }
        TWriter Writer { get; }
        /// <summary>
        /// Connection Got Data To Handle
        /// </summary>
        /// <returns></returns>
        bool CheckData();
    }
}