using System;
using System.Linq;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Shared SAPI text-sanitization and voice-selection logic used by both
    /// AudioFeedbackService and SystemAnnouncementService, so the two speech
    /// pipelines behave identically and only need to be fixed in one place.
    /// </summary>
    internal static class SpeechTextUtils
    {
        /// <summary>
        /// Strips characters that cause SAPI to mispronounce or stutter:
        /// markdown formatting, parentheses, square brackets. Replaces slashes
        /// with "or" and em-dashes with commas for natural pacing.
        /// </summary>
        public static string SanitizeForSpeech(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Dashes used as separators → comma (natural SAPI pause)
            input = Regex.Replace(input, @"\s+[-–—]+\s+", ", ");

            // Forward slash → "or"
            input = input.Replace("/", " or ");

            // Strip markdown formatting characters
            input = input.Replace("#", "")
                         .Replace("*", "")
                         .Replace("_", "")
                         .Replace("`", "")
                         .Replace("~", "");

            // Strip parentheses and square brackets — keep inner content
            input = Regex.Replace(input, @"[\(\)\[\]]", "");

            // Collapse runs of whitespace created by stripping
            input = Regex.Replace(input, @"  +", " ");

            return input.Trim();
        }

        /// <summary>
        /// Picks the best available SAPI voice in priority order:
        ///   1. Any voice containing "Natural" (Windows 11 neural offline voices)
        ///   2. Aria, Jenny, Guy, Davis  (neural voices by name)
        ///   3. Zira (cleaner female standard voice)
        ///   4. Default voice
        /// </summary>
        public static void SelectBestSapiVoice(SpeechSynthesizer synth)
        {
            var voices = synth.GetInstalledVoices()
                              .Where(v => v.Enabled)
                              .Select(v => v.VoiceInfo.Name)
                              .ToList();
            if (voices.Count == 0) return;

            string[] priorities =
            [
                "Natural", "Aria", "Jenny", "Guy", "Davis", "Jane",
                "Jason", "Zira", "Hazel", "Susan"
            ];

            foreach (var pref in priorities)
            {
                var match = voices.FirstOrDefault(
                    v => v.Contains(pref, StringComparison.OrdinalIgnoreCase));
                if (match != null) { synth.SelectVoice(match); return; }
            }
        }
    }
}
