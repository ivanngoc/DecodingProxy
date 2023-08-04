using IziHardGames.Libs.Networking.Pipelines;
using System.IO;
using System.Threading.Tasks;

namespace Test
{
    public class TestDataFromFile : TcpClientPiped
    {
        private FileStream fs;
        public bool isEnded;

        public void Start()
        {
            string filename = "C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\ProxyForDecoding\\test data\\test.txt";
            this.fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            FillPipe();
        }

        public async Task FillPipe()
        {
            while (fs.Position < fs.Length)
            {
                var memory = writer.GetMemory(4096);
                var readed = await fs.ReadAsync(memory).ConfigureAwait(false);
                writer.Advance(readed);
                var res = await writer.FlushAsync().ConfigureAwait(false);
            }
            isEnded = true;
        }
    }
}
