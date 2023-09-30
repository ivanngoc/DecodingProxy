using System;
using System.Threading;

namespace IziHardGames.Libs.ForHttp.Common
{
    public class ConnectionControllerHttp11 : ConnectionControllerHttp
    {
        private CancellationTokenSource cts;

        public void CloseClient()
        {
            socketClient!.Close();
            cts!.Cancel();
        }

        internal void SetCancelation(CancellationTokenSource cts)
        {
            this.cts = cts;
        }

        public override void Dispose()
        {
            base.Dispose();
            cts = default;
        }
    }
}
