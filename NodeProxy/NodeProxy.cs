using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using IziHardGames.NodeProxies.Pipes;
using IziHardGames.NodeProxies.Nodes;
using IziHardGames.NodeProxies.Graphs;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;

namespace IziHardGames.NodeProxies.Version1
{
    public static class NodeProxy
    {
        public static async Task RunSmartTcp(int port, CancellationToken ct = default)
        {
            Console.WriteLine($"Begin Run smart tcp");
            TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Any, port);
            tcpListener.Start();
            while (!ct.IsCancellationRequested)
            {
                var socket = await tcpListener.AcceptSocketAsync().ConfigureAwait(false);
                Console.WriteLine($"Socket accepted");
                var advancer = ProxyNodeAdvancer.GetNew();
                var graph = IziGraph.GetNew(advancer, new RegistryIziNodes());
                graph.indexators[typeof(Indexator<string, Node>)] = new Indexator<string, Node>();
                await (graph.Advancer as ProxyNodeAdvancer)!.RunAsyncV2(socket, ct);
            }
        }
        public static async Task StartTcp(int port, CancellationToken ct = default)
        {
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            ct = cts.Token;

            Schema schema = await Schema.FromFileXML("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\NodeProxy\\Examples\\schema.xml").ConfigureAwait(false);
            TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Any, port);
            tcpListener.Start();
            List<Pipe> pipes = new List<Pipe>();

            while (!cts.IsCancellationRequested)
            {
                Pipe pipe = new Pipe(schema);
                pipes.Add(pipe);
                NodeTcpAcceptStart nodeTcpAccept = Manager.Get<NodeTcpAcceptStart>();
                pipe.Head = nodeTcpAccept;
                nodeTcpAccept.Bind(await tcpListener.AcceptSocketAsync(ct).ConfigureAwait(false));
                pipe.Start(ct);
            }
            await Task.WhenAll(pipes.Select(x => x.task)!);
        }
    }
}