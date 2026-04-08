using System;
using System.Text.Json.Serialization;

namespace VoiceBookStudio.Models
{
    /// <summary>
    /// Represents a single chapter in a VoiceBook project.
    /// </summary>
    public class BookChapter
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "New Chapter";
        public string Content { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        /// <summary>
        /// Structural type of this section (front matter, body chapter, back matter).
        /// Defaults to Chapter for backwards compatibility with existing .vbk files.
        /// </summary>
        public SectionType SectionType { get; set; } = SectionType.Chapter;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        [JsonIgnore]
        public int WordCount => string.IsNullOrWhiteSpace(Content)
            ? 0
            : Content.Split(new[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries).Length;

        public void MarkModified()
        {
            ModifiedAt = DateTime.Now;
        }

        public override string ToString() => Title;
    }
}
