using System.IO.Pipelines;

namespace IziHardGames.Libs.Pipelines.Contracts
{
    public interface IGetPipeWriter
    {
        PipeReader Writer { get; }
    }
}