// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Text;

public class Example
{
    public static async Task HandleDisconnect(TcpClient tcp, TcpClient tcp2, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (tcp.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];

                if (tcp.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    // Client disconnected
                    Console.WriteLine("The requesting client has dropped its connection.");
                    cancellationToken = new CancellationToken(true);
                    break;
                }
            }
            if (tcp2.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];

                if (tcp2.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    // Server disconnected
                    Console.WriteLine("The destination client has dropped its connection.");
                    cancellationToken = new CancellationToken(true);
                    break;
                }
            }

            await Task.Delay(1);
        }
    }


    private static async Task HandleClient(TcpClient clt)
    {
        List<Task> tasks = new List<Task>();
        var bytes = new byte[clt.ReceiveBufferSize];
        var hostHeaderAvailable = 0;
        NetworkStream requestStream = null;
        const string connectText = "connect";

        try
        {
            using (NetworkStream proxyStream = clt.GetStream())
            using (TcpClient requestClient = new TcpClient())
            {
                proxyStream.ReadTimeout = 100;
                proxyStream.WriteTimeout = 100;

                if (proxyStream.DataAvailable && hostHeaderAvailable == 0)
                {
                    await proxyStream.ReadAsync(bytes, 0, bytes.Length);

                    var text = Encoding.UTF8.GetString(bytes);
                    Console.WriteLine(text);

                    if (text.ToLower().StartsWith(connectText))
                    {
                        // extract the url and port
                        var host = text.Remove(0, connectText.Length + 1);
                        var hostIndex = host.IndexOf(" ", StringComparison.Ordinal);
                        var hostEntry = host.Remove(hostIndex).Split(new[] { ":" }, StringSplitOptions.None);
                        // connect to the url and prot supplied
                        await requestClient.ConnectAsync(hostEntry[0], Convert.ToInt32(hostEntry[1]));
                        requestStream = requestClient.GetStream();

                        requestStream.ReadTimeout = 100;
                        requestStream.WriteTimeout = 100;

                        // send 200 response to proxyStream 
                        const string sslResponse = "HTTP/1.0 200 Connection established\r\n\r\n";
                        var sslResponseBytes = Encoding.UTF8.GetBytes(sslResponse);
                        await proxyStream.WriteAsync(sslResponseBytes, 0, sslResponseBytes.Length);
                    }
                }
                hostHeaderAvailable++;

                CancellationToken cancellationToken = new CancellationToken(false);

                Task task = proxyStream.CopyToAsync(requestStream, cancellationToken);
                Task task2 = requestStream.CopyToAsync(proxyStream, cancellationToken);
                Task handleConnection = HandleDisconnect(clt, requestClient, cancellationToken);

                tasks.Add(task);
                tasks.Add(task2);
                tasks.Add(handleConnection);

                await Task.WhenAll(tasks).ConfigureAwait(false);

                // close conenctions
                clt.Close();
                clt.Dispose();
                requestClient.Close();
                requestClient.Dispose();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}