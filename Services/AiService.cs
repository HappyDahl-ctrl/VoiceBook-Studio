using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Calls the Anthropic Messages API (claude-sonnet-4-6) for writing feedback and chat.
    /// API key is loaded from ApiKeyService at call time so changes take effect immediately.
    /// </summary>
    public class AiService
    {
        private const string ApiUrl    = "https://api.anthropic.com/v1/messages";
        private const string Model     = "claude-sonnet-4-6";
        private const string AnthVer   = "2023-06-01";
        private const int    MaxTokens = 2048;

        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        public bool IsAvailable => ApiKeyService.HasApiKey();

        // ----------------------------------------------------------------
        // Public API
        // ----------------------------------------------------------------

        /// <summary>
        /// Returns editor feedback for the given chapter text.
        /// feedbackType: comprehensive | pacing | dialogue | style | structure
        /// bookContext: formatted summary of all other sections (titles + previews)
        ///   so Claude understands the book's continuity while focusing on this chapter.
        /// </summary>
        public async Task<AiFeedback> GetFeedbackAsync(string chapterText,
                                                        string feedbackType = "comprehensive",
                                                        string? bookContext = null)
        {
            string prompt = BuildFeedbackPrompt(chapterText, feedbackType, bookContext);
            string raw    = await CallClaudeAsync(prompt);

            return new AiFeedback
            {
                Assessment   = raw,
                RawText      = raw,
                FeedbackType = feedbackType
            };
        }

        /// <summary>
        /// Analyses the full manuscript — arc, character consistency, pacing,
        /// continuity, and themes across all chapters.
        /// fullBookContent should be every section concatenated in document order.
        /// </summary>
        public async Task<AiFeedback> GetBookFeedbackAsync(string fullBookContent, string bookTitle)
        {
            string prompt = $"""
                You are a professional developmental editor reviewing a complete manuscript.

                BOOK: {bookTitle}

                FULL MANUSCRIPT:
                {fullBookContent}

                Provide book-level developmental feedback. Structure your response with these exact headers:

                OVERALL ASSESSMENT:
                How well does the manuscript work as a whole? What is its greatest strength?

                STORY ARC:
                Does the narrative have a satisfying beginning, middle, and end?
                Where does the arc succeed or need strengthening?

                CHARACTER CONSISTENCY:
                Are characters portrayed consistently across chapters?
                Note any contradictions in voice, motivation, or behaviour.

                PACING:
                Is momentum balanced across the whole book?
                Identify sections that drag or rush and suggest fixes.

                PLOT AND CONTINUITY:
                Flag any plot holes, contradictions, or continuity errors between chapters.

                THEMES:
                How effectively are the central themes developed and carried through?

                KEY RECOMMENDATIONS:
                The 3-5 most important changes that would strengthen the whole manuscript,
                in priority order.

                ENCOURAGEMENT:
                A brief motivating note for the writer.
                """;

            string raw = await CallClaudeAsync(prompt, maxTokens: 4096);

            return new AiFeedback
            {
                Assessment   = raw,
                RawText      = raw,
                FeedbackType = "book"
            };
        }

        /// <summary>
        /// Returns an improved version of a passage together with reasoning.
        /// </summary>
        public async Task<AiSuggestion> SuggestImprovementAsync(string passage, string issue)
        {
            string prompt = $"""
                You are a professional fiction editor. A writer wants to improve the following passage.

                PASSAGE:
                {passage}

                IDENTIFIED ISSUE: {issue}

                Provide:
                IMPROVED VERSION:
                [rewrite that fixes the issue]

                CHANGES MADE:
                [brief bullet list of what changed and why]
                """;

            string raw      = await CallClaudeAsync(prompt);
            string improved = ExtractSection(raw, "IMPROVED VERSION:", "CHANGES MADE:");
            string changes  = ExtractSection(raw, "CHANGES MADE:", null);

            return new AiSuggestion
            {
                OriginalText  = passage,
                SuggestedText = improved.Trim(),
                Reasoning     = changes.Trim()
            };
        }

        /// <summary>
        /// Sends a free-form chat message.
        /// chapterContext: full text of the chapter currently open in the editor.
        /// bookContext: formatted summary of all other sections so Claude understands
        ///   the book's continuity while keeping suggestions focused on the open chapter.
        /// </summary>
        public async Task<string> ChatAsync(string userMessage,
                                            string? chapterContext = null,
                                            string? bookContext    = null)
        {
            bool hasChapter = !string.IsNullOrWhiteSpace(chapterContext);
            bool hasBook    = !string.IsNullOrWhiteSpace(bookContext);

            if (!hasChapter && !hasBook)
                return await CallClaudeAsync(userMessage);

            var sb = new StringBuilder();
            sb.AppendLine("You are a professional writing assistant.");
            sb.AppendLine();
            sb.AppendLine(
                "IMPORTANT: When the user asks you to produce, rewrite, suggest, or generate any " +
                "content, return only the content itself with no introduction, no explanation, " +
                "and no closing remarks. Begin the response with the first word of the content " +
                "and end it with the last word of the content. " +
                "The only exception is when the user asks you a direct question, in which case a " +
                "conversational answer is appropriate.");
            sb.AppendLine();

            if (hasBook)
            {
                sb.AppendLine("BOOK OVERVIEW (for context and continuity — do not rewrite other sections):");
                sb.AppendLine(bookContext);
                sb.AppendLine();
            }

            if (hasChapter)
            {
                sb.AppendLine("CURRENT CHAPTER (the section open in the editor — focus all suggestions here):");
                sb.AppendLine("---");
                sb.AppendLine(chapterContext);
                sb.AppendLine("---");
                sb.AppendLine();
            }

            sb.AppendLine($"Writer's question: {userMessage}");

            return await CallClaudeAsync(sb.ToString());
        }

        /// <summary>
        /// Detects natural chapter breaks in a block of plain text (e.g. from a .docx import).
        /// Returns a list of suggested chapters with titles and content, even when the source
        /// has no headings. Falls back to a single chapter if parsing fails.
        /// </summary>
        public async Task<List<DetectedChapter>> DetectChaptersAsync(string fullText)
        {
            string prompt = $$"""
                You are helping import a Word document into a book editor.
                Analyze this document and identify natural chapter breaks.
                Look for scene changes, time jumps, perspective shifts, and topic changes.

                Return ONLY valid JSON with no explanation, no markdown code fences, in exactly this format:
                [
                  {"title": "Suggested Chapter Title", "startText": "The opening text of the chapter...", "reason": "Detected because..."}
                ]

                Rules:
                - If the text is best left as a single chapter, return a single-item array.
                - Titles should be concise (3-8 words).
                - startText should contain the opening ~120 characters of the chapter.
                - reason should be a short justification (one sentence).

                DOCUMENT:
                {{fullText}}
                """;

            string raw = await CallClaudeAsync(prompt, maxTokens: 8000);

            // Strip possible markdown code fences that Claude may add despite instructions
            raw = raw.Trim();
            if (raw.StartsWith("```"))
            {
                int newline = raw.IndexOf('\n');
                int closing = raw.LastIndexOf("```");
                if (newline >= 0 && closing > newline)
                    raw = raw[(newline + 1)..closing].Trim();
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list    = JsonSerializer.Deserialize<List<DetectedChapterDto>>(raw, options);
                if (list == null || list.Count == 0)
                    return SingleChapterFallback(fullText);

                // Use startText as a positional marker to slice real content out of fullText.
                // This avoids the bug where Content would only be the opening ~120 chars.
                var positioned = new List<(string Title, int Pos)>();
                foreach (var d in list)
                {
                    string marker = d.StartText?.Trim() ?? string.Empty;
                    int pos = string.IsNullOrWhiteSpace(marker)
                        ? -1
                        : fullText.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                    positioned.Add((
                        string.IsNullOrWhiteSpace(d.Title) ? "Untitled Chapter" : d.Title,
                        pos));
                }

                // If no markers were found fall back to single chapter
                var found = positioned.Where(m => m.Pos >= 0).OrderBy(m => m.Pos).ToList();
                if (found.Count == 0)
                    return SingleChapterFallback(fullText);

                var chapters = new List<DetectedChapter>();
                for (int i = 0; i < found.Count; i++)
                {
                    int start = found[i].Pos;
                    int end   = i + 1 < found.Count ? found[i + 1].Pos : fullText.Length;
                    chapters.Add(new DetectedChapter
                    {
                        Title   = found[i].Title,
                        Content = fullText[start..end].Trim()
                    });
                }
                return chapters;
            }
            catch
            {
                return SingleChapterFallback(fullText);
            }
        }

        private static List<DetectedChapter> SingleChapterFallback(string text) =>
            new() { new DetectedChapter { Title = "Imported Chapter", Content = text } };

        // ----------------------------------------------------------------
        // HTTP layer
        // ----------------------------------------------------------------

        private static async Task<string> CallClaudeAsync(string userMessage, int maxTokens = MaxTokens)
        {
            string? apiKey = ApiKeyService.GetApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Anthropic API key is not set.");

            var body = new
            {
                model      = Model,
                max_tokens = maxTokens,
                messages   = new[] { new { role = "user", content = userMessage } }
            };

            string json = JsonSerializer.Serialize(body);

            // Use per-request message so the API key is sent on each call without
            // mutating the shared client's DefaultRequestHeaders.
            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", AnthVer);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage resp = await _httpClient.SendAsync(request);

            if (!resp.IsSuccessStatusCode)
            {
                string err = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Anthropic API error {(int)resp.StatusCode}: {err}");
            }

            string respJson = await resp.Content.ReadAsStringAsync();
            using var doc   = JsonDocument.Parse(respJson);
            return doc.RootElement
                      .GetProperty("content")[0]
                      .GetProperty("text")
                      .GetString() ?? string.Empty;
        }

        // ----------------------------------------------------------------
        // Prompt templates
        // ----------------------------------------------------------------

        private static string BuildFeedbackPrompt(string text, string type,
                                                    string? bookContext = null)
        {
            // If we have book context, prepend it so Claude understands continuity
            // but always focuses feedback on the current chapter only.
            string bookSection = string.IsNullOrWhiteSpace(bookContext) ? string.Empty :
                $"""
                BOOK OVERVIEW (for continuity awareness — focus feedback on the current chapter only):
                {bookContext}

                """;

            return type switch
            {
            "pacing" => $"""
                {bookSection}You are a professional fiction editor specialising in narrative pacing.
                Analyse the following chapter for pacing issues, keeping the book's overall
                arc in mind but commenting only on this chapter.

                Identify where it drags, where it rushes, the overall rhythm,
                and give specific actionable suggestions.

                CURRENT CHAPTER:
                {text}
                """,

            "dialogue" => $"""
                {bookSection}You are a professional fiction editor specialising in dialogue.
                Analyse the following chapter for dialogue quality.
                Use the book overview above to judge character voice consistency across the book.

                Assess: naturalness, character voice distinction, dialogue tags,
                whether each line advances plot or character.

                CURRENT CHAPTER:
                {text}
                """,

            "style" => $"""
                {bookSection}You are a professional copy editor.
                Analyse the following chapter for prose style issues.
                Note any style inconsistencies with the rest of the book where relevant.

                Flag: repeated words, passive voice, weak verbs, adverb overuse,
                sentence length variety, clichés.

                CURRENT CHAPTER:
                {text}
                """,

            "structure" => $"""
                {bookSection}You are a professional developmental editor.
                Analyse the following chapter for structural clarity.
                Consider how this chapter connects to the rest of the book.

                Examine: opening hook, scene transitions, paragraph purpose,
                chapter ending, any sections to split or merge.

                CURRENT CHAPTER:
                {text}
                """,

            _ => $"""
                {bookSection}You are a professional fiction editor giving comprehensive feedback.
                Use the book overview above to inform continuity comments,
                but keep all feedback focused on the current chapter.

                Structure your response with these exact headers:

                OVERVIEW:
                2-3 sentence overall assessment, noting how this chapter fits the book.

                STRENGTHS:
                3-5 specific things working well with examples.

                QUICK WINS:
                3-5 easy improvements the writer can make right now.

                IMPROVEMENTS:
                Most important issues ordered by priority, with specific suggestions.

                ENCOURAGEMENT:
                A brief motivating note.

                CURRENT CHAPTER:
                {text}
                """
            };
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        private static string ExtractSection(string text, string startHeader, string? endHeader)
        {
            int start = text.IndexOf(startHeader, StringComparison.OrdinalIgnoreCase);
            if (start < 0) return text;
            start += startHeader.Length;

            if (endHeader == null) return text[start..];

            int end = text.IndexOf(endHeader, start, StringComparison.OrdinalIgnoreCase);
            return end < 0 ? text[start..] : text[start..end];
        }
    }

    // ----------------------------------------------------------------
    // Data classes
    // ----------------------------------------------------------------

    public class AiFeedback
    {
        public string Assessment   { get; set; } = string.Empty;
        public string RawText      { get; set; } = string.Empty;
        public string FeedbackType { get; set; } = string.Empty;
        public bool   IsStub       { get; set; }
    }

    public class AiSuggestion
    {
        public string OriginalText  { get; set; } = string.Empty;
        public string SuggestedText { get; set; } = string.Empty;
        public string Reasoning     { get; set; } = string.Empty;
        public bool   IsStub        { get; set; }
    }

    /// <summary>
    /// A chapter detected by Claude during .docx import.
    /// </summary>
    public class DetectedChapter
    {
        public string Title   { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// JSON DTO matching Claude's returned array items.
    /// </summary>
    internal sealed class DetectedChapterDto
    {
        [JsonPropertyName("title")]
        public string? Title   { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("startText")]
        public string? StartText { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}
