using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Binary.Writers;
using IziHardGames.Socks5.Enums;
using IziHardGames.Socks5.Headers;

namespace IziHardGames.NodeProxies.Nodes.SOCKS5
{
    internal class NodeSocks5AsServer : BidirectionalNode
    {
        private Func<Node, Task>? callback;
        private Func<Node, CancellationToken, Task<BidirectionalNode>>? requestNextNode;
        private BidirectionalNode? origin;

        public event Action<Node>? OnDestinationObtained;
        private IPAddress? destinationIpAddressObtained;
        private ushort destinationPortObtained;

        public IPAddress DestinationIpAddressObtained => destinationIpAddressObtained ?? throw new NullReferenceException();
        public ushort DestinationPortObtained => destinationPortObtained;

        private IPAddress? destinationIpAddress;
        private ushort destinationPort;
        private Task? completion;

        public event Action<DataFragment>? OnDataOutEvent;
        public event Action<DataFragment>? OnDataInEvent;

        public void SetNextNodeRequester(Func<Node, CancellationToken, Task<BidirectionalNode>>? requestNext)
        {
            this.requestNextNode = requestNext;
        }
        public void SetCallback(Func<Node, Task>? callback)
        {
            this.callback = callback;
        }
        internal void InsertFrag(DataFragment frag)
        {
            lock (fragmentsToInsertIn)
            {
                fragmentsToInsertIn.Enqueue(frag);
            }
        }
        internal void SetAwaitingCompletionTask(Task task)
        {
            this.completion = task;
        }

        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Sustainable | ENodeRunFlags.Async;
        }
        internal override async Task ExecuteAsync(CancellationToken ct)
        {
            var t1 = CollectFrames(ct);
            var frag = await TakeFrameFromIn(ct).ConfigureAwait(false);
            var slice = frag.ReadOnly;
            var greet = BufferReader.ToStructConsume<ClientGreetingsSocks5>(ref slice);
            var methods = BufferReader.Consume(greet.numberOfAuthMethods, ref slice);
            if (!methods.Span.Contains((byte)(EAuth.NoAuthRequired))) throw new NotImplementedException($"Only Auth:{EAuth.NoAuthRequired} implemented");


            ServerChoice serverChoice = new ServerChoice();
            serverChoice.version = (byte)ESocksType.SOCKS5;
            serverChoice.cauth = (byte)EAuth.NoAuthRequired; // (byte)EAuth.None;
            DataFragment fragOut = DataFragment.Get(BufferWriter.ToArray(serverChoice));
            agentIn!.RecieveFragment(fragOut);

            frag = await TakeFrameFromIn(ct).ConfigureAwait(false);
            slice = frag.ReadOnly;

            var ccr = BufferReader.ToStructConsume<ClientRequest>(ref slice);
            var adr = ccr.atyp;

            IPAddress iPAddress = default;
            switch (adr.Type)
            {
                case EAdrType.None: throw new FormatException();
                case EAdrType.IPv4:
                    {
                        iPAddress = BufferReader.ConsumeIPv4AsIPAddress(ref slice);
                        break;
                    }
                case EAdrType.DomainName:
                    {
                        byte lengthDomainName = BufferReader.ConsumeByte(ref slice);
                        var domainName = BufferReader.Consume(lengthDomainName, ref slice);
                        var ips = Dns.GetHostAddresses(Encoding.UTF8.GetString(domainName.Span));
                        iPAddress = ips.First(x => x.AddressFamily == AddressFamily.InterNetwork);
                        if (iPAddress is null) throw new NotImplementedException();
                        break;
                    }
                case EAdrType.IPv6:
                    {
                        iPAddress = BufferReader.ConsumeIPv6AsIPAddress(ref slice);
                        break;
                    }
                default: throw new System.NotImplementedException(adr.Type.ToString());
            }
            ushort destinationPort = BufferReader.ToUshortConsume(ref slice);
            this.destinationIpAddressObtained = iPAddress;
            this.destinationPortObtained = destinationPort;

            this.destinationIpAddress = destinationIpAddressObtained;
            this.destinationPort = destinationPortObtained;

            OnDestinationObtained?.Invoke(this);
            // check if need to override destination
            await callback!(this).ConfigureAwait(false);
            var origin = this.origin = await requestNextNode!(this, ct).ConfigureAwait(false);
            this.SetNext(origin);

            // запускаем origin.ExecuteAsync()
            var t2 = RunAsync(origin);

            // server reply length+ port length
            int lengthReply = 4 + 2;
            ServerReply resp = new ServerReply();
            resp.VER = ESocksType.SOCKS5;
            resp.Reply = EReply.RequestGranted;
            if (destinationIpAddress!.AddressFamily == AddressFamily.InterNetwork)
            {
                resp.atyp.Type = EAdrType.IPv4;
                lengthReply += 4;
            }
            else
            {
                resp.atyp.Type = EAdrType.IPv6;
                lengthReply += 16;
            }
            byte[] bytes = new byte[lengthReply];
            int offset = 0;
            offset += BufferWriter.WirteToBuffer(resp, bytes, offset);
            var adrBytes = destinationIpAddress!.GetAddressBytes();
            offset += BufferWriter.WirteToBuffer(adrBytes, bytes, offset);
            offset += BufferWriter.WirteToBufferUshort(destinationPort, bytes, offset);
            fragOut = DataFragment.Get(bytes);
            agentIn.RecieveFragment(fragOut);

            как внедриться в контейнер?

            var t3 = RunThisToOrigin(ct);
            var t4 = RunOriginToThis(ct);

            await completion!.ConfigureAwait(false);
        }

        private Task RunOriginToThis(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    var frag = await origin!.TakeFragAsync(ct).ConfigureAwait(false);
                    OnDataInEvent?.Invoke(frag);
                    agentIn!.RecieveFragment(frag);
                }
            });
        }
        private Task RunThisToOrigin(CancellationToken ct)
        {
            return Task.Run(async () =>
               {
                   while (!ct.IsCancellationRequested)
                   {
                       var frag = await TakeFrameFromIn(ct).ConfigureAwait(false);
                       OnDataOutEvent?.Invoke(frag);
                       origin!.RecieveFragment(frag);
                   }
               });
        }

        internal void SetSession(SessionControl sessionControl)
        {
            throw new NotImplementedException();
        }
    }
}
