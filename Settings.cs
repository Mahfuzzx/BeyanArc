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
            public bool keepBoth { get; set; } = false;

        }
        private static SettingsJSON settingsJSON = new();
        /// <summary>
        /// Path to the source directory containing the PDF files.
        /// </summary>
        public static string sourcePath { get => settingsJSON.sourcePath; set { settingsJSON.sourcePath = value; } }
        /// <summary>
        /// Path to the directory where tax files will be moved.
        /// </summary>
        public static string taxPath { get => settingsJSON.taxPath; set { settingsJSON.taxPath = value; } }
        /// <summary>
        /// Path to the directory where SGK files will be moved.
        /// </summary>
        public static string sgkPath { get => settingsJSON.sgkPath; set { settingsJSON.sgkPath = value; } }
        public static bool overwrite { get; set; } = false;
        public static bool copyMode { get; set; } = false;
        public static bool keepBoth { get; set; } = false;
        /// <summary>
        /// Loads settings from the 'settings.json' file or creates a new Settings instance.
        /// </summary>
        public static void load()
        {
            var settingsFile = "settings.json";

            if (File.Exists(settingsFile))
            {
                var jsonString = File.ReadAllText(settingsFile);
                settingsJSON = JsonSerializer.Deserialize<SettingsJSON>(jsonString) ?? new();
            }
        }

        /// <summary>
        /// Saves the current settings to the 'settings.json' file.
        /// </summary>
        public static void save()
        {
            var jsonString = JsonSerializer.Serialize(settingsJSON);
            File.WriteAllText("settings.json", jsonString);
        }
    }
}
