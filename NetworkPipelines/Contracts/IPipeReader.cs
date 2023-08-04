using System.IO.Pipelines;

namespace IziHardGames.Libs.Networking.Pipelines.Contracts
{
    public interface IPipeReader
    {
        PipeReader Reader { get; }
    }
}