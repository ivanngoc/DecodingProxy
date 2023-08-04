using System;
using IziHardGames.Libs.Networking.Contracts;
using ProxyServerWebGUI.Workers;

namespace IziHardGames.Proxy.WebGUI
{
    public class ProxyChangeReflector
    {
        private SignalRInfoService signalRInfoService;
        private readonly Action<IConnectionData>[] actions;
        public ProxyChangeReflector(SignalRInfoService signalRInfoService)
        {
            this.signalRInfoService = signalRInfoService;
            actions = new Action<IConnectionData>[]
            {
                (x)=>{throw new ArgumentOutOfRangeException("Action is 0"); },
                (x)=> signalRInfoService.AddForAll(x),               /// <see cref="Monitoring.ConstantsMonitoring.ACTION_ADD"/>
                (x)=> signalRInfoService.RemoveForAll(x),            /// <see cref="Monitoring.ConstantsMonitoring.ACTION_REMOVE"/>
                (x)=> signalRInfoService.UpdateForAll(x),            /// <see cref="Monitoring.ConstantsMonitoring.ACTION_UPDATE"/>
                (x)=> signalRInfoService.UpdateForAllStatus(x),      /// <see cref="Monitoring.ConstantsMonitoring.ACTION_UPDATE"/>
            };
        }
        public void Recieve(IConnectionData data)
        {
            actions[data.Action].Invoke(data);
        }
    }
}