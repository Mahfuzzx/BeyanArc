using iTextSharp.text.pdf;

namespace BeyanArc
{
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
