using System.Threading.Tasks;

namespace IziHardGames.Libs.Streams.Contracts
{
    public interface IObjectProducer<T>
    {
        ValueTask<T> AwaitObject();
    }
}
