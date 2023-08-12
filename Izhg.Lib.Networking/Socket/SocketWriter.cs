using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public abstract class SocketWriter : SocketProcessor
    {
        public virtual async Task SendAsync(byte[] array, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
        public virtual async Task SendAsync(ReadOnlyMemory<byte> readOnlyMemory, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
    }
}