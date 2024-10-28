using System.Globalization;

namespace BeyanArc
{

    /// <summary>
    /// Class to represent and parse information from a PDF file name
    /// </summary>
    public class BFile
    {
        /// <summary>
        /// Destination path based on file type and content
        /// </summary>
        public readonly string? destPath;
        /// <summary>
        /// Generated file name after processing
        /// </summary>
        public readonly string? destFileName;
        /// <summary>
        /// Type of the file, either "TAX" or "SGK"
        /// </summary>
        public readonly string type;
        public readonly DateTime creationTime;

        private readonly string? customerName;
        private readonly int beginYear;
        private readonly int beginMonth;
        private readonly int endYear;
        private readonly int endMonth;
        private readonly string? taxType;
        private readonly string? subDivName;
        private readonly string? periodString;
        private readonly string? sgkType;
        private readonly string? fileType;
        private readonly string[] months = ["OCAK", "SUBAT", "MART", "NISAN", "MAYIS", "HAZIRAN",
                                        "TEMMUZ", "AGUSTOS", "EYLUL", "EKIM", "KASIM", "ARALIK"];
        private readonly PdfMetadataReader? purePdfMetadataReader;

        /// <summary>
        /// Constructor that processes and extracts metadata from the given file name.
        /// </summary>
        /// <param name="file">The PDF file name to process.</param>
        public BFile(string file)
        {
            try
            {
                string[] parts = Path.GetFileName(file).Split('_');
                if (parts.Length < 7) throw new Exception();
                purePdfMetadataReader = new(file);
                string dateString = purePdfMetadataReader.metaData["CreationDate"]; //purePdfMetadataReader.readMetadata("CreationDate");
                dateString = dateString[(dateString.IndexOf("D:") + 2)..];
                dateString = dateString[..(dateString.Length - 1)].Replace("'", ":");

                // Define the date format
                string format = "yyyyMMddHHmmssK";

                // Parse the date string
                DateTime dateTime = DateTime.ParseExact(dateString, format, CultureInfo.InvariantCulture);
                creationTime = dateTime;
                type = parts[2].Length > 9 ? "TAX" : "SGK";
                if (type == "TAX")
                {
                    customerName = filterChars(parts[0]);
                    string[] periods = parts[5].Split('-');
                    beginYear = int.Parse(periods[0][4..]);
                    beginMonth = int.Parse(periods[0].Substring(2, 2));
                    endYear = int.Parse(periods[1][4..]);
                    endMonth = int.Parse(periods[1].Substring(2, 2));
                    taxType = parts[3];
                    destPath = $"{customerName}\\{endYear}\\{taxType}";
                    destFileName = $"{parts[3]}";
                }
                else
                {
                    customerName = filterChars(parts[0], true);
                    beginYear = int.Parse(parts[2][..4]);
                    beginMonth = int.Parse(parts[2].Substring(4, 2));
                    endYear = beginYear;
                    endMonth = beginMonth;
                    subDivName = parts[1];
                    destPath = $"{endYear}\\{customerName}\\{subDivName}";
                    sgkType = parts[5][2..] switch
                    {
                        "A" => "ASIL",
                        "E" => "EK",
                        "I" => "IPTAL",
                        _ => "UNKNOWN",
                    };
                    if (sgkType == "UNKNOWN") throw new Exception();
                    destFileName = $"{parts[5][..2]}_{parts[4]}_{sgkType}";
                }
                fileType = parts[6] switch
                {
                    "THK" => "TAHAKKUK",
                    "BYN" => "BEYANNAME",
                    "HZM" => "HIZMET_LISTESI",
                    _ => "UNKNOWN",
                };
                if (fileType == "UNKNOWN") throw new Exception();
                periodString = calcPeriod(beginMonth, endMonth) + "_" + (endMonth == beginMonth ? months[endMonth - 1] : "DONEM");
                destFileName = $"{periodString}_{destFileName}_{fileType}_{creationTime:yyyyMMddHHmmss}";
            }
            catch (Exception)
            {
                type = "UNKNOWN";
                return;
            }
        }

        /// <summary>
        /// Calculates the period string based on the start and end month.
        /// </summary>
        /// <param name="beginMonth">The beginning month of the period.</param>
        /// <param name="endMonth">The ending month of the period.</param>
        /// <returns>A string representing the period, padded with zeros if needed.</returns>
        private static string calcPeriod(int beginMonth, int endMonth)
        {
            return (endMonth / (endMonth - beginMonth + 1)).ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// Replaces special characters (Turkish letters) in the given string.
        /// </summary>
        /// <param name="srcString">The source string to filter.</param>
        /// <param name="onlySpace">Whether to replace only spaces with underscores.</param>
        /// <returns>A string with filtered characters.</returns>
        private static string filterChars(string srcString, bool onlySpace = false)
        {
            Dictionary<char, char> charMap = new()
    {
        { ' ', '_' }, { 'Ğ', 'G' }, { 'Ü', 'U' }, { 'Ş', 'S' },
        { 'İ', '_' }, { 'Ö', 'O' }, { 'Ç', 'C' }
    };

            if (onlySpace)
            {
                return srcString.Replace(' ', '_');
            }

            foreach (var pair in charMap)
            {
                srcString = srcString.Replace(pair.Key, pair.Value);
            }
            return srcString;
        }
    }
}
