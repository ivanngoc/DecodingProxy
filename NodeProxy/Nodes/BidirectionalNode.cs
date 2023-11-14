using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Async;

namespace IziHardGames.NodeProxies.Nodes.SOCKS5
{
    internal class BidirectionalNode : Node, IFragGiver, IFragReciever
    {
        protected IFragGiver? agentOut;
        protected IFragReciever? agentIn;
        /// <summary>
        /// Данные на входе ноды. Эти данные нода потребляет
        /// </summary>
        protected readonly Queue<DataFragment> fragmentsToInsertIn = new Queue<DataFragment>();
        /// <summary>
        /// Данные на выходе ноды. Эти донные нода создает
        /// </summary>
        protected readonly Queue<DataFragment> fragmentsToTakeOut = new Queue<DataFragment>();
        protected readonly AsyncSignaler asyncSignaler = new AsyncSignaler();

        public void SetSources(IFragGiver giver, IFragReciever? reciever)
        {
            this.agentOut = giver;
            this.agentIn = reciever;
        }

        public void RecieveFragment(DataFragment fragment)
        {
            lock (this)
            {
                fragmentsToInsertIn.Enqueue(fragment);
            }
        }

        public async Task<DataFragment> TakeFragAsync(CancellationToken ct)
        {
            if (await asyncSignaler.Await(ct))
            {
                lock (fragmentsToTakeOut)
                {
                    return fragmentsToTakeOut.Dequeue();
                }
            }
            else
            {
                throw new TaskCanceledException($"Task cancelled inside {nameof(AsyncSignaler)}");
            }
        }

        protected Task CollectFrames(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    var fragment = await agentOut!.TakeFragAsync(ct).ConfigureAwait(false);
                    lock (fragmentsToInsertIn)
                    {
                        fragmentsToInsertIn.Enqueue(fragment);
                    }
                    asyncSignaler.Set();
                }
            });
        }

        protected async ValueTask<DataFragment> TakeFrameFromIn(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (fragmentsToInsertIn.Count > 0)
                {
                    lock (fragmentsToInsertIn)
                    {
                        DataFragment fragment = fragmentsToInsertIn.Dequeue();
                        return fragment;
                    }
                }
                else
                {
                    await asyncSignaler.Await(ct).ConfigureAwait(false);
                }
            }
            throw new TaskCanceledException("Task canceled");
        }
    }
}
