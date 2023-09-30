using System;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Libs.Pipelines;
using IziHardGames.Proxy.Sniffing.ForHttp;

namespace IziHardGames.Libs.ForHttp.Http11
{
    public class ObjectReaderHttp20Piped : PipedObjectReader<HttpReadResult>
    {
        public override void Dispose()
        {
            base.Dispose();
        }
        public async override ValueTask<HttpReadResult> ReadObjectAsync()
        {
            var reader = readerProvider!.Reader;

            while (true)
            {
                var result = await reader.ReadAsync().ConfigureAwait(false);
                var buffer = result.Buffer;
                Console.WriteLine($"As String: {Encoding.UTF8.GetString(buffer)}");
                Console.WriteLine($"As Byte: {ParseByte.ToHexStringFormated(buffer)}");
                reader.AdvanceTo(result.Buffer.End);
            }
        }
    }
}