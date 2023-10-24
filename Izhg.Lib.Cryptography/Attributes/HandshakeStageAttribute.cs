using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Cryptography.Attributes
{
    internal class HandshakeStageAttribute : Attribute
    {
        public int Numeric { get; set; }
        public EHandshakeStage Stage { get; set; }
        public ESide SideAccepting { get; set; }
    }
}
