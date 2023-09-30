using System;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.gRPC.Services;
using Microsoft.AspNetCore.SignalR;
using ProxyServerWebGUI.Constants;

namespace ProxyServerWebGUI.Hubs
{
    /// <summary>
    /// <see cref="ProxyServerWebGUI.Workers.SignalRInfoService"/>
    /// </summary>
    public class SignalRInfoHub : Hub
    {
        private static SignalRInfoHub singleton;
        private static readonly object lockSingleton = new object();
        private GrpcHubService grpcService;
        public static int countCreated; 

        public SignalRInfoHub()
        {
            Interlocked.Increment(ref countCreated);

            if (singleton == null)
            {
                lock (lockSingleton)
                {
                    if (singleton == null)
                    {
                        singleton = this;
                    }
                    else
                    {
                        Console.WriteLine($"Duplicate detected: {GetType().FullName}");
                    }
                }
            }
        }

        public async Task SyncTextBox(string textBox)
        {
            await Clients.All.SendAsync(method: "syncTextBox", textBox).ConfigureAwait(false);
        }

        public async Task NotifyOthers(string content)
        {
            await Clients.Others.SendAsync(method: "notify", content).ConfigureAwait(false);
        }

        public async Task NotifyStart(string content)
        {
#if DEBUG
            Console.WriteLine($"{typeof(SignalRInfoHub).Name}.{nameof(NotifyStart)} SignalR Client:{Context.ConnectionId} Recived:{content}");
#endif   
            await Groups.AddToGroupAsync(Context.ConnectionId, "notify-me").ConfigureAwait(false);
        }
        public async Task NotifyEnd(string content)
        {
#if DEBUG
            Console.WriteLine($"{typeof(SignalRInfoHub).Name}.{nameof(NotifyEnd)} SignalR Client:{Context.ConnectionId} Recived:{content}");
#endif   
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "notify-me").ConfigureAwait(false);
        }

        public async Task SubscribeConnectionsAdd(string content)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ConstantsForWebGUI.SignalR.GROUPE_CONNECTIONS_ADD).ConfigureAwait(false);
        }
    }
}
