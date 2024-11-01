﻿using BeyanArc;

if (args.Length == 0 && File.Exists("settings.json")) Settings.load();
else if (args.Length < 3)
{
    Console.WriteLine("Kullanım: BeyanArc.exe [Kaynak VergiHedef SGKHedef] [--o] [--c] [--k] [--s]");
    Environment.Exit(1);
}
else
{
    Settings.sourcePath = args[0];
    Settings.taxPath = StringOperationsHelper.addSlash(args[1]);
    Settings.sgkPath = StringOperationsHelper.addSlash(args[2]);
    Settings.overwrite = args.Contains("--o");
    Settings.copyMode = args.Contains("--c");
    Settings.keepBoth = args.Contains("--k");
    if (args.Contains("--s")) Settings.save();
}

string[] files = Directory.GetFiles(Settings.sourcePath, "*.pdf");

foreach (string file in files)
{
    BFile bFile = new(file);
    Console.Write($"{file} dosyası");
    if (bFile.type == "UNKNOWN") Console.WriteLine(" tanımsız.");
    else
    {
        var destPath = (bFile.type == "TAX" ? Settings.taxPath : Settings.sgkPath) + bFile.destPath;
        var destFile = $"{destPath}\\{bFile.destFileName}.pdf";
        FileOperationsHelper.moveFile(file, destFile);
        Console.WriteLine($"\n{destFile} hedefine taşındı.");
    }
}


