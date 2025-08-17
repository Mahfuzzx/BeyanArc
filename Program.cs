using BeyanArc;

// D:\INDIRILEN \\Halil1\BEYANNAMELER\LUCA \\Halil1\BEYANNAMELER\SGK --o --s
if (args.Length == 0 && File.Exists("settings.json")) Settings.load();
else if (args.Length < 3)
{
    Console.WriteLine("Kullanım: BeyanArc.exe [Kaynak VergiHedef SGKHedef] [--o] [--c] [--s]");
    Environment.Exit(1);
}
else
{
    Settings.sourcePath = args[0];
    Settings.taxPath = args[1];
    Settings.sgkPath = args[2];
    Settings.overwrite = args.Contains("--o");
    Settings.copyMode = args.Contains("--c");
    if (args.Contains("--s")) Settings.save();
}

var customers = CsvHelper.readCsv<Customer>("musteriler.csv");

string[] files = Directory.GetFiles(Settings.sourcePath, "*.pdf");

foreach (string file in files)
{
    PdfTextHelper pdfTextHelper = new(file, 1);
    var sgkLabel = pdfTextHelper.hasLabel("SOSYAL GÜVENLİK KURUMU BAŞKANLIĞI");
    var tahakkukLabel = pdfTextHelper.hasLabel("TAHAKKUK FİŞİ");
    var beyannameLabel = pdfTextHelper.hasLabel("BEYANNAMESİ");
    var sgkBildirgeLabel = pdfTextHelper.hasLabel("SİGORTALI HİZMET LİSTESİ");
    Console.WriteLine($"{file} dosyası");
    string destPathName = "";
    string destFileName = "";
    string? season = null;
    string? vkn = null;
    if (sgkBildirgeLabel)
    {
        season = pdfTextHelper.getRightOf("Yıl - Ay")?.Replace(":", "").Trim();
    }
    else if (sgkLabel && tahakkukLabel)
    {
        season = pdfTextHelper.getRightOf("AİT OLDUĞU YIL / AY")?.Replace(":", "").Trim();
        vkn = pdfTextHelper.getRightOf("VERGİ KİMLİK NUMARASI");
        if (vkn?.Length > 11)
            vkn = vkn[..11];
    }
    if (string.IsNullOrEmpty(season)) continue;
    if (sgkLabel)
    {
        string[] parts = season.Split('/');
        string year = parts[0].Trim();
        string month = parts.Length > 1 ? parts[1].Trim() : "";
        destPathName = $"{year}\\{filterChars("")}";
    }
    var destPath = Path.Combine(!sgkLabel ? Settings.taxPath : Settings.sgkPath, destPathName);
    var destFile = Path.Combine(destPath, destFileName);
    FileOperationsHelper.moveFile(file, destFile);
    Console.WriteLine($"\n{destFile} hedefine taşındı.");
}

static string filterChars(string srcString, bool onlySpace = false)
{
    Dictionary<char, char> charMap = new() { { ' ', '_' }, { 'Ğ', 'G' }, { 'Ü', 'U' }, { 'Ş', 'S' }, { 'İ', '_' }, { 'Ö', 'O' }, { 'Ç', 'C' } };

    if (onlySpace) return srcString.Replace(' ', '_');

    foreach (var pair in charMap) srcString = srcString.Replace(pair.Key, pair.Value);

    return srcString;
}


