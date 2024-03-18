using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Socks5.Headers;
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<int, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using IziHardGames.Libs.Cryptography;
using IziHardGames.NodeProxies.Nodes.Tls;

namespace IziHardGames.NodeProxies.Nodes
{

    /// <summary>
    /// анализирует первый пакет и выявляет тип протокола
    /// </summary>
    internal class NodeGate : Node, IFragsPeeker
    {
        public EGateProtocol Protocol => protocol;
        private EGateProtocol protocol;
        private IFragsShowing? source;

        public void SetSourceForShowing(IFragsShowing source)
        {
            this.source = source;
        }

        internal override async Task ExecuteAsync(CancellationToken ct)
        {
            int count = 1;
            do
            {
                var frags = await source!.ShowFragsAsync<IEnumerable<DataFragment>>(count, ct);
                if (count == 1)
                {
                    this.protocol = DetectProtocol(frags.First());
                }
                else
                {
                    this.protocol = DetectProtocol(frags);
                }
                count++;
            }
            while (protocol == EGateProtocol.TooShortToIdentify);
        }

        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Awaitable | ENodeRunFlags.NoAdvancing;
        }
        public override ETraits GetTraits()
        {
            return ETraits.Async | ETraits.FragmentPeeking;
        }

        internal static EGateProtocol DetectProtocol(IEnumerable<DataFragment> fragments)
        {
            throw new System.NotImplementedException();
        }
        internal static EGateProtocol DetectProtocol(DataFragment dataFragment)
        {
            var mem = (ReadOnlyMemory<byte>)dataFragment.buffer;
            if (mem.Length >= 5)
            {
                var result = NodeTls.DetectTls(dataFragment);
                if (result!= EGateProtocol.TLS_ERROR)
                {
                    return result;
                }
                if (result == EGateProtocol.TooShortToIdentify) return result;
                if (mem.Length > 6)
                {
                    string str = Encoding.UTF8.GetString(mem.Slice(0, 7).Span).ToUpper();
                    if (string.Equals(str, "CONNECT", StringComparison.InvariantCultureIgnoreCase)) return EGateProtocol.HTTP_CONNECT;
                    if (str.StartsWith("GET") || str.StartsWith("POST") || str.StartsWith("PUT")) return EGateProtocol.HTTP11;
                }
                ClientGreetingsSocks5 clientGreetingsSocks5 = BufferReader.ToStruct<ClientGreetingsSocks5>(mem);
                if (clientGreetingsSocks5.IsSocsk5())
                {
                    return EGateProtocol.SOCKS5;
                }
                return EGateProtocol.Unknown;
            }
            else
            {
                return EGateProtocol.TooShortToIdentify;
            }
        }
    }
}
