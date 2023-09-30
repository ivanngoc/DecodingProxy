using System;
using System.Collections.Generic;
using static IziHardGames.Libs.ForHttp20.ConstantsForHttp20;

namespace IziHardGames.Libs.ForHttp20.DecodingHeaders
{
    public class DynamicTable
    {
        public List<HeaderField20> headerFields= new List<HeaderField20>();
        public HeaderField20 GetEntry(int index)
        {
            if (index < HPACK.LENGTH_STATIC_TABLE)
            {
                return StaticTable.GetEntry(index);
            }
            return headerFields[index];
        }
    }
}
