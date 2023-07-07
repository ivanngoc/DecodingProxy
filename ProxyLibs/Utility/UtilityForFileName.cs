using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.IO
{

    public static class UtilityForFileName
    {
        private static readonly char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

        public static string FormatFileName(string input)
        {
            return new string(input.Where(x => !invalidFileNameChars.Contains(x)).ToArray());
        }
    }
}
