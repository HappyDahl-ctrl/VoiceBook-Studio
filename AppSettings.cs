using Microsoft.Win32;

namespace VoiceBookStudio.Utils
{
    /// <summary>
    /// Application-wide settings. Runtime flags are held in memory;
    /// user preferences are persisted to the Windows registry so they
    /// survive restarts.
    /// </summary>
    public static partial class AppSettings
    {
        private const string RegKey = @"SOFTWARE\VoiceBookStudio";

        // ----------------------------------------------------------------
        // Runtime flags (never persisted)
        // ----------------------------------------------------------------

        /// <summary>True when JAWS is running at the moment the app started.</summary>
        public static bool IsJawsDetected { get; set; } = false;

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

        /// <summary>Folder where .vbk projects are saved by default.</summary>
        public static string ProjectsFolder { get; set; } =
            System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
                "VoiceBook Projects");

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
    }
}
