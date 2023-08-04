using System;
using System.IO;

namespace IziHardGames.Libs.IO
{
    public static class HelperForStream
    {
        public static void CopyStream(Stream from, Stream to)
        {
            byte[] bytes = new byte[8096];

            try
            {
                while (true)
                {
                    var readed = from.Read(bytes, 0, bytes.Length);
                    to.Write(bytes, 0, readed);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

    }
}
