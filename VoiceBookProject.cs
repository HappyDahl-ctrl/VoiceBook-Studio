using System;
using System.Collections.Generic;
using System.Linq;

namespace VoiceBookStudio.Models
{
    /// <summary>
    /// Represents a VoiceBook project containing an ordered list of chapters.
    /// Serialised to .vbk files (JSON format).
    /// </summary>
    public class VoiceBookProject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = "Untitled Project";
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
        public string SchemaVersion { get; set; } = "1.0";
        public List<BookChapter> Chapters { get; set; } = new();

        public int TotalWordCount => Chapters.Sum(c => c.WordCount);

        public void MarkModified()
        {
            ModifiedAt = DateTime.Now;
        }

        /// <summary>
        /// Re-assigns SortOrder values to match current list positions.
        /// Call after any reorder operation.
        /// </summary>
        public void NormaliseSortOrder()
        {
            for (int i = 0; i < Chapters.Count; i++)
                Chapters[i].SortOrder = i;
        }
    }
}
