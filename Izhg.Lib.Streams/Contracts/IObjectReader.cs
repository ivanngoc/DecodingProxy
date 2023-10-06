using System.Threading.Tasks;

namespace IziHardGames.Libs.Streams.Contracts
{
    public interface IObjectReader<T>
    {
        ValueTask<T> ReadObjectAsync();
    }
}
