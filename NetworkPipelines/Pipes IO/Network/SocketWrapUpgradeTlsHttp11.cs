using System;
using System.IO;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Lib.Networking.Exceptions;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.DevTools;
using IziHardGames.Libs.Networking.Pipelines.Contracts;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SocketWrapUpgradeTlsHttp11 : SocketWrapUpgradeTlsHttp, ISslEndpoint, IPoolBind<SocketWrapUpgradeTlsHttp11>, IReader
    {
        private IPoolReturn<SocketWrapUpgradeTlsHttp11> pool;
        private PipeReader readerSsl;
        private SslStream streamSsl;
        private Stream stream;
        private PipeReader Reader { get; set; }
        private PipeReader readerRaw;
        private X509Certificate2 remoteCert;
        private X509Certificate2 remoteCertForged;
        private X509Certificate2 caRoot;
        private readonly StreamPipeReaderOptions options;
        private static readonly StalledConnection exStalledConnection = new StalledConnection();
        public string Host => wrap.ConnectionData.Host;
        public int Port => wrap.ConnectionData.Port;


        public SocketWrapUpgradeTlsHttp11() : base()
        {
            options = new StreamPipeReaderOptions(default, 4096, 1024, true);
        }

        public async Task AuthAsServerAsync(X509Certificate2 certForged, X509Certificate2 certOrigin, X509Certificate2 caRootCert)
        {
            this.remoteCertForged = certForged;
            this.remoteCert = certOrigin;
            this.caRoot = caRootCert;
            UpgradeToSsl();
            try
            {
                await streamSsl.AuthenticateAsServerAsync(certForged).ConfigureAwait(false);
                wrap.EnableFlags(States.EConnectionFlags.AuthenticatedSslServer);
            }
            catch (System.IO.IOException ex)
            {
                // 'Unable to read data from the transport connection: An established connection was aborted by the software in your host machine..'
                // need to kill connection from browser
                throw SuddenBreakException.shared;
            }
            catch (AuthenticationException)
            {
                throw new System.NotImplementedException();
            }
            streamSsl.WriteTimeout = 30000;
            streamSsl.ReadTimeout = 15000;
            Logger.ReportTime($"Auth as Server. guid:{wrap.Guid}.");
        }
        public async Task AuthAsClientAsync()
        {
            UpgradeToSsl();
            try
            {
                await streamSsl.AuthenticateAsClientAsync(Host);
                wrap.EnableFlags(States.EConnectionFlags.AuthenticatedSslClient);
            }
            catch (IOException ex)
            {   // если не удалось установить ssl соединение с удаленным сервером то его нельзя использовать в пуле для домена.
                throw SuddenBreakException.shared;
            }
            catch (AuthenticationException ex)
            {
                throw ex;
            }
            remoteCert = (streamSsl.RemoteCertificate as X509Certificate2) ?? throw new NullReferenceException($"Remote Cert Is not Founded");
#if DEBUG
            Console.WriteLine($"[{wrap.Title}] authenticated as Client");
#endif
            Logger.ReportTime($"Auth as Client. guid:{wrap.Guid}.");
        }

        private void UpgradeToSsl()
        {
            streamSsl = new SslStream(stream);
            readerSsl = PipeReader.Create(streamSsl, options);
            readerRaw.Complete();
            readerRaw = default;
            Reader = readerSsl;
            wrap.EnableFlags(EConnectionFlags.Ssl);
        }

        public void BindToPool(IPoolReturn<SocketWrapUpgradeTlsHttp11> pool)
        {
            this.pool = pool;
        }
        public void AdjustConnection()
        {
            CreateDefaultPipe();
        }

        public void CreateDefaultPipe()
        {
            stream = new NetworkStream(wrap.Socket);
#if DEBUG
            stream = new DebugStream(stream, ELogType.HexFormat);
#endif
            readerRaw = PipeReader.Create(stream, options);
            Reader = readerRaw;
        }

        public async ValueTask<ReadResult> ReadPipeAsync(CancellationToken token = default)
        {
            int zeroReads = default;
            try
            {
                while (zeroReads < 50)
                {
                    var result = await Reader.ReadAsync(token);
                    if (result.Buffer.Length > 0)
                    {
                        zeroReads = default;
                        return result;
                    }
                    zeroReads++;
                    await Task.Delay(100).ConfigureAwait(false);
                }
                throw exStalledConnection;
            }
            catch (TimeoutException)
            {
                throw exStalledConnection;
            }
        }
        public async Task SendAsync(string str, CancellationToken token)
        {
            try
            {
                await streamSsl.WriteAsync(Encoding.UTF8.GetBytes(str)).ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                throw SuddenBreakException.shared;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task SendAsync(ReadOnlyMemory<byte> mem, CancellationToken token)
        {
            try
            {
                await streamSsl.WriteAsync(mem).ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                throw SuddenBreakException.shared;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ReportConsume(SequencePosition position)
        {
            Reader.AdvanceTo(position);
        }
        public override void Dispose()
        {
            pool.Return(this);
            pool = default;
            readerSsl.Complete();
            readerSsl = default;
            Reader = default;
            readerRaw = default;
        }

        public X509Certificate2 GetRemoteCert()
        {
            return remoteCert ?? throw new NullReferenceException("Cert is Null");
        }

        public override void ApplyTo(SocketWrap socketWrap)
        {
            throw new NotImplementedException();
        }

        public static implicit operator SocketWrap(SocketWrapUpgradeTlsHttp11 upgrade) => upgrade.wrap;
    }
}