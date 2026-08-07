using System;

namespace VoiceBookStudio.Models
{
    public class FeedbackEntry
    {
        public string Id            { get; set; } = string.Empty;  // "A1", "B2", etc.
        public string CategoryLetter { get; set; } = "A";           // A–E
        public string ChapterTitle  { get; set; } = string.Empty;

        /// <summary>AI-generated short title summarizing what the feedback is about. Empty for
        /// entries saved before this field existed — falls back to ChapterTitle for display.</summary>
        public string Title         { get; set; } = string.Empty;

        public DateTime CreatedAt   { get; set; } = DateTime.Now;
        public string Text          { get; set; } = string.Empty;
    }
}
