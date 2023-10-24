using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DevConsole.Shared.Consoles
{
    /// <summary>
    /// Recieve data from ConsoleClient
    /// </summary>
    public class ConsoleConnection
    {
        public string id;
        private CommunicationAdapter adapter;
        private readonly Queue<LogItem> logs = new Queue<LogItem>();
        private string json;

        internal ConsoleConnection(string id, CommunicationAdapter adapter)
        {
            this.id = id;
            this.adapter = adapter;
        }

        public JsonNode GetLogsAsJsonObject()
        {
            var el = JsonSerializer.SerializeToElement(logs);
            var array = JsonArray.Create(el) ?? throw new NullReferenceException();
            return array;
        }
        public JsonObject ToJsonObject()
        {
            // избыточная аллокация
            JsonObject jObj = new JsonObject()
            {
                ["id"] = id,
                ["logs"] = GetLogsAsJsonObject(),
            };
            return jObj;
        }

        public string ToJsonString()
        {
            if (string.IsNullOrEmpty(json))
            {
                json = ToJsonObject().ToJsonString();
            }
            return json;
        }

        internal async Task Run(Action<LogItem> action, CancellationToken ct = default)
        {
            while (true)
            {
                LogHeader header = await adapter.ReadHeaderAsync().ConfigureAwait(false);
                var line = await adapter.ReadLineAsync(ct).ConfigureAwait(false);
#if DEBUG
                Console.WriteLine(line);
#endif
                if (logs.Count == ConstantsForConsoles.MAX_LOGS)
                {
                    logs.Dequeue();
                }
                LogItem item = new LogItem();
                logs.Enqueue(item);
                action(item);
            }
        }
    }

    public class LogItem
    {
        public LogHeader meta;
        public byte[] bytes;
    }
}