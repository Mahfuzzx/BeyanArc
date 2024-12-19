using System.Collections;
using System.Security.Cryptography;

namespace BeyanArc
{
    /// <summary>
    /// Provides functionality to move or copy files if they are not identical.
    /// </summary>
    public class FileMover
    {
        /// <summary>
        /// Moves or copies a file to the target path if the file at the target path is not identical to the source file.
        /// </summary>
        /// <param name="sourceFilePath">The path of the source file.</param>
        /// <param name="targetFilePath">The path of the target file.</param>
        /// <param name="copy">If true, the file will be copied instead of moved.</param>
        public static void moveFileIfNotIdentical(string sourceFilePath, string targetFilePath, bool copy = false)
        {
            //string fileName = Path.GetFileName(sourceFilePath);
            //string targetFilePath = Path.Combine(targetDirectory, fileName);
            string targetDirectory = Path.GetDirectoryName(targetFilePath) ?? "";

            // Check if a file with the same name already exists in the target directory
            if (File.Exists(targetFilePath))
            {
                if (areFilesIdentical(sourceFilePath, targetFilePath))
                {
                    return;
                }
                else
                {
                    // Use each file's modified date/time as the timestamp
                    string sourceModifiedDate = File.GetLastWriteTime(sourceFilePath).ToString("yyyyMMdd_HHmmss");
                    string targetModifiedDate = File.GetLastWriteTime(targetFilePath).ToString("yyyyMMdd_HHmmss");

                    // Rename source and target files with their respective modified date
                    string newSourceFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath) ?? "",
                                                            $"{Path.GetFileNameWithoutExtension(targetFilePath)}_{sourceModifiedDate}{Path.GetExtension(sourceFilePath)}");

                    string newTargetFilePath = Path.Combine(targetDirectory,
                                                            $"{Path.GetFileNameWithoutExtension(targetFilePath)}_{targetModifiedDate}{Path.GetExtension(targetFilePath)}");

                    File.Move(sourceFilePath, newSourceFilePath);
                    File.Move(targetFilePath, newTargetFilePath);

                    // Move the renamed source file to the target directory
                    if (copy) File.Copy(newSourceFilePath, Path.Combine(targetDirectory, Path.GetFileName(newSourceFilePath)));
                    else File.Move(newSourceFilePath, Path.Combine(targetDirectory, Path.GetFileName(newSourceFilePath)));
                }
            }
            else
            {
                // If no conflict, just move the file
                if (copy) File.Move(sourceFilePath, targetFilePath);
                else File.Move(sourceFilePath, targetFilePath);
            }
        }

        /// <summary>
        /// Determines if two files are identical by comparing their sizes and SHA256 hashes.
        /// </summary>
        /// <param name="filePath1">The path of the first file.</param>
        /// <param name="filePath2">The path of the second file.</param>
        /// <returns>True if the files are identical, otherwise false.</returns>
        private static bool areFilesIdentical(string filePath1, string filePath2)
        {
            var fileInfo1 = new FileInfo(filePath1);
            var fileInfo2 = new FileInfo(filePath2);

            if (fileInfo1.Length != fileInfo2.Length)
                return false;

            using var hashAlgorithm = SHA256.Create();
            byte[] hash1 = getFileHash(hashAlgorithm, filePath1);
            byte[] hash2 = getFileHash(hashAlgorithm, filePath2);

            return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
        }

        /// <summary>
        /// Computes the SHA256 hash of a file.
        /// </summary>
        /// <param name="hashAlgorithm">The hash algorithm to use.</param>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The SHA256 hash of the file as a byte array.</returns>
        private static byte[] getFileHash(HashAlgorithm hashAlgorithm, string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return hashAlgorithm.ComputeHash(stream);
        }
    }
}
