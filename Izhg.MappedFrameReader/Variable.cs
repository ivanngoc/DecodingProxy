namespace IziHardGames.MappedFrameReader
{
    internal class Variable
    {
        /// <summary>
        /// Идентификатор переменной
        /// </summary>
        public string id;
        /// <summary>
        /// Имя поля
        /// </summary>
        private string path;
        /// <summary>
        /// Index in types table
        /// </summary>
        public DefinedType type;
        /// <summary>
        /// Node which perform Read Of That Node
        /// </summary>
        public Node node;
        public NodeResult nodeResult;

        public Variable(string id, string fieldName, DefinedType type, Node node, NodeResult result)
        {
            this.id = id;
            this.path = fieldName;
            this.type = type;
            this.node = node;
            this.nodeResult = result;
        }
    }
}