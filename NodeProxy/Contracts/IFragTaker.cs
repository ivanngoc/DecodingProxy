using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    /// <summary>
    /// <see cref="ETraits.FragmentTaking"/>
    /// </summary>
    internal interface IFragTaker
    {
        public IFragTakable? SourceToTakeFrom { get; set; }
    }
}
