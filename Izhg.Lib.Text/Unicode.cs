using System;
using System.Globalization;

namespace IziHardGames.Libs.Text
{
    public static class Unicode
    {
        /// <summary>
        /// Space, tab, line feed (newline), carriage return, form feed, and vertical tab characters are called "white-space characters" because they serve the same purpose as the spaces between words and lines on a printed page — they make reading easier. Tokens are delimited (bounded) by white-space characters and by other tokens, such as operators and punctuation. When parsing code, the C compiler ignores white-space characters unless you use them as separators or as components of character constants or string literals. Use white-space characters to make a program more readable. Note that the compiler also treats comments as white space.
        /// https://learn.microsoft.com/en-us/cpp/c-language/white-space-characters?view=msvc-170
        /// https://learn.microsoft.com/en-us/dotnet/api/system.char.iswhitespace?view=net-7.0&devlangs=csharp&f1url=%3FappId%3DDev16IDEF1%26l%3DEN-US%26k%3Dk(System.Char.IsWhiteSpace)%3Bk(DevLang-csharp)%26rd%3Dtrue
        /// </summary>
        public readonly static char[] whieSpaces = new char[]{

            (char)9, //cat:Control. Binary: 1001.                               // \t 
            (char)10, //cat:Control. Binary: 1010.                              // \n LF
            (char)11, //cat:Control. Binary: 1011.
            (char)12, //cat:Control. Binary: 1100.
            (char)13, //cat:Control. Binary: 1101.                              // \r CR
            (char)32, //cat:SpaceSeparator. Binary: 100000.                     // Space
            (char)133, //cat:Control. Binary: 10000101.
            (char)160, //cat:SpaceSeparator. Binary: 10100000.
            (char)5760, //cat:SpaceSeparator. Binary: 1011010000000.
            (char)8192, //cat:SpaceSeparator. Binary: 10000000000000.
            (char)8193, //cat:SpaceSeparator. Binary: 10000000000001.
            (char)8194, //cat:SpaceSeparator. Binary: 10000000000010.
            (char)8195, //cat:SpaceSeparator. Binary: 10000000000011.
            (char)8196, //cat:SpaceSeparator. Binary: 10000000000100.
            (char)8197, //cat:SpaceSeparator. Binary: 10000000000101.
            (char)8198, //cat:SpaceSeparator. Binary: 10000000000110.
            (char)8199, //cat:SpaceSeparator. Binary: 10000000000111.
            (char)8200, //cat:SpaceSeparator. Binary: 10000000001000.
            (char)8201, //cat:SpaceSeparator. Binary: 10000000001001.
            (char)8202, //cat:SpaceSeparator. Binary: 10000000001010.
            (char)8232, //cat:LineSeparator. Binary: 10000000101000.
            (char)8233, //cat:ParagraphSeparator. Binary: 10000000101001.
            (char)8239, //cat:SpaceSeparator. Binary: 10000000101111.
            (char)8287, //cat:SpaceSeparator. Binary: 10000001011111.
            (char)12288, //cat:SpaceSeparator. Binary: 11000000000000.
        };
        static Unicode()
        {
            /// Members of the UnicodeCategory.SpaceSeparator category
            //SPACE(U + 0020)
            //NO - BREAK SPACE(U + 00A0)
            //OGHAM SPACE MARK(U + 1680)
            //EN QUAD(U+2000)
            //EM QUAD(U+2001)
            //EN SPACE(U+2002)
            //EM SPACE(U+2003)
            //THREE - PER - EM SPACE(U + 2004)
            //FOUR - PER - EM SPACE(U + 2005)
            //SIX - PER - EM SPACE(U + 2006)
            //FIGURE SPACE(U+2007)
            //PUNCTUATION SPACE(U+2008)
            //THIN SPACE(U+2009)
            //HAIR SPACE(U+200A)
            //NARROW NO-BREAK SPACE(U + 202F)
            //MEDIUM MATHEMATICAL SPACE(U + 205F)
            //IDEOGRAPHIC SPACE(U + 3000)

            ///Members of the UnicodeCategory.LineSeparator category, which consists solely of the 
            // LINE SEPARATOR character (U+2028)

            /// Members of the UnicodeCategory.ParagraphSeparator category which consists solely of the 
            // PARAGRAPH SEPARATOR character (U+2029)

            /// The characters: 
            //CHARACTER TABULATION (U+0009)
            //LINE FEED (U+000A)
            //LINE TABULATION (U+000B)
            //FORM FEED (U+000C)
            //CARRIAGE RETURN (U+000D)
            //and NEXT LINE (U+0085)
        }

        public static char[] GetWhieSpaces(UnicodeCategory category)
        {
            return whieSpaces;
        }
        public static char[] GetChars(UnicodeCategory category)
        {
            throw new System.NotImplementedException();
        }

        public static void PrintWhiteSpaces()
        {
            for (int i = char.MinValue; i < char.MaxValue; i++)
            {
                char c = Convert.ToChar(i);
                if (c > char.MinValue)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        var cat = char.GetUnicodeCategory(c);
                        Console.WriteLine($"(char){(int)c}, //cat:{cat}. Binary: {Convert.ToString(c, 2)}.");
                    }
                }
            }
        }
    }
}
