using IziHardGames.Libs.Networking.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using ProxyServerWebGUI.Hubs;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyServerWebGUI.Workers
{
    public class SignalRInfoService : BackgroundService
    {
        private readonly IHubContext<SignalRInfoHub> hubContext;
        public SignalRInfoService(IHubContext<SignalRInfoHub> hubContext)
        {
            this.hubContext = hubContext;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SendAsync();
        }

        private async Task SendAsync()
        {
            await Task.Delay(2000).ConfigureAwait(false);
            await hubContext.Clients.All.SendAsync(method: "logDebug", "this is m msg as string");
        }
        #region Connections To Origins
        public void AddForAll(IConnectionData data)
        {
#if DEBUG
            Console.WriteLine($"{typeof(SignalRInfoService).Name}{nameof(AddForAll)} {data.ToString()}");
#endif
            hubContext.Clients.All.SendAsync("connectionAdd", JsonSerializer.Serialize(data));
        }
        public void RemoveForAll(IConnectionData data)
        {
#if DEBUG
            Console.WriteLine($"{typeof(SignalRInfoService).Name}{nameof(RemoveForAll)} {data.ToString()}");
#endif
            hubContext.Clients.All.SendAsync("connectionRemove", JsonSerializer.Serialize(data));
        }
        public void UpdateForAll(IConnectionData data)
        {
#if DEBUG
            Console.WriteLine($"{typeof(SignalRInfoService).Name}{nameof(UpdateForAll)} {data.ToString()}");
#endif
            hubContext.Clients.All.SendAsync("connectionUpdateAll", JsonSerializer.Serialize(data));
        }

        public void UpdateForAllStatus(IConnectionData data)
        {
#if DEBUG
            Console.WriteLine($"{typeof(SignalRInfoService).Name}{nameof(UpdateForAllStatus)} {data.ToString()}");
#endif
            hubContext.Clients.All.SendAsync("connectionUpdateAll", data.Id, data.Status);
        } 
        #endregion
    }
}
