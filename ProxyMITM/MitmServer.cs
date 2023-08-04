using System.Net.Security;
using System.Net.Sockets;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using IziHardGames.Networking.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace IziHardGames.Proxy.MITM
{
    /// https://stackoverflow.com/questions/36198931/direct-ssl-tls-no-connect-message-mitm-proxy-using-c-sharp-sslstream
    public class MitmServer
    {
        //public int port = 49704;
        public int port = 443;
        public const string certPwd = "1234";
        private readonly object lockFile = new object();
        public static int clientNextId = 1000;
        public static void Test()
        {
            MitmServer sslTcpProxy = new MitmServer();
            sslTcpProxy.Run();

        }
        public void Run()
        {
            // Create a TCP/IP (IPv4) socket and listen for incoming connections.
            TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            tcpListener.Start();

            Console.WriteLine($"Server listening on 127.0.0.1:{port}");
            Console.WriteLine();
            Console.WriteLine("Waiting for a client to connect...");
            Console.WriteLine();

            while (true)
            {
                // Application blocks while waiting for an incoming connection.
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Task.Run(() => AcceptConnection(tcpClient));
            }
        }

        private void AcceptConnection(TcpClient client)
        {
            clientNextId++;
            try
            {
                //string host2 = "www.google.com";
                string siteName = "zeroscans.com";
                string host2 = siteName;
                string SERVER_NAME = "localhost";
                //string serverIp = "104.21.80.237";
                string serverIp = "172.67.155.149";
                // Using a pre-created certificate.
                string certFilePath = @"C:\Users\ngoc\Documents\[Projects] C#\DecodingProxy\DecodingProxy\cert\ProxyMitmServer.cer";

                X509Certificate2 certificate;

                try
                {
                    certificate = new X509Certificate2(certFilePath, certPwd);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not create the certificate from file from {certFilePath}", ex);
                }

                SslStream clientSslStream = new SslStream(client.GetStream(), false);
                clientSslStream.AuthenticateAsServer(certificate, false, SslProtocols.Tls12, false);

                // Display the properties and settings for the authenticated as server stream.
                Console.WriteLine("clientSslStream.AuthenticateAsServer");
                Console.WriteLine("------------------------------------");
                DisplaySecurityLevel(clientSslStream);
                DisplaySecurityServices(clientSslStream);
                DisplayCertificateInformation(clientSslStream);
                DisplayStreamProperties(clientSslStream);

                Console.WriteLine();


                //var ip = HelperIpAddress.FindIpByHostNameAndDNS(siteName, "8.8.8.8");
                // The Ip address of the server we are trying to connect to.
                // Dont use the URI as it will resolve from the host file.
                TcpClient server = new TcpClient(serverIp, port);
                SslStream serverSslStream = new SslStream(server.GetStream(), false, SslValidationCallback, null);
                serverSslStream.AuthenticateAsClient(host2);

                // Display the properties and settings for the authenticated as server stream.
                Console.WriteLine("serverSslStream.AuthenticateAsClient");
                Console.WriteLine("------------------------------------");
                DisplaySecurityLevel(serverSslStream);
                DisplaySecurityServices(serverSslStream);
                DisplayCertificateInformation(serverSslStream);
                DisplayStreamProperties(serverSslStream);

                new Task(() => ReadFromClient(client, server, clientSslStream, serverSslStream, clientNextId)).Start();
                new Task(() => ReadFromServer(client, server, serverSslStream, clientSslStream, clientNextId)).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        private static Boolean SslValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        private static void ReadFromServer(TcpClient client, TcpClient server, SslStream serverStreams, SslStream clientStreams, int clientNextId)
        {
            Byte[] message = new Byte[4096];

            Int32 serverBytes;

            FileInfo fileInfo = new FileInfo($"serverToClient_{clientNextId}.log");
            FileInfo fileInfoRaw = new FileInfo($"serverToClient_{clientNextId}.raw");

            if (!fileInfo.Exists)
            {
                fileInfo.Create().Dispose();
            }
            if (!fileInfoRaw.Exists)
            {
                fileInfoRaw.Create().Dispose();
            }


            //Stream from = server.GetStream();
            //Stream to = client.GetStream();
            Stream from = serverStreams;
            Stream to = clientStreams;

            try
            {
                using (FileStream fs = fileInfo.OpenWrite())
                {
                    while ((serverBytes = from.Read(message, 0, message.Length)) > 0)
                    {
                        to.Write(message, 0, serverBytes);
                        fs.Write(message, 0, serverBytes);
                    }
                }
            }
            catch
            {
                // Whatever
            }
        }

        private static void ReadFromClient(TcpClient client, TcpClient server, SslStream clientStreams, SslStream serverStreams, int clientNextId)
        {
            Byte[] buffer = new Byte[4096];

            FileInfo fileInfo = new FileInfo($"clientToServer_{clientNextId}.log");

            if (!fileInfo.Exists)
            {
                fileInfo.Create().Dispose();
            }

            //Stream from = client.GetStream();
            //Stream to = server.GetStream();
            Stream from = clientStreams;
            Stream to = serverStreams;

            using (FileStream stream = fileInfo.OpenWrite())
            {
                while (true)
                {
                    Int32 clientBytes;

                    try
                    {
                        clientBytes = from.Read(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        break;
                    }

                    if (clientBytes == 0)
                    {
                        break;
                    }

                    to.Write(buffer, 0, clientBytes);
                    stream.Write(buffer, 0, clientBytes);
                }
                client.Close();
            }
        }

        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }

        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }

        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine($"Can read: {stream.CanRead}, write {stream.CanWrite}");
            Console.WriteLine($"Can timeout: {stream.CanTimeout}");
        }

        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine($"Certificate revocation list checked: {stream.CheckCertRevocationStatus}");

            X509Certificate localCertificate = stream.LocalCertificate;

            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }

            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;

            if (stream.RemoteCertificate != null)
            {
                if (remoteCertificate != null)
                {
                    Console.WriteLine(
                        $"Remote cert was issued to {remoteCertificate.Subject} and is valid from {remoteCertificate.GetEffectiveDateString()} until {remoteCertificate.GetExpirationDateString()}.");
                }
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }

        }
    }
}