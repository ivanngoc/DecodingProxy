using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IClientPiped<TReader, TWriter> : IClient<TReader, TWriter>
    {
        Task RunWriterLoop();
        Task StopWriteLoop();
    }
}