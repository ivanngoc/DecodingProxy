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
        public static T ToStruct<T>(this IIndexReader<byte> reader, int offset) where T : unmanaged
        {
            throw new System.NotImplementedException();
        }
        public static ushort ToUshort(this IIndexReader<byte> reader, int offset)
        {
            return BufferReader.ToUshort(reader[offset], reader[offset + 1]);
        }
    }
}
