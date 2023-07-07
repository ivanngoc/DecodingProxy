using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Hashing
{
    public static class HashIndex
    {
        public static int GetHash(string st)
        {
            return st.GetHashCode();
        }

        public static int GetHash(Span<char> span)
        {
            return new string(span).GetHashCode();
        }
    }
}
