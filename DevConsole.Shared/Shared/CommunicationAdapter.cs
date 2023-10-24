using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DevConsole.Shared.Consoles;

namespace DevConsole.Shared
{
    /// <summary>
    /// Адаптер для отправки и получения данных между процессами. Для каждого канала связи свой адаптер. Например для gRPC или для SignalR или для NetworkStream и т.д.
    /// </summary>
    public abstract class CommunicationAdapter
    {
        public abstract Task<string> ReadLineAsync(CancellationToken ct);
        public abstract Task<string> Initilize();
        public abstract Task<LogHeader> ReadHeaderAsync();
    }
}