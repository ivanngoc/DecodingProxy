using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Proxy;

namespace IziHardGames.Libs.Proxy.Pairing
{
    public class ConnectionHttp : ConnectionBase
    {
        public EHttpConnectionFlags Flags => flags;
        protected EHttpConnectionFlags flags;
    }

    public class ConnectionToOrigin : ConnectionHttp
    {

    }

    public class ConnectionToAgent : ConnectionHttp
    {

    }

    public class ConnectionPair
    {
        public ConnectionToAgent agent;
        public ConnectionToOrigin origin;
    }
}
