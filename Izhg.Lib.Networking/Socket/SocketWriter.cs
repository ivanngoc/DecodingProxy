using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.Contracts;

namespace IziHardGames.Libs.Networking.SocketLevel
{

    public class SocketWriter : SocketProcessor
    {
        protected SocketWriter? source;
        protected SocketWriter? destination;
        protected readonly List<ISocketWriterBind> binds = new List<ISocketWriterBind>();

        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            if (this is SocketWriterRaw) return;
            this.source = wrap.Writer;
            this.source.destination = this;
        }

        public virtual int Write(byte[] array, int offset, int length)
        {
            throw new System.NotImplementedException();
        }
        public virtual async Task WriteAsync(byte[] array, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }
        public virtual async Task WriteAsync(ReadOnlyMemory<byte> readOnlyMemory, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            foreach (var item in binds)
            {
                item.SetWrtier(source!);
            }
            binds.Clear();
            base.Dispose();
            source!.destination = default;
            source = default;
        }

        public void Bind(ISocketWriterBind bind)
        {
            binds.Add(bind);
            bind.SetWrtier(this);
        }
    }
}