using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ProxyServerWebGUI.Hubs
{
    public class SignalRInfoHub : Hub
    {
        private static SignalRInfoHub singleton;
        private static readonly object lockSingleton = new object();
        public SignalRInfoHub()
        {
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
    }
}
