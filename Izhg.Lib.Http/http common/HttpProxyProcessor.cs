using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.Cryptography.Defaults;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.HttpCommon.Helpers;
using IziHardGames.Libs.HttpCommon.Monitoring;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Libs.Networking.Tls;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.Libs.Streams;
using IziHardGames.Proxy;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Tls;

namespace IziHardGames.Libs.HttpCommon.Common
{
    public static class AwaitIO
    {
        public static async Task IfFaulted(this Task task, Action faulted)
        {
            throw new System.NotImplementedException();
        }
        public static async Task AwaitNetwork(Task task, Action faulted, Action continuation)
        {
            try
            {
                await task.ConfigureAwait(false);
                continuation();
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException)
                {
                    faulted();
                }
            }
        }
    }

    public class HttpProxyProcessor
    {
        private static uint counter;
        private const int timeOutAgent = 1000;
        private const string dir = @"C:\Builds\DecodingProxy\Records";
        public static async Task HandleSocket(HttpConsumer consumer, Socket socketClient, CancellationToken ct = default)
        {
            Guid guid = Guid.NewGuid();
            Stream currentStreamClient = default;
            Stream currentStreamOrigin = default;
            SocketStream socketStreamClient = PoolObjectsConcurent<SocketStream>.Shared.Rent();
            socketStreamClient.Initilize(socketClient);
            currentStreamClient = socketStreamClient;
            StreamDemultiplexer demuxClient = PoolObjectsConcurent<StreamDemultiplexer>.Shared.Rent();
            demuxClient.Initilize(socketStreamClient);
            currentStreamClient = demuxClient;
            //DEBUG
            StreamForRecording recordAgent = PoolObjectsConcurent<StreamForRecording>.Shared.Rent();
            recordAgent.Initilize($"{guid}", dir, demuxClient);
            currentStreamClient = demuxClient;
            var keyReaderRecordClient = demuxClient.RegistReader(recordAgent.actionRecordReader);
            var keyWriterRecordClient = demuxClient.RegistWriter(recordAgent.actionRecordWriter);

            counter++;
            uint idConnection = counter;
            HttpEventCenter.OnNewConnection(idConnection);
            HttpEventCenter.OnAddState(idConnection, EHttpConnectionStates.ClientConnected);

            HttpSource dataSource = new HttpSource($"Connection:{counter}");
            Console.WriteLine($"Begin Handle Socket counter:[{counter}]");
            byte[] rawReadBuffer = ArrayPool<byte>.Shared.Rent((1 << 20) * 32);
            int size = await ReaderHttpBlind.AwaitHeadersWithEmptyBody(rawReadBuffer, demuxClient).ConfigureAwait(false);
            Console.WriteLine("Awaited First HTTP request");

            var memWithMsg = new ReadOnlyMemory<byte>(rawReadBuffer, 0, size);
            Console.WriteLine(Encoding.UTF8.GetString(memWithMsg.Span));
            Stream streamClientCurrent = default;
            var certManager = CertManager.GetOrCreateShared();

            if (ReaderHttpBlind.TryReadStartLine(in memWithMsg, out StartLineReadResult startLine))
            {
                var flags = startLine.flags;
                var host = startLine.Host;
                var port = startLine.port;
                if (!flags.HasFlag(EStartLine.PortPresented))
                {
                    if (flags.HasFlag(EStartLine.HttpsPresented)) port = 443;
                    else if (flags.HasFlag(EStartLine.HttpPresented)) port = 80;
                    else throw new FormatException("Impossible?");
                }
                if (ReaderHttpBlind.TryFindHostFeild(in memWithMsg, out ReadOnlyMemory<byte> hostFromField))
                {
                    var url = HttpHelper.DisassembleHost(in hostFromField);
                    if (url.isPortPresented)
                    {
                        port = url.Port;
                    }
                    host = url.Host;
                }
                dataSource.host = host;
                dataSource.port = port;

                HttpEventCenter.OnFindHostAndPort(idConnection, host, port);

                var adresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
                Socket socketOrigin = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(adresses.First(x => x.AddressFamily == AddressFamily.InterNetwork), port);
                Console.WriteLine($"Begin connection to Origin host:{host}. port:{port}");
                await socketOrigin.ConnectAsync(endPoint).ConfigureAwait(false);
                HttpEventCenter.OnAddState(idConnection, EHttpConnectionStates.OriginConnected);
                Console.WriteLine($"Connected to Origin host:{host}. port:{port}");

                SocketStream socketStreamOrigin = PoolObjectsConcurent<SocketStream>.Shared.Rent();
                socketStreamOrigin.Initilize(socketOrigin);
                currentStreamOrigin = socketStreamOrigin;
                StreamDemultiplexer demuxOrigin = PoolObjectsConcurent<StreamDemultiplexer>.Shared.Rent();
                demuxOrigin.Initilize(socketStreamOrigin);
                currentStreamOrigin = demuxOrigin;
                //DEBUG
                StreamForRecording recordOrigin = PoolObjectsConcurent<StreamForRecording>.Shared.Rent();
                recordOrigin.Initilize($"{guid}", dir, demuxOrigin);
                var keyReaderRecordOrigin = demuxOrigin.RegistReader(recordOrigin.actionRecordReader);
                var keyWriterRecordOrigin = demuxOrigin.RegistWriter(recordOrigin.actionRecordWriter);
                currentStreamOrigin = demuxOrigin;



                if (flags.HasFlag(EStartLine.MethodConnect))
                {
                    await currentStreamClient.WriteAsync(ConstantsForHttp.Responses.bytesOk11, ct).ConfigureAwait(false);
                    Console.WriteLine($"OK200 Sended");

                    TlsHandshakeReadOperation tlsReaderClient = PoolObjectsConcurent<TlsHandshakeReadOperation>.Shared.Rent();
                    tlsReaderClient.Initilize();
                    var keyReaderHandshakeClient = demuxClient.RegistReader(tlsReaderClient.actionAddDataWithCheck);

                    StreamBufferedForReads streamBufferedClient = PoolObjectsConcurent<StreamBufferedForReads>.Shared.Rent();
                    streamBufferedClient.Initilize(demuxClient);
                    var t1 = streamBufferedClient.StartFillingBuffer();

                    SslStream sslStreamClient = new SslStream(streamBufferedClient);
                    Console.WriteLine($"Begin await handshake From Client");
                    await tlsReaderClient.AwaitHandShake().ConfigureAwait(false);
                    Console.WriteLine($"Complete await handshake From Client");
                    var clientHandshake = tlsReaderClient.AnalyzAsClient();
                    Console.WriteLine($"Client handshake analyzed");
                    await streamBufferedClient.StopFillingBuffer();

                    var clientProtocolsSsl = clientHandshake.protocolsSsl;
                    var clientProtocols = clientHandshake.protocols;

                    var alpnList = new List<SslApplicationProtocol>();
                    if (clientProtocols.HasFlag(ENetworkProtocols.HTTP3))
                    {
                        alpnList.Add(SslApplicationProtocol.Http3);
                    }
                    if (clientProtocols.HasFlag(ENetworkProtocols.HTTP2))
                    {
                        alpnList.Add(SslApplicationProtocol.Http2);
                    }
                    if (clientProtocols.HasFlag(ENetworkProtocols.HTTP11))
                    {
                        alpnList.Add(SslApplicationProtocol.Http11);
                    }
                    SslClientAuthenticationOptions optionsClient = SslOptionsFactory.CreateOptionsForClient(host, clientProtocolsSsl, alpnList);
                    ProtocolType clientProtocol = ProtocolType.Tcp;

                    if (clientProtocol == ProtocolType.Tcp)
                    {
                        TlsHandshakeReadOperation tlsHandshakeReaderOrigin = PoolObjectsConcurent<TlsHandshakeReadOperation>.Shared.Rent();
                        tlsHandshakeReaderOrigin.Initilize();

                        var keyReaderHandshakeOrigin = demuxOrigin.RegistReader(tlsHandshakeReaderOrigin.actionAddDataWithCheck);

                        var sslStreamOrigin = new SslStream(demuxOrigin);
                        Console.WriteLine($"Begin Authentication to Origin as Client");
                        try
                        {
                            await sslStreamOrigin.AuthenticateAsClientAsync(optionsClient).ConfigureAwait(false);
                        }
                        catch (SocketException ex)
                        {
                            throw;
                        }
                        HttpEventCenter.OnAddState(idConnection, EHttpConnectionStates.OriginAuthenticated);
                        Console.WriteLine($"Begin await handshake From Server");
                        await tlsHandshakeReaderOrigin.AwaitHandShake().ConfigureAwait(false);
                        Console.WriteLine($"Complete await handshake From Server");
                        var handshakeOrigin = tlsHandshakeReaderOrigin.AnalyzAsServer();

                        var accepted = sslStreamOrigin.NegotiatedApplicationProtocol;
                        Console.WriteLine($"Complete Authentication to Origin as Client. Accepted protocol:{accepted}");

                        var appProtocols = new List<SslApplicationProtocol>(3);
                        X509Certificate2 caCert = CertManager.SharedCa;
                        X509Certificate2 certOrigin = (X509Certificate2)sslStreamOrigin.RemoteCertificate!;
                        certManager.Test(certOrigin);

                        SslServerAuthenticationOptions optionsSever = await SslOptionsFactory.CreateOptionsForServer(certManager, clientProtocolsSsl, appProtocols, caCert, certOrigin).ConfigureAwait(false);

                        if (accepted == SslApplicationProtocol.Http3)
                        {
                            Console.WriteLine($"Origin Accepted HTTP3");
                            appProtocols.Add(SslApplicationProtocol.Http3);
                        }
                        else if (accepted == SslApplicationProtocol.Http2)
                        {
                            Console.WriteLine($"Origin Accepted HTTP2");
                            appProtocols.Add(SslApplicationProtocol.Http2);
                        }
                        else if (accepted == SslApplicationProtocol.Http11)
                        {
                            Console.WriteLine($"Origin Accepted HTTP1");
                            appProtocols.Add(SslApplicationProtocol.Http11);
                        }
                        else
                        {
                            Console.WriteLine($"Origin Accepted PROTOCOLS_EMPTY");
                            appProtocols.Add(SslApplicationProtocol.Http11);
                        }

                        Console.WriteLine($"Begin sslStramClient Authentication As Server");
                        await sslStreamClient.AuthenticateAsServerAsync(optionsSever, ct).ConfigureAwait(false);

                        Console.WriteLine($"Complete sslStramClient Authenticated As Server. Protocols: {sslStreamClient.NegotiatedApplicationProtocol}");

                        if (accepted == SslApplicationProtocol.Http3)
                        {
                            throw new System.NotImplementedException();
                        }
                        else if (accepted == SslApplicationProtocol.Http2)
                        {
                            dataSource.flagsAgent = EHttpConnectionFlags.HTTP20 | EHttpConnectionFlags.AuthenticatedHttp20;
                            dataSource.flagsOrigin = EHttpConnectionFlags.HTTP20 | EHttpConnectionFlags.AuthenticatedHttp20;

                            Console.WriteLine($"Begin H2 Process");
                            StreamDemultiplexer demuxClientTls = PoolObjectsConcurent<StreamDemultiplexer>.Shared.Rent();
                            StreamDemultiplexer demuxOriginTls = PoolObjectsConcurent<StreamDemultiplexer>.Shared.Rent();
                            demuxClientTls.Initilize(sslStreamClient);
                            demuxOriginTls.Initilize(sslStreamOrigin);
                            var keyReaderRequestHttp20 = demuxClientTls.RegistReader((x) => { consumer.PushRequestHttp20(dataSource, x); return ct.IsCancellationRequested; });
                            var keyReaderResponseHttp20 = demuxOriginTls.RegistReader((x) => { consumer.PushResponseHttp20(dataSource, x); return ct.IsCancellationRequested; });

                            var t3 = HelperForStreams.CopyStreamToStream(demuxClientTls, demuxOriginTls);
                            var t4 = HelperForStreams.CopyStreamToStream(demuxOriginTls, demuxClientTls);
                            Console.WriteLine($"Begin Awaiting Stream to Stream");
                            var results = await Awaiting.WhenAll(t3, t4).ConfigureAwait(false);
                            Console.WriteLine($"Complete Awaiting Stream to Stream");
                            demuxOriginTls.Dispose();
                            demuxClientTls.Dispose();
                            PoolObjectsConcurent<StreamDemultiplexer>.Shared.Return(demuxOriginTls);
                            PoolObjectsConcurent<StreamDemultiplexer>.Shared.Return(demuxClientTls);
                        }
                        else if (accepted == SslApplicationProtocol.Http11)
                        {
                            dataSource.flagsAgent = EHttpConnectionFlags.HTTP11 | EHttpConnectionFlags.AuthenticatedHttp11;
                            dataSource.flagsOrigin = EHttpConnectionFlags.HTTP11 | EHttpConnectionFlags.AuthenticatedHttp11;

                            Console.WriteLine($"Begin HTTP/1.1 Process");

                            StreamDemultiplexer demuxClientTls = PoolObjectsConcurent<StreamDemultiplexer>.Shared.Rent();
                            StreamDemultiplexer demuxOriginTls = PoolObjectsConcurent<StreamDemultiplexer>.Shared.Rent();

                            demuxClientTls.Initilize(sslStreamClient);
                            demuxOriginTls.Initilize(sslStreamOrigin);

                            var keyReaderRequestHttp11 = demuxClientTls.RegistReader((x) => { consumer.PushRequestHttp11(dataSource, x); return ct.IsCancellationRequested; });
                            var keyReaderResponseHttp11 = demuxOriginTls.RegistReader((x) => { consumer.PushResponseHttp11(dataSource, x); return ct.IsCancellationRequested; });
                            // no connection control?
                            var t3 = HelperForStreams.CopyStreamToStream(demuxClientTls, demuxOriginTls);
                            var t4 = HelperForStreams.CopyStreamToStream(demuxOriginTls, demuxClientTls);

                            var results = await Awaiting.WhenAll(t3, t4).ConfigureAwait(false);

                            demuxOriginTls.Dispose();
                            demuxClientTls.Dispose();
                            PoolObjectsConcurent<StreamDemultiplexer>.Shared.Return(demuxOriginTls);
                            PoolObjectsConcurent<StreamDemultiplexer>.Shared.Return(demuxClientTls);
                        }
                        else
                        {
                            throw new System.NotSupportedException();
                        }

                        throw new System.NotImplementedException();
                        PoolObjectsConcurent<TlsHandshakeReadOperation>.Shared.Return(tlsHandshakeReaderOrigin);
                        //PoolObjectsConcurent<StreamBuffered>.Shared.Return(streamBufferedOrigin);
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                    tlsReaderClient.Dispose();
                    sslStreamClient.Dispose();
                    PoolObjectsConcurent<StreamBufferedForReads>.Shared.Return(streamBufferedClient);
                    PoolObjectsConcurent<TlsHandshakeReadOperation>.Shared.Return(tlsReaderClient);
                }
                else if (flags.HasFlag(EStartLine.MethodGet))
                {
                    dataSource.flagsAgent = EHttpConnectionFlags.HTTP11;
                    dataSource.flagsOrigin = EHttpConnectionFlags.HTTP11;

                    socketClient.ReceiveTimeout = timeOutAgent;
                    await socketOrigin.SendAsync(memWithMsg, SocketFlags.None, ct).ConfigureAwait(false);
                    CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    ConnectionControllerHttp11 controller = PoolObjectsConcurent<ConnectionControllerHttp11>.Shared.Rent();
                    controller.Initlize(socketClient, socketOrigin);
                    controller.SetCancelation(cts);
                    ct = cts.Token;
                    consumer.OnCloseIndicatedEvent += controller.CloseClient;

                    var keyReaderRequestHttp11 = demuxClient.RegistReader((x) => { consumer.PushRequestHttp11(dataSource, x); return ct.IsCancellationRequested; });
                    var keyReaderResponseHttp11 = demuxOrigin.RegistReader((x) => { consumer.PushResponseHttp11(dataSource, x); return ct.IsCancellationRequested; });

                    var t3 = HelperForStreams.CopyStreamToStream(demuxClient, demuxOrigin);
                    var t4 = HelperForStreams.CopyStreamToStream(demuxOrigin, demuxClient);

                    var results = await Awaiting.WhenAll(t3, t4).ConfigureAwait(false);

                    controller.Dispose();
                    PoolObjectsConcurent<ConnectionControllerHttp11>.Shared.Return(controller);
                }
                else
                {
                    throw new System.NotImplementedException();
                }
                demuxOrigin.Dispose();
                recordAgent.Dispose();
                recordOrigin.Dispose();
                PoolObjectsConcurent<SocketStream>.Shared.Return(socketStreamOrigin);
                PoolObjectsConcurent<StreamDemultiplexer>.Shared.Return(demuxOrigin);
                PoolObjectsConcurent<StreamForRecording>.Shared.Return(recordOrigin);
            }
            demuxClient.Dispose();
            PoolObjectsConcurent<SocketStream>.Shared.Return(socketStreamClient);
            PoolObjectsConcurent<StreamDemultiplexer>.Shared.Return(demuxClient);
            PoolObjectsConcurent<StreamForRecording>.Shared.Return(recordAgent);
        }



    }
}
