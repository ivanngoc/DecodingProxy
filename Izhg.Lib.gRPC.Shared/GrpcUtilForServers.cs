using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Izhg.Libs.gRPC.Shared
{
    public class GrpcUtilForServers
    {
        public static async Task<bool> HandleBidirectional<T>(IAsyncStreamReader<T> reader, IServerStreamWriter<T> responseStream, ServerCallContext context, Func<T, T> func, CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                if (await reader.MoveNext(context.CancellationToken))
                {
                    T item = reader.Current;
                    T response = func(item);
                    await responseStream.WriteAsync(response).ConfigureAwait(false);
                }
                else return false;
            }
            return true;
        }
    }
}