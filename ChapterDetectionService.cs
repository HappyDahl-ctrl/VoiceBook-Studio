using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Detects chapter boundaries in plain text (and paragraph + style metadata when available).
    /// Uses heuristic pattern matching first, then falls back to AI when needed.
    /// </summary>
    public class ChapterDetectionService
    {
        private static readonly Regex[] ChapterPatterns = new Regex[]
        {
            // Matches: Chapter 1, CHAPTER 1, Chapter One
            new(@"^\s*chapter\s+\d+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"^\s*chapter\s+\w+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // Matches: Ch. 1, Ch 1, Part 1
            new(@"^\s*(ch\.?|part)\s+\d+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // Standalone uppercase lines (e.g. "THE JOURNEY")
            new(@"^[A-Z ]{3,}$", RegexOptions.Compiled),
            // Lines that are a number followed by text: "1. The Beginning"
            new(@"^\s*\d+\.?\s+\S.+$", RegexOptions.Compiled),
        };

        /// <summary>
        /// Attempts to find chapter breaks using simple heuristics on paragraph-level text.
        /// Accepts an array of paragraphs (plain text) and optional styles per paragraph.
        /// Returns a list of detected chapters (title and starting paragraph index).
        /// </summary>
        public List<(string Title, int StartIndex)> DetectByPatterns(List<ParagraphData> paragraphs)
        {
            var results = new List<(string Title, int StartIndex)>();

            for (int i = 0; i < paragraphs.Count; i++)
            {
                string text = paragraphs[i].Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text)) continue;

                // Heading styles from Word: Heading1, Heading2
                if (!string.IsNullOrWhiteSpace(paragraphs[i].Style))
                {
                    var st = paragraphs[i].Style!.ToLowerInvariant();
                    if (st.Contains("heading 1") || st.Contains("heading1") || st.Contains("heading 2") || st.Contains("heading2"))
                    {
                        results.Add((text, i));
                        continue;
                    }
                }

                foreach (var rx in ChapterPatterns)
                {
                    if (rx.IsMatch(text))
                    {
                        // Use short title (strip trailing digits and punctuation)
                        string title = text;
                        // If title is an indexed line like "1. The Beginning" remove leading number
                        title = Regex.Replace(title, "^\\s*\\d+\\.?\\s*", "");
                        results.Add((title, i));
                        break;
                    }
                }
            }

            // Deduplicate results that are too close together
            var dedup = results.OrderBy(r => r.StartIndex).ToList();
            var final = new List<(string Title, int StartIndex)>();
            int? last = null;
            foreach (var r in dedup)
            {
                if (last != null && r.StartIndex - last.Value <= 1) continue;
                final.Add(r);
                last = r.StartIndex;
            }

            return final;
        }
    }
}
