using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyanArc
{
    internal class FileOperationsHelper
    {
        /// <summary>
        /// Moves a file from the source location to the destination, creating directories if necessary.
        /// </summary>
        /// <param name="sourceFile">The source file path.</param>
        /// <param name="destFile">The destination file path.</param>
        public static void moveFile(string sourceFile, string destFile)
        {
            try
            {
                string destDir = Path.GetDirectoryName(destFile) ?? "";

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                if (File.Exists(destFile) && Settings.overwrite) File.Delete(destFile);
                //FileMover.moveFileIfNotIdentical(sourceFile, destFile, settings.copyMode);
                if (Settings.copyMode) File.Copy(sourceFile, destFile);
                else File.Move(sourceFile, destFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
