namespace IziHardGames.MappedFrameReader
{
    internal class TableOfPromises
    {
        private readonly Dictionary<string, ValuePromise> promises = new Dictionary<string, ValuePromise>();
        public  TableOfVariables? tableOfVariables;

        internal ValuePromise GetValuePromise(string path)
        {
            if (!promises.TryGetValue(path, out var promise))
            {
                var variable = tableOfVariables.GetVariable(path);
                promise = new ValuePromise(path, variable);
                promises.Add(path, promise);
            }
            return promise;
        }
    }
}