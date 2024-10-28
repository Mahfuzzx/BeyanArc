using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyanArc
{
    internal class StringOperationsHelper
    {
        /// <summary>
        /// Adds a trailing slash to the path if it doesn't already have one.
        /// </summary>
        /// <param name="path">The file or directory path to process.</param>
        /// <returns>The path with a trailing backslash.</returns>
        public static string addSlash(string path)
        {
            return path[^1..] != "\\" ? path + "\\" : path;
        }
    }
}
