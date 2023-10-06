using System.Threading.Tasks;

namespace IziHardGames.Libs.Streams.Contracts
{
    public interface IObjectWriter<T>
    {
        Task WriteObjectAsync(T value);
    }
}
