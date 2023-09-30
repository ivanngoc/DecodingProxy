using System.IO.Pipelines;

namespace IziHardGames.Libs.Pipelines.Contracts
{
    public interface IGetPipeReader
    {
        PipeReader Reader { get; }
    }
}