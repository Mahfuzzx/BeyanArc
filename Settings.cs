using System.Text.Json;

namespace BeyanArc
{
    static class Settings
    {
        private class SettingsJSON
        {
            public string sourcePath { get; set; } = "";
            public string taxPath { get; set; } = "";
            public string sgkPath { get; set; } = "";
            public bool overwrite { get; set; } = false;
            public bool copyMode { get; set; } = false;
        }
        private static SettingsJSON settingsJSON = new();

        public static string sourcePath { get => settingsJSON.sourcePath; set { settingsJSON.sourcePath = value; } }
        public static string taxPath { get => settingsJSON.taxPath; set { settingsJSON.taxPath = value; } }
        public static string sgkPath { get => settingsJSON.sgkPath; set { settingsJSON.sgkPath = value; } }
        public static bool overwrite { get => settingsJSON.overwrite; set { settingsJSON.overwrite = value; } }
        public static bool copyMode { get => settingsJSON.copyMode; set { settingsJSON.copyMode = value; } }

        public static void load()
        {
            var settingsFile = "settings.json";

            if (File.Exists(settingsFile))
            {
                var jsonString = File.ReadAllText(settingsFile);
                settingsJSON = JsonSerializer.Deserialize<SettingsJSON>(jsonString) ?? new();
            }
        }

        public static void save()
        {
            var jsonString = JsonSerializer.Serialize(settingsJSON);
            File.WriteAllText("settings.json", jsonString);
        }
    }
}
