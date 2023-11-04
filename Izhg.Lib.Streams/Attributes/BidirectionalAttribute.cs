using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Streams.Attributes
{
    public class FakeWriteAttribute : Attribute
    {

    }

    public class OneDirectionAttribute : Attribute
    {
        public ETransmitType Type { get; set; }
    }

    /// <summary>
    /// Duplex mode
    /// </summary>
    public class BidirectionalAttribute : Attribute
    {
        public ETransmitType Type { get; set; }
    }

    public enum ETransmitType
    {
        None,
        OnlyWrite,
        OnlyRead,
        /// <summary>
        /// It's possibe for Arguments to point to the same Buffer or not
        /// </summary>
        ReadWrite,
        /// <summary>
        /// Read or write to Same source (for example - File System)
        /// </summary>
        ReadWriteToSameBuffer,
        /// <summary>
        /// Read of Write to Different Buffers (for example - Network System)
        /// </summary>
        ReadWriteToDifferentBuffer,
    }
}
