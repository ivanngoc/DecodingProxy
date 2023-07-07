using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Streaming
{
    public abstract class ReusableStream : Stream
    {
        protected ReusableStream()
        {

        }

        public static T Rent<T>() where T : ReusableStream
        {
            throw new NotImplementedException();
        }
    }
}