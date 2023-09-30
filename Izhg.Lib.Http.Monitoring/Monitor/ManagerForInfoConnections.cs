using System.Collections.Concurrent;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Libs.NonEngine.Memory;
using static IziHardGames.Libs.ForHttp.Http20.HeadersFrame;

namespace IziHardGames.Libs.ForHttp.Monitoring
{
    public class ManagerForInfoConnections
    {
        public readonly ConcurrentDictionary<uint, InfoConnection> connections = new ConcurrentDictionary<uint, InfoConnection>();

        internal void AddLog(uint idConnection, int groupe, string message)
        {
            connections[idConnection].AddLog(groupe, message);
        }

        internal void AddState(uint idConnection, EHttpConnectionStates originConnected)
        {
            connections[idConnection].AddState(originConnected);
        }

        internal void Create(uint idConnection)
        {
            var rent = PoolObjectsConcurent<InfoConnection>.Shared.Rent();
            rent.Initilize(idConnection);
            if (!connections.TryAdd(idConnection, rent))
            {
                throw new System.NotImplementedException();
            }
        }

        internal void SetHostAndPort(uint idConnection, string host, int port)
        {
            connections[idConnection].SetHostAndPort(host, port);
            TryApplyUpdates(idConnection);
        }

        internal void UpdateFlags(uint idConnection, EConnectionFlags flags)
        {
            connections[idConnection].UpdateFlags(flags);
        }

        internal void UpdateProtocols(uint idConnection, ENetworkProtocols protocols)
        {
            connections[idConnection].UpdateProtocols(protocols);
        }

        /// <summary>
        /// Update GUI
        /// </summary>
        /// <param name="idConnection"></param>
        private void TryApplyUpdates(uint idConnection)
        {

        }
    }
}