using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Pools.Abstractions.NetStd21;
using AsyncSignaler = IziHardGames.Libs.Async.AsyncSignaler;

namespace IziHardGames.Libs.Streams
{
    public class HelperForStreams
    {
        public static async ValueTask<EStatus> CopyStreamToStream(Stream from, Stream to, int bufferSize = 8192, CancellationToken ct = default)
        {
            ConcurrentQueue<BufferSegment> segments = PoolObjectsConcurent<ConcurrentQueue<BufferSegment>>.Shared.Rent();
            AsyncSignaler control = AsyncSignaler.Rent();

            var t2 = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    if (await control.Await(ct).ConfigureAwait(false))
                    {
                        if (segments.TryDequeue(out var segment))
                        {
                            await to.WriteAsync(segment.GetReadOnlyMemory(), ct).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            });

            while (!ct.IsCancellationRequested)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    var t1 = from.ReadAsync(buffer, ct);
                    var readed = await t1.ConfigureAwait(false);
                    if (readed > 0)
                    {
                        segments.Enqueue(new BufferSegment(buffer, readed));
                        control.Set();
                    }
                    else
                    {
                        await Task.Delay(ConstantsForStream.Timeouts.DEFAULT_ZERO_READ_TIMEOUT).ConfigureAwait(false);
                    }
                }
                catch (System.Exception ex)
                {
                    return EStatus.Faulted;
                }
            }
            await t2.ConfigureAwait(false);
            control.Dispose();
            return EStatus.FinishedProperly;
        }
    }

    public enum EStatus
    {
        None,
        Faulted,
        FinishedProperly,
        Canceled,
    }
}
