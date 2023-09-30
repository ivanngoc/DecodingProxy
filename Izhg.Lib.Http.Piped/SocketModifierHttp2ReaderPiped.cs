using System;
using IziHardGames.Libs.ForHttp.Http11;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.Pipelines.Contracts;

namespace IziHardGames.Libs.ForHttp.Http20
{
    public class SocketModifierHttp2ReaderPiped : SocketWrapModifier
    {
        private readonly ObjectReaderHttp20Piped reader = new ObjectReaderHttp20Piped();
        public ObjectReaderHttp20Piped Reader => reader;

        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            if (wrap.Reader is IGetPipeReader reader)
            {
                this.reader.Bind(reader);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public override void InitilizeReverse()
        {
            base.InitilizeReverse();
            this.reader.Dispose();
        }
    }
}
