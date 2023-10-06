namespace IziHardGames.MappedFrameReader
{
    public class VariableResultStorage
    {

    }

    public class ValuePromise
    {
        public int type;

        internal ValuePromise IfEqual(in ReadOnlyMemory<byte> readOnlyMemory)
        {
            throw new NotImplementedException();
        }

        internal ReadOnlyMemory<byte> ParseValue(string meta)
        {
            throw new NotImplementedException();
        }
    }
}