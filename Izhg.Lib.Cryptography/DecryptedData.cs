using System;
using System.Collections.Generic;
using IziHardGames.Libs.Cryptography.Infos;

namespace IziHardGames.Libs.Cryptography.Recording
{
    public class DecryptedData
    {
        public ReadOnlyMemory<byte> dataAgentToOrigin;
        public ReadOnlyMemory<byte> dataOriginToAgent;
        public TlsSession tlsSession;

        internal void AsClientFrom(List<TlsFrame> framesAgentToOrigin)
        {
            throw new NotImplementedException();
        }

        internal void AsServerFrom(List<TlsFrame> framesOriginToClient)
        {
            throw new NotImplementedException();
        }
    }
}