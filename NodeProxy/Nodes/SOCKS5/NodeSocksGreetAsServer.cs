using System;
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
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<int, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using IziHardGames.NodeProxies.Advancing;

namespace IziHardGames.NodeProxies.Nodes.SOCKS5
{
    internal class NodeSocksGreetAsServer : Node, IFragTaker, IFragProducer
    {
        private NodeSocketOrigin? origin;
        private NodeGate? nodeGate;
        private IPAddress? destinationIpAddressObtained;
        private IPAddress? destinationIpAddress;
        private ushort destinationPortObtained;
        private ushort destinationPort;

        public IFragTakable? SourceToTakeFrom { get; set; }
        internal void SetOrigin(NodeSocketOrigin origin)
        {
            this.origin = origin;
        }
        public void SetGate(NodeGate nodeGate)
        {
            this.nodeGate = nodeGate;
        }

        internal override async Task ExecuteAsync(CancellationToken ct)
        {
            var origin = this.origin!;
            var index = graph!.indexators[typeof(Indx)].As<Indx>();
            index[INDX_SOCKS_GREET] = this;

            NodeSocketWriter writer = index[INDX_AGENT_SOCKET_WRITER] as NodeSocketWriter ?? throw new NullReferenceException();

            var nodeSource = graph!.navigator.First<Node>(nodeGate!.id, (x) => x is IFragTakable); // && x.traits.HasFlag()
            var source = SourceToTakeFrom = nodeSource as IFragTakable ?? throw new NullReferenceException();
            var iziNodeThis = graph.Nodes[this.id];
            var iziSource = graph.Nodes[nodeSource.id];
            var iziWriter = graph.Nodes[writer.id];

            var frag = await source.TakeFragAsync(ct);
            frag.SetOwner(this);

            var slice = frag.ReadOnly;
            var greet = BufferReader.ToStructConsume<ClientGreetingsSocks5>(ref slice);
            var methods = BufferReader.Consume(greet.numberOfAuthMethods, ref slice);
            if (!methods.Span.Contains((byte)(EAuth.NoAuthRequired))) throw new NotImplementedException($"Only Auth:{EAuth.NoAuthRequired} implemented");

            DataFragment.Destroy(ref frag);

            ServerChoice serverChoice = new ServerChoice();
            serverChoice.version = (byte)ESocksType.SOCKS5;
            serverChoice.cauth = (byte)EAuth.NoAuthRequired; // (byte)EAuth.None;
            DataFragment fragOut = DataFragment.Get(BufferWriter.ToArray(serverChoice));
            writer.RecieveFragment(fragOut);

            frag = await source.TakeFragAsync(ct).ConfigureAwait(false);
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
            DataFragment.Destroy(ref frag);

            this.destinationIpAddressObtained = iPAddress;
            this.destinationPortObtained = destinationPort;

            this.destinationIpAddress = NodeProxyGlobals.GetDestAddressSocks5(destinationIpAddressObtained);
            this.destinationPort = NodeProxyGlobals.GetDestPortSocks5(destinationPortObtained);

            await origin.ConnectAsync(destinationIpAddress, destinationPort).ConfigureAwait(false);

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
            writer.RecieveFragment(fragOut);
        }
        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Async | ENodeRunFlags.Sustainable;
        }
        public override ETraits GetTraits()
        {
            return ETraits.FragmentCreating | ETraits.FragmentExchange | ETraits.FragmentDestroying;
        }
    }
}
