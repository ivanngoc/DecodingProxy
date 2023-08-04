using IziHardGames.Libs.Tables;
using System.Threading.Tasks;

namespace IziHardGames.Lib.AspNetCore.Components.SyncTables
{
    //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/target-aspnetcore?view=aspnetcore-7.0&tabs=visual-studio

    public class TableControllerAspNet : TableController
    {
        public async Task AddRawAsync<T>()
        {
            throw new System.NotImplementedException();
        }
        public async Task UpdateRawAsync<T>(int id, int indexColumn, T value)
        {
            throw new System.NotImplementedException();
        }
        public async Task UpdateRawAsync<T>(string querySelector, T value)
        {
            throw new System.NotImplementedException();
        }
    }
}
