using System.Collections.Concurrent;
using System;
using System.Text.Json.Nodes;
using System.Linq;

namespace DevConsole.Shared.Consoles
{
    public class ConsolesManager : IDisposable
    {
        internal ConcurrentDictionary<string, ConsoleConnection> consoles = new ConcurrentDictionary<string, ConsoleConnection>();

        internal void AddConnection(ConsoleConnection connection)
        {
            if (!consoles.TryAdd(connection.id, connection)) throw new ArgumentException($"Failed to accept {nameof(ConsoleConnection)}");
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ConsoleConnection GetConnection(string id)
        {
            return consoles[id];
        }
        public string GetConnectionsDataAsJson()
        {
            JsonObject jObj = new JsonObject()
            {
                ["connections"] = new JsonArray(consoles.Values.Select(x => x.ToJsonObject()).ToArray()),
            };
            return jObj.ToJsonString();
        }
    }
}