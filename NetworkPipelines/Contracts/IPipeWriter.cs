using System.IO.Pipelines;

namespace IziHardGames.Libs.Networking.Pipelines.Contracts
{
    public interface IPipeWriter
    {
        PipeReader Writer { get; }
    }
}