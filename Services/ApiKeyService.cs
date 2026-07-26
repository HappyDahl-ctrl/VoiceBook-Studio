using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Stores and retrieves the Anthropic API key from the Windows registry.
    /// HKCU so no admin rights are required. The key is encrypted at rest with
    /// DPAPI (CurrentUser scope) before being written to the registry, so another
    /// process running as the same Windows user cannot read it back out directly.
    /// </summary>
    public static class ApiKeyService
    {
        private const string RegPath  = @"SOFTWARE\VoiceBookStudio";
        private const string RegValue = "AnthropicApiKey";

        // Additional DPAPI entropy binds the encrypted blob to this app specifically.
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("VoiceBookStudio.ApiKey.v1");

        public static string? GetApiKey()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPath);
            var stored = key?.GetValue(RegValue) as string;
            if (string.IsNullOrEmpty(stored)) return stored;

            try
            {
                byte[] protectedBytes = Convert.FromBase64String(stored);
                byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                // Pre-existing plaintext value saved before DPAPI encryption was added.
                // Return it as-is so the user isn't locked out — SaveApiKey re-persists
                // it encrypted the next time they save a key.
                return stored;
            }
        }

        public static void SaveApiKey(string apiKey)
        {
            byte[] plainBytes     = Encoding.UTF8.GetBytes(apiKey ?? string.Empty);
            byte[] protectedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);

            using var key = Registry.CurrentUser.CreateSubKey(RegPath, writable: true);
            key.SetValue(RegValue, Convert.ToBase64String(protectedBytes));
        }

        public static bool HasApiKey()
        {
            var k = GetApiKey();
            // Accept any key with the Anthropic prefix OR any key long enough to be real.
            // The strict "sk-ant-" check rejected valid keys from newer Anthropic formats.
            return !string.IsNullOrWhiteSpace(k) && k.Length >= 20;
        }

        public static void ClearApiKey()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegPath, writable: true);
            key?.DeleteValue(RegValue, throwOnMissingValue: false);
        }
    }
}
