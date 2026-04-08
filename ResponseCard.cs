using System;

namespace VoiceBookStudio.Models
{
    public class ResponseCard
    {
        public string Id       { get; set; } = Guid.NewGuid().ToString();
        public string Title    { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Content  { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
