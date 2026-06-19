using System;

namespace VoiceBookStudio.Models
{
    public class FeedbackEntry
    {
        public string Id            { get; set; } = string.Empty;  // "A1", "B2", etc.
        public string CategoryLetter { get; set; } = "A";           // A–E
        public string ChapterTitle  { get; set; } = string.Empty;
        public DateTime CreatedAt   { get; set; } = DateTime.Now;
        public string Text          { get; set; } = string.Empty;
    }
}
