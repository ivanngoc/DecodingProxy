using IziHardGames.Libs.Binary.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Core.Buffers.Extensions
{

    public static class ExtensionsForIIndexReader
    {
        public static ushort ToUshort(this IIndexReader<byte> reader, int offset)
        {
            return BufferReader.ToUshort(reader[offset], reader[offset + 1]);
        }
    }
}
