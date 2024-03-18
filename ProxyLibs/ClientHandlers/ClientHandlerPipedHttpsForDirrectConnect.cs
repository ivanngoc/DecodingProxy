using System;
using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Core;
using IziHardGames.Libs.Cryptography;
using IziHardGames.Libs.Cryptography.Tls;
using IziHardGames.Libs.HttpCommon.Piped;
using IziHardGames.Libs.IO;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.Libs.ObjectsManagment;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Tcp.Tls;
using IziHardGames.Tls;
using Enumerator = IziHardGames.Libs.Cryptography.Tls.Shared.TlsHelloFromClientExtensionsEnumerator;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class ClientHandlerPipedHttpsForDirrectConnect : IClientHandlerAsync<SocketWrap>
    {
        private ConsumingProvider consumingProvider;
        private X509Certificate2 caRootCert;
        private CertManager certManager;
        private ManagerBase<string, ConnectionsToDomainTls<SocketWrap>, (string, int)> managerSsl;
        private HttpSource dataSource;
        private readonly IChangeNotifier<IConnectionData> monitor;


        //public ClientHandlerPipedHttpsForDirrectConnect(ConsumingProvider consumingProvider, Core.IChangeNotifier<IConnectionData> monitorForConnections, Libs.ObjectsManagment.ManagerBase<string, ConnectionsToDomainSsl<TcpWrapPiped>, (string, int)> managerSsl, System.Security.Cryptography.X509Certificates.X509Certificate2 caRootCert, Tls.CertManager certManager)
        //{
        //    this.consumingProvider = consumingProvider;
        //    this.monitor = monitorForConnections;
        //    this.caRootCert = caRootCert;
        //    this.certManager = certManager;
        //    //this.managerSsl = managerSsl;
        //    this.dataSource = new DataSource($"{nameof(ClientHandlerPipedHttpsV2)}");
        //}

        public async Task<SocketWrap> HandleClientAsync(SocketWrap wrap, CancellationToken token = default)
        {
            wrap.AddModifier(PoolObjectsConcurent<SocketModifierReaderPiped>.Shared);
            wrap.AddModifier(PoolObjectsConcurent<SocketModifierWriterDefault>.Shared);

            var reader = wrap.Reader as SocketReaderPiped;
            var writer = wrap.Writer as SocketWriterDefault;

            var t1 = reader.RunWriter(token);
            var data = await DetectIncomeType(reader, token).ConfigureAwait(false);
            var type = data.negotioanions;

            switch (type)
            {
                case ENegotioanions.None: break;
                case ENegotioanions.Connect:
                    {
                        await writer!.WriteAsync(ConstantsForHttp.Responses.bytesOk11).ConfigureAwait(false);
                        var httpVersion = await HandShakeAnalyz(reader).ConfigureAwait(false); // highest support version
                        var hub = managerSsl.GetOrCreate($"{data.connectionKey}", (data.Host, data.Port));
                        var t2 = hub.GetOrCreateAsync(BuildFilter(data, httpVersion), "Client", PoolObjectsConcurent<SocketWrap>.Shared);
                        break;
                    }
                case ENegotioanions.Direct:
                    break;
                case ENegotioanions.Handshake:
                    break;
                default:
                    break;
            }

            throw new System.NotImplementedException();
        }
        private static EConnectionFlags BuildFilter(ConnectionDataPoolable data, EHttpVersion httpVersion)
        {
            EConnectionFlags result = EConnectionFlags.None;
            if (httpVersion == EHttpVersion.Version30)
            {
                result |= EConnectionFlags.HTTP3 | EConnectionFlags.TLS13 | EConnectionFlags.Ssl;
            }
            else
            if (httpVersion == EHttpVersion.Version20)
            {
                result |= EConnectionFlags.HTTP2 | EConnectionFlags.TLS12 | EConnectionFlags.Ssl;
            }
            else
            if (httpVersion == EHttpVersion.Version11)
            {
                result |= EConnectionFlags.HTTP11 | EConnectionFlags.TLS12 | EConnectionFlags.Ssl;
            }
            return result;
        }
        private static async Task<ConnectionDataPoolable> DetectIncomeType(SocketReaderPiped reader, CancellationToken token)
        {
            var connect = "CONNECT ";
            var pool = PoolObjectsConcurent<ConnectionDataPoolable>.Shared;
            ConnectionDataPoolable data = pool.Rent();
            data.BindToPool(pool);

            while (!token.IsCancellationRequested)
            {
                var result = await reader.ReadPipeAsync();

                Console.WriteLine(Encoding.UTF8.GetString(result.Buffer));
                var buffer = result.Buffer;

                if (connect.Length <= result.Buffer.Length)
                {
                    if (buffer.IsStartWithSingleSize(connect))
                    {
                        var pos = result.Buffer.FindPosAfterEndOfHeaders();
                        reader.AdvanceTo(pos);
                        data.Version += ENegotioanions.Connect;
                        return data;
                    }
                }
                reader.AdvanceTo(result.Buffer.Start);
            }
            data.Version = $"ENegotioanions.None";
            return data;
        }
        private static async Task<EHttpVersion> HandShakeAnalyz(SocketReaderPiped reader, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                var result = await reader.ReadPipeAsync();

                if (TryAnalyzTlsHello(result.Buffer, out var httpVersion))
                {   // only peek data
                    reader.AdvanceTo(result.Buffer.Start);
                    return httpVersion;
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            throw new System.NotImplementedException();
        }
        private static bool TryAnalyzTlsHello(ReadOnlySequence<byte> buffer, out EHttpVersion httpVersion)
        {
            Enumerator num = new Enumerator(buffer);
            while (num.MoveNext())
            {
                var extension = num.Current;
                Console.WriteLine($"DEBUG: {(ETlsExtensions)extension.type}. Length:{extension.length}. DataAsString: {Encoding.UTF8.GetString(extension.data)}. Data Raw:{ParseByte.ToHexStringFormated(extension.data)}");

                if (extension.type == (ushort)(ETlsExtensions.APPLICATION_LAYER_PROTOCOL_NEGOTIATION))
                {
                    //h3 - HTTP/3 0x68 0x33
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.h3))
                    {
                        httpVersion = EHttpVersion.Version30;
                        return true;
                    }
                    // https://www.rfc-editor.org/rfc/rfc7301.html#section-6
                    // 0x00 0x0C 0x02
                    // 0x68 0x32 - "h2". The string is serialized into an ALPN protocol identifier as the two-octet sequence: 0x68, 0x32. https://httpwg.org/specs/rfc9113.html#versioning
                    // 0x08 - горизонтальная табуляция
                    // 0x68 0x74 0x74 0x70 0x2f 0x31 0x2e 0x31 = "http/1.1"
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.h2))
                    {
                        httpVersion = EHttpVersion.Version20;
                        return true;
                    }
                    if (extension.data.ContainSequence(ConstantsForTls.ALPN.http11))
                    {
                        httpVersion = EHttpVersion.Version11;
                        return true;
                    }
                    throw new System.NotImplementedException();
                }
            }
            throw new System.NotImplementedException();
        }
    }
}