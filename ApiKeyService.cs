using Microsoft.Win32;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Stores and retrieves the Anthropic API key from the Windows registry.
    /// HKCU so no admin rights are required.
    /// </summary>
    public static class ApiKeyService
    {
        private const string RegPath  = @"SOFTWARE\VoiceBookStudio";
        private const string RegValue = "AnthropicApiKey";

        public static string? GetApiKey()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPath);
            return key?.GetValue(RegValue) as string;
        }

        public static void SaveApiKey(string apiKey)
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegPath, writable: true);
            key.SetValue(RegValue, apiKey ?? string.Empty);
        }

        public static bool HasApiKey()
        {
            var k = GetApiKey();
            return !string.IsNullOrWhiteSpace(k) && k.StartsWith("sk-ant-");
        }

        public static void ClearApiKey()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPath, writable: true);
            key?.DeleteValue(RegValue, throwOnMissingValue: false);
        }
    }
}
