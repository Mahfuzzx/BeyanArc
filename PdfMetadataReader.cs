using System.Text.RegularExpressions;

namespace BeyanArc
{
    namespace Pure
    {
        internal partial class DataExtractor
        {
            [GeneratedRegex("/(\\w+)\\s*\\((.*?)\\)+", RegexOptions.Singleline)]
            public static partial Regex myRegex();

            public static Dictionary<string, string> extractFields(string data)
            {
                var result = new Dictionary<string, string>();

                // Use regex to find the data block starting with << and ending with >>
                var match = myRegex().Match(data);

                while (match.Success)
                {
                    var fieldName = match.Groups[1].Value;
                    var fieldValue = match.Groups[2].Value;
                    result[fieldName] = fieldValue;
                    match = match.NextMatch();
                }

                /*if (match.Success)
                {
                    // Extract the content within the matched pattern
                    var content = match.Groups[1].Value;

                    // Define a pattern to capture fields like /FieldName(value)
                    var fieldPattern = @"/(\w+)\s*\((.*?)\)";
                    var fieldMatches = Regex.Matches(content, fieldPattern);

                    foreach (Match fieldMatch in fieldMatches)
                    {
                        var fieldName = fieldMatch.Groups[1].Value;
                        var fieldValue = fieldMatch.Groups[2].Value;
                        result[fieldName] = fieldValue;
                    }
                }*/

                return result;
            }
        }

        public partial class PdfMetadataReader
        {
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
}
