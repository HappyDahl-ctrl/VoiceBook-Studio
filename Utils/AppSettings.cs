using Microsoft.Win32;
using System;
using System.IO;
using System.Text.Json;

namespace VoiceBookStudio.Utils
{
    /// <summary>
    /// Application-wide settings.
    /// Runtime flags are held in memory only.
    /// User preferences (welcome/tutorial/Azure) are persisted to the Windows Registry.
    /// DefaultProjectFolder is persisted to %APPDATA%\VoiceBookStudio\settings.json
    /// so that it is accessible to the folder-structure logic without Registry overhead.
    /// </summary>
    public static partial class AppSettings
    {
        private const string RegKey = @"SOFTWARE\VoiceBookStudio";

        // Path to the JSON settings file (default project folder, etc.)
        private static string JsonSettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VoiceBookStudio", "settings.json");

        // ----------------------------------------------------------------
        // Runtime flags (never persisted)
        // ----------------------------------------------------------------

        /// <summary>True when JAWS is running at the moment the app started.</summary>
        public static bool IsJawsDetected { get; set; } = false;

        /// <summary>True when Dragon NaturallySpeaking (natspeak.exe) is running at startup.</summary>
        public static bool IsDragonRunning { get; set; } = false;

        /// <summary>True when J-Say is running at the moment the app started.</summary>
        public static bool IsJSayDetected { get; set; } = false;

        // ----------------------------------------------------------------
        // Persisted preferences
        // ----------------------------------------------------------------

        /// <summary>
        /// Show the welcome / tutorial dialog on startup.
        /// Persisted so the user's "don't show again" choice survives restarts.
        /// </summary>
        public static bool ShowWelcomeOnStartup { get; set; } = true;

        /// <summary>Whether the tutorial has been completed at least once.</summary>
        public static bool TutorialCompleted { get; set; } = false;

        // ----------------------------------------------------------------
        // Derived paths (not persisted — always computed fresh)
        // ----------------------------------------------------------------

        /// <summary>Fallback folder when no DefaultProjectFolder is set.</summary>
        public static string ProjectsFolder { get; set; } =
            System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                "VoiceBook Projects");

        // ----------------------------------------------------------------
        // Default project folder (persisted to JSON)
        // ----------------------------------------------------------------

        /// <summary>
        /// User-chosen folder where new projects are saved automatically.
        /// Empty string = not set (fall back to prompting the user on each save).
        /// Persisted to %APPDATA%\VoiceBookStudio\settings.json.
        /// </summary>
        public static string DefaultProjectFolder { get; set; } = string.Empty;

        /// <summary>
        /// True once the welcome dialog has been shown on the very first launch.
        /// When false on startup, the welcome/tutorial flow is triggered.
        /// Persisted to %APPDATA%\VoiceBookStudio\settings.json.
        /// </summary>
        public static bool FirstLaunchComplete { get; set; } = false;

        // ----------------------------------------------------------------
        // Persistence helpers
        // ----------------------------------------------------------------

        // ----------------------------------------------------------------
        // Azure TTS settings (persisted)
        // ----------------------------------------------------------------

        /// <summary>Azure Cognitive Services Speech subscription key. Empty = use SAPI.</summary>
        public static string AzureSpeechKey    { get; set; } = string.Empty;

        /// <summary>Azure region, e.g. "eastus". Required when key is set.</summary>
        public static string AzureSpeechRegion { get; set; } = string.Empty;

        /// <summary>Azure neural voice name, e.g. "en-US-AriaNeural".</summary>
        public static string AzureVoiceName    { get; set; } = "en-US-AriaNeural";

        /// <summary>True when Azure TTS is fully configured.</summary>
        public static bool IsAzureTtsConfigured =>
            !string.IsNullOrWhiteSpace(AzureSpeechKey) &&
            !string.IsNullOrWhiteSpace(AzureSpeechRegion);

        // ----------------------------------------------------------------
        // Persistence
        // ----------------------------------------------------------------

        public static void Load()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegKey);
            if (key == null) return;

            ShowWelcomeOnStartup = (int)(key.GetValue("ShowWelcomeOnStartup", 1)!) != 0;
            TutorialCompleted    = (int)(key.GetValue("TutorialCompleted",    0)!) != 0;

            AzureSpeechKey    = (string)(key.GetValue("AzureSpeechKey",    string.Empty)!);
            AzureSpeechRegion = (string)(key.GetValue("AzureSpeechRegion", string.Empty)!);
            AzureVoiceName    = (string)(key.GetValue("AzureVoiceName",    "en-US-AriaNeural")!);
        }

        public static void Save()
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegKey);
            key.SetValue("ShowWelcomeOnStartup", ShowWelcomeOnStartup ? 1 : 0, RegistryValueKind.DWord);
            key.SetValue("TutorialCompleted",    TutorialCompleted    ? 1 : 0, RegistryValueKind.DWord);
            key.SetValue("AzureSpeechKey",       AzureSpeechKey,    RegistryValueKind.String);
            key.SetValue("AzureSpeechRegion",    AzureSpeechRegion, RegistryValueKind.String);
            key.SetValue("AzureVoiceName",       AzureVoiceName,    RegistryValueKind.String);
        }

        // ----------------------------------------------------------------
        // JSON settings (DefaultProjectFolder)
        // ----------------------------------------------------------------

        /// <summary>
        /// Loads DefaultProjectFolder from %APPDATA%\VoiceBookStudio\settings.json.
        /// Safe to call even if the file does not exist yet.
        /// </summary>
        public static void LoadJsonSettings()
        {
            try
            {
                string path = JsonSettingsPath;
                if (!File.Exists(path)) return;

                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                if (doc.RootElement.TryGetProperty("defaultProjectFolder", out JsonElement el))
                    DefaultProjectFolder = el.GetString() ?? string.Empty;
                if (doc.RootElement.TryGetProperty("firstLaunchComplete", out JsonElement flc))
                    FirstLaunchComplete = flc.GetBoolean();
            }
            catch { /* missing or corrupt JSON — use default (empty string) */ }
        }

        /// <summary>
        /// Persists DefaultProjectFolder to %APPDATA%\VoiceBookStudio\settings.json.
        /// Creates the directory if it does not exist.
        /// </summary>
        public static void SaveJsonSettings()
        {
            try
            {
                string path = JsonSettingsPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(
                    new { defaultProjectFolder = DefaultProjectFolder, firstLaunchComplete = FirstLaunchComplete }, options);
                File.WriteAllText(path, json);
            }
            catch { /* non-fatal — setting will revert to empty on next launch */ }
        }
    }
}
