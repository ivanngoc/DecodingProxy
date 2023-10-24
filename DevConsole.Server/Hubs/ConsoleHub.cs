using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DevConsole.Shared.Consoles;
using Microsoft.AspNetCore.SignalR;

namespace DevConsole.Server.Services
{
    internal class ConsoleHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private ConsolesServer consolesServer;
        private ConsolesManager manager;

        public ConsoleHub(ConsolesServer consolesServer, ConsolesManager manager)
        {
            this.consolesServer = consolesServer;
            this.manager = manager;
        }
        public async Task NotifyStart(string content)
        {
#if DEBUG
            Console.WriteLine($"{typeof(ConsoleHub).Name}.{nameof(NotifyStart)} SignalR Client:{Context.ConnectionId} Recived:{content}");
#endif
            await Groups.AddToGroupAsync(Context.ConnectionId, "notify-me").ConfigureAwait(false);
        }

        public async Task GetLogs(JsonObject jObj)
        {
            string id = ((string)jObj["id"]!) ?? throw new NullReferenceException();
            var con = manager.GetConnection(id);
            var result = con.ToJsonObject();
            await Clients.Caller.SendAsync("fill-console", result).ConfigureAwait(false);
        }
        public async Task RequestInitilization(string args)
        {
            string json = manager.GetConnectionsDataAsJson();
            Console.WriteLine($"RequestInitilization:{json}");
            await Clients.Caller.SendAsync("initilize", json).ConfigureAwait(false);
        }
    }
}
