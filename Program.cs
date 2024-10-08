﻿
using System.Text.Json;

Settings settings = new();

if (args.Length == 1 && args[0].Equals("-s", StringComparison.CurrentCultureIgnoreCase)) LoadSettings();
else if (args.Length < 3)
{
    Console.WriteLine("Kullanım: BeyanArc.exe [-s] [Kaynak VergiHedef SGKHedef]");
    Environment.Exit(1);
}

if (args[0].Equals("-s", StringComparison.CurrentCultureIgnoreCase))
{
    settings.sourcePath = args[1];
    settings.taxPath = addSlash(args[2]);
    settings.sgkPath = addSlash(args[3]);
    SaveSettings();
}
else
{
    settings.sourcePath = args[0];
    settings.taxPath = addSlash(args[1]);
    settings.sgkPath = addSlash(args[2]);
}

string[] files = Directory.GetFiles(settings.sourcePath, "*.pdf");

foreach (string file in files)
{
    BFile bFile = new(Path.GetFileName(file));
    Console.Write($"{file} dosyası");
    if (bFile.type == "UNKNOWN") Console.WriteLine(" tanımsız.");
    else
    {
        var destPath = (bFile.type == "TAX" ? settings.taxPath : settings.sgkPath) + bFile.destPath;
        var destFile = $"{destPath}\\{bFile.destFileName}.pdf";
        moveFile(file, destFile);
        Console.WriteLine($"\n{destFile} hedefine taşındı.");
    }
}

/// <summary>
/// Moves a file from the source location to the destination, creating directories if necessary.
/// </summary>
/// <param name="sourceFile">The source file path.</param>
/// <param name="destFile">The destination file path.</param>
void moveFile(string sourceFile, string destFile)
{
    try
    {
        string destDir = Path.GetDirectoryName(destFile) ?? "";

        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        File.Move(sourceFile, destFile);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

/// <summary>
/// Adds a trailing slash to the path if it doesn't already have one.
/// </summary>
/// <param name="path">The file or directory path to process.</param>
/// <returns>The path with a trailing backslash.</returns>
string addSlash(string path)
{
    return path[^1..] != "\\" ? path + "\\" : path;
}

/// <summary>
/// Loads settings from the 'settings.json' file or creates a new Settings instance.
/// </summary>
void LoadSettings()
{
    var settingsFile = "settings.json";

    if (!File.Exists(settingsFile)) settings = new();
    else
    {
        var jsonString = File.ReadAllText(settingsFile);
        settings = JsonSerializer.Deserialize<Settings>(jsonString) ?? new();
    }
}

/// <summary>
/// Saves the current settings to the 'settings.json' file.
/// </summary>
void SaveSettings()
{
    var jsonString = JsonSerializer.Serialize(settings);
    File.WriteAllText("settings.json", jsonString);
}

/// <summary>
/// Class to hold the paths for the source, tax, and SGK directories
/// </summary>
class Settings
{
    public Settings()
    {
        this.sourcePath = "";
        this.taxPath = "";
        this.sgkPath = "";
    }

    /// <summary>
    /// Path to the source directory containing the PDF files.
    /// </summary>
    public string sourcePath { get; set; }
    /// <summary>
    /// Path to the directory where tax files will be moved.
    /// </summary>
    public string taxPath { get; set; }
    /// <summary>
    /// Path to the directory where SGK files will be moved.
    /// </summary>
    public string sgkPath { get; set; }
}

/// <summary>
/// Class to represent and parse information from a PDF file name
/// </summary>
class BFile
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

    /// <summary>
    /// Constructor that processes and extracts metadata from the given file name.
    /// </summary>
    /// <param name="file">The PDF file name to process.</param>
    public BFile(string file)
    {
        try
        {
            string[] parts = file.Split('_');
            if (parts.Length != 8) throw new Exception();
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
            destFileName = $"{periodString}_{destFileName}_{fileType}";
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