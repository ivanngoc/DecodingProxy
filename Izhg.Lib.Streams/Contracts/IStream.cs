using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Streams.Contracts
{
    public interface IObjectStream<T> : IObjectWriter<T>, IObjectReader<T> { }

    public interface IObjectProducer<T>
    {
        ValueTask<T> AwaitObject();
    }

    public interface IObjectWriter<T>
    {
        Task WriteObjectAsync(T value);
    }

    public interface IObjectReader<T>
    {
        ValueTask<T> ReadObjectAsync();
    }

    /// <summary>
    /// Split input to several outputs
    /// </summary>
    public interface IDemultiplexer
    {

    }

    public interface IStream
    {

    }
}
