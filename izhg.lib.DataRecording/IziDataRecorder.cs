using System.Threading.Tasks;
using IziHardGames.DataRecording.Abstractions.Lib.Headers;

namespace IziHardGames.DataRecording.Abstractions.Lib
{
    public static class IziDataRecorder
    {
        public static IziDataRecorderToDatabaseAbstract? ToDataBase { get; set; }
        public static IziDataRecorderToFile? ToFile { get; set; }
    }

    public abstract class IziDataRecorderToFile
    {

    }
    public abstract class IziDataRecorderToDatabaseAbstract
    {
        public abstract Task SaveToDataBaseAsync<T>(DelimeterTyped delimeter, T data);
        public abstract Task SaveToDataBaseAsync(DelimeterTyped delimeter, string description, byte[] data);
    }
}
