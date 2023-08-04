using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Pipelines.Contracts
{

    public interface IReader
    {
        ValueTask<ReadResult> ReadAsync(CancellationToken token = default);
        void ReportConsume(SequencePosition position);
    }
}