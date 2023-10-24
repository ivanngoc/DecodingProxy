using System;

namespace IziHardGames.Libs.Cryptography.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class HandshakeAnalyzAttribute : Attribute
    {
        public ESide Side { get; set; }
    }
}
