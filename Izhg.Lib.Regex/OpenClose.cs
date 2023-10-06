using System.Text.RegularExpressions;

namespace IziHardGames.Libs.RegexExpressions
{
    public static class OpenClose
    {
        /// <summary>
        /// ищет самое первое вхождение. если идут подряд {{}{}{{}}} {} {} то выберет только самый первую пару вместе с вложениями
        /// </summary>
        public static readonly Regex firstEnclosure = new Regex(@"{(?>[^{}]+|(?<Open>{)|(?<Close-Open>}))*}");


        public static string Tag(string tag)
        {
            throw new System.NotImplementedException();
        }
    }
}