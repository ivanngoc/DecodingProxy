namespace IziHardGames.NodeProxies.Nodes
{
    internal interface IFragFlowNode
    {
        void RedirectFragmentsTo<T>(T node) where T : IFragReciever
        {

        }

        void PullFragmentsFrom<T>(T node) where T : IFragGiver
        {

        }

        void CopyFragmentsTo(Node node)
        {

        }
    }
}
