using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevConsole.Client.CompareWindow
{
    public class CompareWindowClient
    {
        public readonly CompareWindowConnection connection;
        public readonly CompareWindowState state;
        public readonly CompareWindowClient control;

        private CompareWindowClient()
        {
            connection = new CompareWindowConnection();
            state = new CompareWindowState();
            control = new CompareWindowClient();
        }

        internal async static Task<CompareWindowClient> CreateAsync()
        {
            CompareWindowClient compareWindowUnit = new CompareWindowClient();
            return compareWindowUnit;
        }
    }

    public class CompareWindowConnection
    {

    }

    public class CompareWindowState
    {

    }
}
