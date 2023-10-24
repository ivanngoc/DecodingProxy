using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using DevConsole.Server.Delegates;

namespace DevConsole.Shared.Consoles
{
    public class ConsolesServer : IDisposable
    {
        private readonly ConsolesManager manager;

        public ConsolesServer(ConsolesManager manager)
        {
            this.manager = manager;
        }

        internal Task StartServer(Func<ConsoleConnection, Task> funcStart, Action22 actionUpdate, CancellationToken ct = default)
        {
            return Task.Run(async () =>
            {
                Console.WriteLine($"Console Server Initilized");
                TcpListener tcpListener = new TcpListener(IPAddress.Any, ConstantsForConsoles.SERVER_PORT);
                tcpListener.Start();
                Console.WriteLine($"Console Server Started");
                var list = new List<Task>();

                while (!ct.IsCancellationRequested)
                {
                    var tcpCLient = await tcpListener.AcceptTcpClientAsync();
                    Console.WriteLine($"Console Server accepted client");
                    TcpAdapter tcpAdapter = new TcpAdapter(tcpCLient);
                    string id = await tcpAdapter.Initilize();
                    Console.WriteLine($"Console Server client initilized:{id}");
                    ConsoleConnection consoleConnection = new ConsoleConnection(id, tcpAdapter);
                    manager.AddConnection(consoleConnection);
                    var t1 = Task.Run(async () =>
                    {
                        await funcStart(consoleConnection).ConfigureAwait(false);
                        await consoleConnection.Run((x) =>
                        {
                            actionUpdate(consoleConnection, x);
                        }, ct).ConfigureAwait(false);
                    });
                    list.Add(t1);
                    Console.WriteLine($"Console Server connection added");
                }
                Console.WriteLine($"Console Server Canceled");
                tcpListener.Stop();
                manager.Dispose();
                this.Dispose();
                Console.WriteLine($"Console Server Disposed");
                await Task.WhenAll(list.ToArray());
                Console.WriteLine($"Console Server Finished");
            });
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}