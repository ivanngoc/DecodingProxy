namespace IziHardGames.MappedFrameReader
{
    internal class ValuePromise
    {
        public string path;
        public Variable variable;

        public ValuePromise(string path, Variable variable)
        {
            this.path = path;
            this.variable = variable;
        }

        internal ValuePromise IfEqual(in ReadOnlyMemory<byte> readOnlyMemory)
        {
            throw new NotImplementedException();
        }

        public ReadOnlyMemory<byte> GetValue(ReaderContext context)
        {
            var value = context.tableOfResults.GetResult(path);
            return value.nodeResult.ResultDeepCopy;
        }
        internal ReadOnlyMemory<byte> ParseValue(string meta)
        {
            throw new NotImplementedException();
        }
    }
}