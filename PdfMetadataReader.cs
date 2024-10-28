using System.Text.RegularExpressions;

namespace BeyanArc
{
    partial class PdfMetadataReader
    {
        private partial class DataExtractor
        {
            [GeneratedRegex("/(\\w+)\\s*\\((.*?)\\)", RegexOptions.Singleline)]
            private static partial Regex MyRegex();

            public static Dictionary<string, string> extractFields(string data)
            {
                var result = new Dictionary<string, string>();

                var match = MyRegex().Match(data);

                while (match.Success)
                {
                    var fieldName = match.Groups[1].Value;
                    var fieldValue = match.Groups[2].Value;
                    result[fieldName] = fieldValue;
                    match = match.NextMatch();
                }

                return result;
            }
        }

        public Dictionary<string, string> metaData;

        public PdfMetadataReader(string filePath)
        {
            using var reader = new BinaryReader(File.Open(filePath, FileMode.Open));
            string fileContent = readAllText(reader);
            metaData = DataExtractor.extractFields(fileContent);
        }

        private static string readAllText(BinaryReader reader)
        {
            using var memoryStream = new MemoryStream();
            reader.BaseStream.CopyTo(memoryStream);
            return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
