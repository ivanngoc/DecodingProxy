using System.Threading.Tasks;
using IziHardGames.DataRecording.Abstractions.Lib;
using IziHardGames.DataRecording.Abstractions.Lib.Headers;
using IziHardGames.DataRecording.Abstractions.Lib.ToDataBase;

namespace IziHardGames.DataRecording.Lib.ToDataBase
{
    public class IziDataRecorderPostgreSql : IziDataRecorderToDatabaseAbstract
    {
        private DataRecordingPostgreSql context;
        public override async Task SaveToDataBaseAsync<T>(DelimeterTyped delimeter, T data)
        {
            using (context = new())
            {

            }
        }

        public override Task SaveToDataBaseAsync(DelimeterTyped delimeter, string description, byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}