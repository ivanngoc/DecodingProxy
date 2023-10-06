namespace IziHardGames.MappedFrameReader
{
    internal class Variables
    {
        private string typeName;
        private string fieldName;

        public Variables(string typeName, string fieldName)
        {
            this.typeName = typeName;
            this.fieldName = fieldName;
        }

        internal static ValuePromise GetValue(string idVariable)
        {
            throw new NotImplementedException();
        }
    }
}