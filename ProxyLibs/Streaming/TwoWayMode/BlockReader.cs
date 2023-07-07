using IziHardGames.Proxy.Datas;
using Collection = System.Collections.Concurrent.ConcurrentQueue<IziHardGames.Proxy.Datas.Block>;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class BlockReader : IDisposable
    {
        private int counter;
        private readonly Collection blocks = new Collection();
        private Stream output;

        public void StartReusable(Stream output)
        {
            this.output = output;
        }

        public void PushBlock(Block block)
        {
#if DEBUG
            if (block.length <= 0)
            {
                throw new ArgumentException($"Empty block");
            }
#endif
            counter++;
            block.SetSequnceNumber(counter);
            blocks.Enqueue(block);
        }

        public void Dispose()
        {
            foreach (var block in blocks)
            {
                block.TryDispose();
            }
            CleanToReuse();
        }

        public void CleanToReuse()
        {
            counter = default;
            blocks.Clear();
        }

        public void Update(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (blocks.TryDequeue(out var block))
                {
                    output.Write(block.Data, 0, block.length);
                    block.Release();
                }
            }
        }
    }
}