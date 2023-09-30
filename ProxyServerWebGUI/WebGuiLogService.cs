using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace IziHardGames.Proxy
{
    public class WebGuiLogService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var output = Console.Out;
            var input = Console.In;

            Stream stdIn = Console.OpenStandardInput();
            Stream stdOut = Console.OpenStandardOutput();
            Stream stdErr = Console.OpenStandardError();

            Console.WriteLine("Not Implemented Start");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Not Implemented Stop");
            return Task.CompletedTask;
        }
    }
}