using System;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.ForHttp.Monitoring
{
    public class InfoConnection : IDisposable
    {
        public uint id;
        public int bytesRecived;
        public int bytesSended;
        public int BytesTotal => bytesRecived + bytesSended;
        public string host = string.Empty;
        public int port;
        public ENetworkProtocols protocols;
        public EConnectionFlags flags;
        public EHttpConnectionStates states;
        private List<LogEntry> logs = new List<LogEntry>();

        internal void Initilize(uint idConnection)
        {
            this.id = idConnection;
        }

        public void Dispose()
        {
            this.id = default;
            this.bytesRecived = default;
            this.bytesSended = default;

            this.host = string.Empty;
            this.port = default;

            this.flags = EConnectionFlags.None;
            this.states = EHttpConnectionStates.None;
            this.protocols = ENetworkProtocols.None;

            foreach (var log in logs)
            {
                PoolObjectsConcurent<LogEntry>.Shared.Return(log);
                log.Dispose();
            }
            logs.Clear();
        }

        internal void SetHostAndPort(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        internal void AddState(EHttpConnectionStates originConnected)
        {
            states |= originConnected;
        }

        internal void UpdateFlags(EConnectionFlags flags)
        {
            this.flags = flags;
        }

        internal void UpdateProtocols(ENetworkProtocols protocols)
        {
            this.protocols = protocols;
        }

        internal void AddLog(int groupe, string message)
        {
            var log = PoolObjectsConcurent<LogEntry>.Shared.Rent();
            log.Initilize(groupe, message);
        }
        private class LogEntry : IDisposable
        {
            public int groupe;
            public string message = string.Empty;

            public void Dispose()
            {
                message = string.Empty;
            }
            internal void Initilize(int groupe, string message)
            {
                this.groupe = groupe;
                this.message = message;
            }
        }
    }

}