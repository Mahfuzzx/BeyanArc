using System.Collections;
using System.Security.Cryptography;

namespace BeyanArc
{
    public class FileMover
    {
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
                    Console.WriteLine("Files are identical, no need to move.");
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
                    Console.WriteLine("Files were not identical. Renamed and moved the file.");
                }
            }
            else
            {
                // If no conflict, just move the file
                if (copy) File.Move(sourceFilePath, targetFilePath);
                else File.Move(sourceFilePath, targetFilePath);
                Console.WriteLine("File moved successfully.");
            }
        }

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

        private static byte[] getFileHash(HashAlgorithm hashAlgorithm, string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return hashAlgorithm.ComputeHash(stream);
        }
    }
}
