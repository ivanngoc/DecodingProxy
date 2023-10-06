using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Lib.Collections.Contracts
{
    public interface ISingleEdgeList<T> : ISingleEdgeNode<T> 
    {

    }

    public interface ISingleEdgeNode<T>
    {
        public T? Next { get; set; }
    }
}
