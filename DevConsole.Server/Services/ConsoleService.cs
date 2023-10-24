using System;
using System.Threading;
using System.Threading.Tasks;
using DevConsole.Server.Delegates;
using DevConsole.Shared.Consoles;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace DevConsole.Server.Services
{
    internal class ConsoleService : BackgroundService
    {
        private readonly ConsoleHub hub;
        private readonly IHubContext<ConsoleHub> hubContext;
        internal readonly Action22 actionUpdate;
        internal readonly Func<ConsoleConnection, Task> funcStart;
        internal readonly ConsolesServer server;

        private Task tail = Task.CompletedTask;

        public ConsoleService(IHubContext<ConsoleHub> hubContext, ConsoleHub hub, ConsolesServer server)
        {
            this.hubContext = hubContext;
            this.hub = hub;
            actionUpdate = Update;
            funcStart = FuncStart;
            this.server = server;
        }

        private async Task FuncStart(ConsoleConnection connection)
        {
            await NotifyOnConsoleConnect(connection, default).ConfigureAwait(false);
        }
        private void Update(ConsoleConnection connection, LogItem text)
        {
            //tail = tail.ContinueWith(async (x) =>
            //{
            //    await WriteLine($"id:{connection.id}\t{text}");
            //}); 
            var t1 = Task.Run(async () =>
            {
                await WriteLine($"id:{connection.id}\t{text}").ConfigureAwait(false);
            });
            t1.Wait();
        }

        protected override Task ExecuteAsync(CancellationToken ct = default)
        {
            return SendFromReadline(ct);
        }

        private Task SendFromReadline(CancellationToken ct = default)
        {
            var t1 = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    var value = Console.ReadLine();
                    if (!string.IsNullOrEmpty(value))
                    {
                        await WriteLine($"{value}\r\n", ct).ConfigureAwait(false);
                    }
                }
            });
            return t1;
        }

        public async Task Write(string text, CancellationToken ct = default)
        {
            await hubContext.Clients.All.SendAsync("write-text", text, ct).ConfigureAwait(false);
        }
        public async Task WriteLine(string text, CancellationToken ct = default)
        {
            await hubContext.Clients.All.SendAsync("write-line", text, ct).ConfigureAwait(false);
        }

        public async Task NotifyOnConsoleConnect(ConsoleConnection connection, CancellationToken ct = default)
        {
            await hubContext.Clients.All.SendAsync("connect-console", connection.ToJsonString(), ct).ConfigureAwait(false);
        }
        public async Task NotifyOnConsoleDisconnect(ConsoleConnection connection, CancellationToken ct = default)
        {
            await hubContext.Clients.All.SendAsync("disconnect-console", connection.ToJsonString(), ct).ConfigureAwait(false);
        }
    }
}
