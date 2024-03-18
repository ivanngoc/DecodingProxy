using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.Tls;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Security;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Pipelines
{
    [Obsolete]
    public class TcpClientPipedSsl : TcpClientPiped, IPoolBind<TcpClientPipedSsl>
    {
        private SslStream sslStream;
        private PipeReader readerSsl;
        private X509Certificate2 certAsServer;
        private IPoolReturn<TcpClientPipedSsl>? pool;
        public override PipeReader Reader { get => readerSsl == null ? base.reader : readerSsl; set => throw new NotImplementedException(); }

        private void InitSsl()
        {
            SslStream sslStream = this.sslStream = new SslStream(this);
        }

        public async Task AuthAsClientAsync()
        {
            InitSsl();
            await sslStream.AuthenticateAsClientAsync(host);
            PipeReader pipeReader = this.readerSsl = PipeReader.Create(sslStream);
#if DEBUG
            Console.WriteLine($"{title}.Authenticated as Client");
#endif   
        }
        public async Task AuthAsSrverAsync(X509Certificate2 cert)
        {
            this.certAsServer = cert;
            InitSsl();
            await sslStream.AuthenticateAsServerAsync(cert);
            PipeReader pipeReader = this.readerSsl = PipeReader.Create(sslStream);
#if DEBUG
            Console.WriteLine($"{title}.Authenticated as Server");
#endif        
        }
        public new async Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken token)
        {
            if (sslStream == null)
            {
                await base.SendAsync(memory, token);
            }
            else
            {
                await sslStream.WriteAsync(memory, token);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var reader = Reader;
            // block zero read
            while (true)
            {
                if (reader.TryRead(out var result))
                {
#if DEBUG
                    var s = result.Buffer.ToStringUtf8();
                    Console.WriteLine($"Read: {title}.{s}");
#endif
                    int length = (int)result.Buffer.Length;
                    int toCopy = length > count ? count : length;
                    if (toCopy == 0) throw new NotSupportedException("For using SslStream there must be zero read block. Invent read at least?");
                    var span = new Span<byte>(buffer, offset, toCopy);
                    result.Buffer.Slice(0, toCopy).CopyTo(span);
                    ReportConsume(toCopy);
                    reader.AdvanceTo(result.Buffer.GetPosition(toCopy));
                    return toCopy;
                }
            }
#if DEBUG
            Console.WriteLine($"Readed 0");
#endif
            return 0;
        }
        public override int Read(Span<byte> buffer)
        {
            throw new System.NotImplementedException();
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        public override int ReadByte()
        {
            throw new System.NotImplementedException();
        }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var reader = Reader;
            //REPEAT:
            var result = await reader.ReadAsync(cancellationToken);
            //if (result.Buffer.Length < 32)
            //{
            //    await Task.Delay(1000);
            //    var start = result.Buffer.Start;
            //    reader.AdvanceTo(start, start);
            //    goto REPEAT;
            //}
            int length = (int)result.Buffer.Length;
            int toCopy = length > buffer.Length ? buffer.Length : length;
            if (toCopy == 0) throw new NotSupportedException("For using SslStream there must be zero read block. Invent read at least?");
            var slice = result.Buffer.Slice(0, toCopy);
            slice.CopyTo(buffer.Span);
            base.ReportConsume(toCopy);
            reader.AdvanceTo(slice.End);
            return toCopy;
        }
        public override void Close()
        {
            base.Close();
            pool!.Return(this);
            pool = default;
            readerSsl = default;
            certAsServer = default;
        }

        public X509Certificate2 GetRemoteCert()
        {
            return (sslStream.RemoteCertificate as X509Certificate2) ?? throw new NullReferenceException("Remote Cert Must be present");
        }
        public void BindToPool(IPoolReturn<TcpClientPipedSsl> poolObjects)
        {
            this.pool = poolObjects ?? throw new NullReferenceException();
        }

        public TcpClientPipedSsl UpgradeTls()
        {
            throw new NotImplementedException();
        }
    }
}