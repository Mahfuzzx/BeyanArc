
using System.Text.Json;

Settings settings = new();

if (args.Length == 1 && args[0] == "-s") LoadSettings();
else if (args.Length < 3)
{
    Console.WriteLine("Kullanım: BeyanArc.exe [-s] [Kaynak VergiHedef SGKHedef]");
    Environment.Exit(1);
}

if (args[0] == "-s")
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

static void moveFile(string sourceFile, string destFile)
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

static string addSlash(string path)
{
    return path[^1..] != "\\" ? path + "\\" : path;
}

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

void SaveSettings()
{
    var jsonString = JsonSerializer.Serialize(settings);
    File.WriteAllText("settings.json", jsonString);
}
class Settings
{
    public Settings()
    {
        this.sourcePath = "";
        this.taxPath = "";
        this.sgkPath = "";
    }

    public string sourcePath { get; set; }
    public string taxPath { get; set; }
    public string sgkPath { get; set; }
}

class BFile
{
    public readonly string? destPath;
    public readonly string? destFileName;
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

    private static string calcPeriod(int beginMonth, int endMonth)
    {
        return (endMonth / (endMonth - beginMonth + 1)).ToString().PadLeft(2, '0');
    }

    private static string filterChars(string srcString, bool onlySpace = false)
    {
        char[] ochars = [' ', 'Ğ', 'Ü', 'Ş', 'İ', 'Ö', 'Ç'];
        char[] nchars = ['_', 'G', 'U', 'S', '_', 'O', 'C'];
        string output = srcString;
        if (onlySpace) output = output.Replace(' ', '_');
        else
        {
            for (int i = 0; i < ochars.Length; i++)
            {
                output = output.Replace(ochars[i], nchars[i]);
            }
        }
        return output;
    }
}