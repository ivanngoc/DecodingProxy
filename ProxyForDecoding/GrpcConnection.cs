using Grpc.Core;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reply = IziHardGames.Proxy.gRPC.ProtobufReply;
using Request = IziHardGames.Proxy.gRPC.ProtobufRequest;

namespace IziHardGames.Proxy
{
    /// <summary>
    /// Duplex mode gRPC persistant connection
    /// </summary>
    public class GrpcConnection : IDisposable
    {
        private readonly Reply reply = new Reply() { Connection = new gRPC.ProtobufDataConnection() };
        private ConcurrentQueue<IConnectionData> queue = new ConcurrentQueue<IConnectionData>();

        public uint id;
        public IPoolReturn<GrpcConnection> pool;
        public Task task;
        public IAsyncStreamReader<Request> reader;
        public IServerStreamWriter<Reply> writer;
        public ConcurrentDictionary<uint, GrpcConnection> dic;
        public ServerCallContext context;
        private readonly AsyncAutoResetEvent are = new AsyncAutoResetEvent();

        public void Dispose()
        {
            are.Reset();
            throw new NotImplementedException();
        }
        public async Task RunAsServerNotifier(CancellationToken token)
        {
            while (!token.IsCancellationRequested || queue.Count > 0)
            {
                await are.WaitAsync().ConfigureAwait(false);
                if (queue.TryDequeue(out var data))
                {
                    reply.Connections.Capacity = 0;
                    var con = reply.Connection;
                    con.Id = data.Id;
                    con.Host = data.Host;
                    con.Port = data.Port;
                    con.Status = data.Status;
                    con.Version = data.Version;
                    con.Action = data.Action;
#if DEBUG
                    Console.WriteLine($"{typeof(GrpcConnection).Name}.{nameof(RunAsServerNotifier)}() begin sending: {data.ToInfoConnectionData()}");
#endif
                    try
                    {
                        await writer.WriteAsync(reply).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
#if DEBUG
                    Console.WriteLine($"{typeof(GrpcConnection).Name}.{nameof(RunAsServerNotifier)}() sended: {data.ToInfoConnectionData()}");
#endif
                }
            }
            if (!dic.TryRemove(new KeyValuePair<uint, GrpcConnection>(id, this)))
            {
                throw new System.ArgumentOutOfRangeException($"{GetType().FullName}.{nameof(RunAsServerNotifier)}() can't remove with key:{id}");
            }
        }

        public void Put(IConnectionData data)
        {
            queue.Enqueue(data);
            are.Set();
        }

        public void RunAsClient()
        {
            throw new NotImplementedException();
        }
    }
}