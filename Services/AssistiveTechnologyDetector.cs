using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Detects which assistive technologies are running when VoiceBook starts.
    /// All checks are wrapped in try-catch — GetProcessesByName can throw on
    /// locked-down systems; failures report the technology as not detected.
    /// </summary>
    public static class AssistiveTechnologyDetector
    {
        /// <summary>True when JAWS screen reader is running.</summary>
        public static bool IsJawsRunning()
        {
            try
            {
                if (Process.GetProcessesByName("jfw").Length   > 0
                    || Process.GetProcessesByName("jaws").Length > 0
                    || Process.GetProcessesByName("jfw64").Length > 0
                    || Process.GetProcessesByName("fsjaws").Length > 0)
                    return true;

                // Fallback: scan all running process names for anything containing
                // "jaws" or starting with "jfw". Exact-name checks above cover every
                // JAWS release this app has been tested against, but a future or
                // regional build using a process name we haven't seen would otherwise
                // go completely undetected — with no visible symptom beyond the app
                // quietly narrating over JAWS's own reading. This scan is the backstop
                // for that case; it never throws even if a process exits mid-enumeration.
                foreach (var p in Process.GetProcesses())
                {
                    using (p)
                    {
                        try
                        {
                            string name = p.ProcessName;
                            if (name.StartsWith("jfw", StringComparison.OrdinalIgnoreCase) ||
                                name.Contains("jaws", StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                        catch { /* process exited or is inaccessible — skip it */ }
                    }
                }
                return false;
            }
            catch { return false; }
        }

        /// <summary>
        /// Checks for JAWS up to 3 times, waiting 2000 ms between attempts.
        /// Returns true as soon as jfw.exe or jaws.exe is found; false only after
        /// all 3 attempts have failed.
        ///
        /// Why: JAWS 2026 requires a Vispero account sign-in on first run. The
        /// jfw.exe process may not appear for several seconds after the OS desktop
        /// is visible. Three retries with 2-second gaps covers the typical sign-in
        /// window without making startup feel slow on machines where JAWS is not
        /// installed.
        /// </summary>
        public static async Task<bool> IsJawsRunningWithRetry()
        {
            const int attempts  = 3;
            const int delayMs   = 2000;

            for (int i = 0; i < attempts; i++)
            {
                if (IsJawsRunning()) return true;
                if (i < attempts - 1)
                    await Task.Delay(delayMs);
            }
            return false;
        }

        /// <summary>True when Dragon speech recognition is running.</summary>
        public static bool IsDragonRunning()
        {
            try
            {
                return Process.GetProcessesByName("natspeak").Length   > 0
                    || Process.GetProcessesByName("natspeak64").Length > 0
                    || Process.GetProcessesByName("dragon").Length     > 0
                    || Process.GetProcessesByName("dragon64").Length   > 0;
            }
            catch { return false; }
        }

        /// <summary>True when J-Say is running.</summary>
        public static bool IsJSayRunning()
        {
            try
            {
                return Process.GetProcessesByName("jsay").Length   > 0
                    || Process.GetProcessesByName("leasey").Length > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Returns a plain-English sentence describing which assistive technologies
        /// were detected and which were not. Suitable for TTS — no abbreviations,
        /// no slashes, no parentheses, no ellipsis.
        /// </summary>
        public static string BuildStartupStatusMessage()
        {
            bool jaws   = IsJawsRunning();
            bool dragon = IsDragonRunning();
            bool jSay   = IsJSayRunning();

            // Only report what IS running — listing absent tools creates noise.
            var parts = new List<string>();
            if (jaws)   parts.Add("JAWS screen reader is running");
            if (dragon) parts.Add("Dragon NaturallySpeaking is running");
            if (jSay)   parts.Add("J-Say is running");

            return parts.Count > 0 ? string.Join(". ", parts) + ". " : string.Empty;
        }
    }
}
