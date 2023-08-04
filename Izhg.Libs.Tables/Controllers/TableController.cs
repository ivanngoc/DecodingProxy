using System.Threading.Tasks;

namespace IziHardGames.Libs.Tables
{
    public class TableController
    {
        protected ClassicDynamicTable table;
        public virtual async Task UpdateAsync<T>(int raw, int col, T value)
        {

        }
    }
}
