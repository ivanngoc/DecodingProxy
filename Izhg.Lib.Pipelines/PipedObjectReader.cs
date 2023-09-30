using System;
using System.Threading.Tasks;
using IziHardGames.Libs.Pipelines.Contracts;
using IziHardGames.Libs.Streams.Contracts;

namespace IziHardGames.Libs.Pipelines
{
    public abstract class PipedObjectReader<T> : IObjectReader<T>, IDisposable
    {
        protected IGetPipeReader? readerProvider;
        public void Bind(IGetPipeReader getPipeReader)
        {
            this.readerProvider = getPipeReader ?? throw new NullReferenceException();
        }
        public abstract ValueTask<T> ReadObjectAsync();

        public virtual void Dispose()
        {
            readerProvider = default;
        }
    }
}