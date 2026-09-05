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
        // Chapter headings are short titles, never full paragraphs of prose — every
        // pattern below is anchored to a title-length line (see the length guard in
        // DetectByPatterns) specifically so an ordinary sentence that happens to start
        // with a number ("2020 was a strange year.") or an ALL-CAPS exclamation
        // ("STOP RIGHT THERE!") can't masquerade as a chapter break. Real manuscripts
        // triggered exactly that on the unguarded, un-anchored versions of these
        // patterns, producing hundreds of false "chapters" from ordinary body text.
        private const int MaxHeadingLength = 80;

        private static readonly Regex[] ChapterPatterns = new Regex[]
        {
            // Matches: Chapter 1, CHAPTER 1, Chapter One
            new(@"^\s*chapter\s+\d+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"^\s*chapter\s+\w+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // Matches: Ch. 1, Ch 1, Part 1 — followed by nothing or a short title,
            // never a full sentence ("Part of my plan was to move to Paris" must not match).
            new(@"^\s*(ch\.?|part)\s+\d{1,3}\b\s*[:.\-]?\s*[^.!?]{0,40}$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            // Standalone all-caps titles (e.g. "THE JOURNEY BEGINS") — requires at
            // least two words so a single shouted word ("STOP", "NO") isn't mistaken
            // for a title; a real heading is virtually never a lone word.
            new(@"^[A-Z]+(?:\s[A-Z]+){1,7}$", RegexOptions.Compiled),
            // Lines that are a short number followed by a short title: "1. The Beginning".
            // The 1-3 digit cap excludes years ("2020 was..."); excluding . ! ? from the
            // rest of the line excludes full sentences that merely start with a numeral.
            new(@"^\s*\d{1,3}\.?\s+[^.!?]{2,60}$", RegexOptions.Compiled),
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

                // Pattern matching below is for un-styled text, where a chapter heading
                // is only distinguishable from body text by looking title-like. Ordinary
                // paragraphs of prose run far longer than this, so skip them outright
                // rather than risk a pattern matching partway into one.
                if (text.Length > MaxHeadingLength) continue;

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
