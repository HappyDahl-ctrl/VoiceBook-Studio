using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace VoiceBookStudio.Utils
{
    /// <summary>
    /// Locates a passage of text Claude identified as a "replace this" target inside the
    /// live chapter text. Tries an exact match first, then falls back to a whitespace-
    /// tolerant match — Claude occasionally normalizes line breaks or spacing when it
    /// copies a passage back, even when told to copy verbatim.
    /// </summary>
    public static class TextLocator
    {
        public static bool TryFind(string haystack, string target, out int start, out int length)
        {
            start  = -1;
            length = 0;
            if (string.IsNullOrEmpty(haystack) || string.IsNullOrEmpty(target)) return false;

            int exact = haystack.IndexOf(target, StringComparison.Ordinal);
            if (exact >= 0)
            {
                start  = exact;
                length = target.Length;
                return true;
            }

            string[] words = target.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return false;

            string pattern = string.Join(@"\s+", words.Select(Regex.Escape));
            Match  match   = Regex.Match(haystack, pattern, RegexOptions.Singleline);
            if (!match.Success) return false;

            start  = match.Index;
            length = match.Length;
            return true;
        }
    }
}
