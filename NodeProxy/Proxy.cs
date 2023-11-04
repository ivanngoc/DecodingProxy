// See https://aka.ms/new-console-template for more information
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using IziHardGames.NodeProxy.Pipes;
using IziHardGames.NodeProxy.Nodes;

namespace IziHardGames.NodeProxy
{
    public static class Proxy
    {
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
                NodeTcpAccept nodeTcpAccept = Manager.Get<NodeTcpAccept>();
                pipe.Head = nodeTcpAccept;
                nodeTcpAccept.Bind(await tcpListener.AcceptSocketAsync(ct).ConfigureAwait(false));
                pipe.Start(ct);
            }
            await Task.WhenAll(pipes.Select(x => x.task)!);
        }
    }
}