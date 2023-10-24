using System;
using System.Runtime.ConstrainedExecution;
using System.Threading;

namespace IziHardGames.Libs.Text
{
    public static class ExtensionsForString
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="StackOverflowException">method is recursive</exception>
        public static string[] SplitByWhiteSpaceRecursive(this string value)
        {
            int count = 0;
            throw new System.NotImplementedException();
        }
        public static string[] SplitByWhiteSpace(this string value)
        {
            return value.Split(Unicode.whieSpaces, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
