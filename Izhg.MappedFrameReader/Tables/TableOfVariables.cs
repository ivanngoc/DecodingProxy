namespace IziHardGames.MappedFrameReader
{
    internal class TableOfVariables
    {
        private readonly Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

        public Variable CreateVariable(string id, string path, DefinedType type, Node node, NodeResult result)
        {
            Variable variable = new Variable(id, path, type, node, result);
            variables.Add(path, variable);
            node.WriteToResults(path);
            return variable;
        }
        public Variable GetVariable(string path)
        {
            return variables[path];
        }
    }
}