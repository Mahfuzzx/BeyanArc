using iTextSharp.text.pdf;
using System.Text.RegularExpressions;

namespace BeyanArc
{
    namespace Pure
    {
        public partial class PdfMetadataReader(string filePath)
        {
            private readonly string _filePath = filePath;
            [GeneratedRegex("/ModDate(.*?)>>", RegexOptions.Singleline)]
            private static partial Regex myRegex();

            public string readMetadata(string key)
            {
                try
                {
                    using var reader = new BinaryReader(File.Open(_filePath, FileMode.Open));
                    // Read the PDF file until we find the /Info section
                    string fileContent = readAllText(reader);
                    var infoMatch = myRegex().Match(fileContent);

                    if (infoMatch.Success)
                    {
                        string infoContent = infoMatch.Groups[1].Value;

                        // Extract individual metadata fields
                        return extractField(infoContent, $"/{key}");
                    }
                    else
                    {
                        throw new Exception("No metadata found.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error reading PDF: {ex.Message}");
                }
            }

            private static string readAllText(BinaryReader reader)
            {
                using var memoryStream = new MemoryStream();
                reader.BaseStream.CopyTo(memoryStream);
                return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            private static string extractField(string content, string fieldName)
            {
                var match = Regex.Match(content, $"{fieldName}\\s+\\((.*?)\\)", RegexOptions.Singleline);
                return match.Success ? $"{fieldName}: {match.Groups[1].Value}" : $"{fieldName}: N/A";
            }
        }
    }

    public class PdfMetadataReader
    {
        private readonly string _filePath;
        private readonly Dictionary<string, string> _metadata;

        public PdfMetadataReader(string filePath)
        {
            _filePath = filePath;
            _metadata = [];
            readMetadata();
        }

        // Property to access metadata
        public Dictionary<string, string> metadata => _metadata;

        // Reads all metadata entries from the PDF
        private void readMetadata()
        {
            using var reader = new PdfReader(_filePath);
            // Populate the metadata dictionary
            foreach (var entry in reader.Info)
            {
                _metadata[entry.Key] = entry.Value;
            }
        }
    }
}
