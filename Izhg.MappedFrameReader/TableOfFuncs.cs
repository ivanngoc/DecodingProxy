using System.Threading.Tasks;
using Func = System.Func<System.ReadOnlyMemory<byte>, IziHardGames.MappedFrameReader.TableOfResults, System.Threading.Tasks.ValueTask<int>>;
using Wrap = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.ValueTask<int>>;

namespace IziHardGames.MappedFrameReader
{
    public class TableOfFuncs
    {
        public readonly Dictionary<string, Wrap> funcs = new Dictionary<string, Wrap>();
        internal TableOfResults tableOfResults = new TableOfResults();
        public void AddAdvancingFunc(string id, Func func)
        {
            Wrap wrap = (x) => func(x, tableOfResults);
            funcs.Add(id, wrap);
        }
        internal Wrap GetFunc(string valueString)
        {
            return funcs[valueString];
        }
    }

    public static class PopularFuncs
    {
        public static ValueTask<int> ReadBodyHttp11(ReadOnlyMemory<byte> mem, TableOfResults results)
        {
            return ValueTask.FromResult<int>(0);
        }
    }
}