using System.Net.Sockets;

namespace IziHardGames.Libs.IO
{
    public static class Connection
    {
        public static bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
    }
}
