using System.Collections.Generic;
using System.Text;
using System;

namespace IziHardGames.Libs.ForHttp20.DecodingHeaders
{

    /// <summary>
    /// The StaticTable class.
    /// </summary>
    public static class StaticTable
    {
        /// <summary>
        /// The static table
        /// Appendix A: Static Table
        /// </summary>
        /// <note type="rfc">http://tools.ietf.org/html/rfc7541#appendix-A</note>
        private static List<HeaderField20> STATIC_TABLE = new List<HeaderField20>() {
			/*  1 */new HeaderField20(":authority", String.Empty),
			/*  2 */new HeaderField20(":method", "GET"),
			/*  3 */new HeaderField20(":method", "POST"),
			/*  4 */new HeaderField20(":path", "/"),
			/*  5 */new HeaderField20(":path", "/index.html"),
			/*  6 */new HeaderField20(":scheme", "http"),
			/*  7 */new HeaderField20(":scheme", "https"),
			/*  8 */new HeaderField20(":status", "200"),
			/*  9 */new HeaderField20(":status", "204"),
			/* 10 */new HeaderField20(":status", "206"),
			/* 11 */new HeaderField20(":status", "304"),
			/* 12 */new HeaderField20(":status", "400"),
			/* 13 */new HeaderField20(":status", "404"),
			/* 14 */new HeaderField20(":status", "500"),
			/* 15 */new HeaderField20("accept-charset", String.Empty),
			/* 16 */new HeaderField20("accept-encoding", "gzip, deflate"),
			/* 17 */new HeaderField20("accept-language", String.Empty),
			/* 18 */new HeaderField20("accept-ranges", String.Empty),
			/* 19 */new HeaderField20("accept", String.Empty),
			/* 20 */new HeaderField20("access-control-allow-origin", String.Empty),
			/* 21 */new HeaderField20("age", String.Empty),
			/* 22 */new HeaderField20("allow", String.Empty),
			/* 23 */new HeaderField20("authorization", String.Empty),
			/* 24 */new HeaderField20("cache-control", String.Empty),
			/* 25 */new HeaderField20("content-disposition", String.Empty),
			/* 26 */new HeaderField20("content-encoding", String.Empty),
			/* 27 */new HeaderField20("content-language", String.Empty),
			/* 28 */new HeaderField20("content-length", String.Empty),
			/* 29 */new HeaderField20("content-location", String.Empty),
			/* 30 */new HeaderField20("content-range", String.Empty),
			/* 31 */new HeaderField20("content-type", String.Empty),
			/* 32 */new HeaderField20("cookie", String.Empty),
			/* 33 */new HeaderField20("date", String.Empty),
			/* 34 */new HeaderField20("etag", String.Empty),
			/* 35 */new HeaderField20("expect", String.Empty),
			/* 36 */new HeaderField20("expires", String.Empty),
			/* 37 */new HeaderField20("from", String.Empty),
			/* 38 */new HeaderField20("host", String.Empty),
			/* 39 */new HeaderField20("if-match", String.Empty),
			/* 40 */new HeaderField20("if-modified-since", String.Empty),
			/* 41 */new HeaderField20("if-none-match", String.Empty),
			/* 42 */new HeaderField20("if-range", String.Empty),
			/* 43 */new HeaderField20("if-unmodified-since", String.Empty),
			/* 44 */new HeaderField20("last-modified", String.Empty),
			/* 45 */new HeaderField20("link", String.Empty),
			/* 46 */new HeaderField20("location", String.Empty),
			/* 47 */new HeaderField20("max-forwards", String.Empty),
			/* 48 */new HeaderField20("proxy-authenticate", String.Empty),
			/* 49 */new HeaderField20("proxy-authorization", String.Empty),
			/* 50 */new HeaderField20("range", String.Empty),
			/* 51 */new HeaderField20("referer", String.Empty),
			/* 52 */new HeaderField20("refresh", String.Empty),
			/* 53 */new HeaderField20("retry-after", String.Empty),
			/* 54 */new HeaderField20("server", String.Empty),
			/* 55 */new HeaderField20("set-cookie", String.Empty),
			/* 56 */new HeaderField20("strict-transport-security", String.Empty),
			/* 57 */new HeaderField20("transfer-encoding", String.Empty),
			/* 58 */new HeaderField20("user-agent", String.Empty),
			/* 59 */new HeaderField20("vary", String.Empty),
			/* 60 */new HeaderField20("via", String.Empty),
			/* 61 */new HeaderField20("www-authenticate", String.Empty)
        };

        private static Dictionary<string, int> STATIC_INDEX_BY_NAME = CreateMap();

        /// <summary>
        /// The number of header fields in the static table.
        /// </summary>
        /// <value>The length.</value>
        public static int Length { get { return STATIC_TABLE.Count; } }

        /// <summary>
        /// Return the header field at the given index value.
        /// </summary>
        /// <returns>The entry.</returns>
        /// <param name="index">Index.</param>
        public static HeaderField20 GetEntry(int index)
        {
            return STATIC_TABLE[index - 1];
        }

        /// <summary>
        /// Returns the lowest index value for the given header field name in the static table.
        /// Returns -1 if the header field name is not in the static table.
        /// </summary>
        /// <returns>The index.</returns>
        /// <param name="name">Name.</param>
        public static int GetIndex(byte[] name)
        {
            var nameString = Encoding.UTF8.GetString(name);
            if (!STATIC_INDEX_BY_NAME.ContainsKey(nameString))
            {
                return -1;
            }
            return STATIC_INDEX_BY_NAME[nameString];
        }

        /// <summary>
        /// Returns the index value for the given header field in the static table.
        /// Returns -1 if the header field is not in the static table.
        /// </summary>
        /// <returns>The index.</returns>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        public static int GetIndex(byte[] name, byte[] value)
        {
            var index = GetIndex(name);
            if (index == -1)
            {
                return -1;
            }

            // Note this assumes all entries for a given header field are sequential.
            while (index <= StaticTable.Length)
            {
                var entry = GetEntry(index);
                if (!HeaderField20.Equals(name, entry.Name))
                {
                    break;
                }
                if (HeaderField20.Equals(value, entry.Value))
                {
                    return index;
                }
                index++;
            }

            return -1;
        }

        /// <summary>
        /// create a map of header name to index value to allow quick lookup
        /// </summary>
        /// <returns>The map.</returns>
        private static Dictionary<string, int> CreateMap()
        {
            var length = STATIC_TABLE.Count;
            var ret = new Dictionary<string, int>(length);

            // Iterate through the static table in reverse order to
            // save the smallest index for a given name in the map.
            for (var index = length; index > 0; index--)
            {
                var entry = GetEntry(index);
                var name = Encoding.UTF8.GetString(entry.Name);
                ret[name] = index;
            }
            return ret;
        }
    }
}
