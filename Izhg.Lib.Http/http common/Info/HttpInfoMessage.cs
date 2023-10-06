using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Libs.ForHttp20;
using IziHardGames.Libs.ForHttp20.Maps;

namespace IziHardGames.Libs.HttpCommon.Info
{
    public class HttpInfoMessage
    {
        public int protocol;
        public int length;
        public int lengthHeaders;
        public int lengthBody;
        public string representation = string.Empty;

        public List<HttpInfoHeader> headers = new List<HttpInfoHeader>();

        /// <summary>
        /// <see cref="ReaderHttp2.Test"/>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static HttpInfoMessage Create(MapMessageHttp20 map)
        {
            HttpInfoMessage info = new HttpInfoMessage();
            info.representation = map.ToStringInfo();
            return info;
        }

        public string ToInfoString()
        {
            return $"protocol:{protocol}; length:{length}; representation:{Environment.NewLine}{representation}";
        }
    }

    public class HttpInfoHeader
    {

    }
    public class HttpInfoBody
    {

    }
}
