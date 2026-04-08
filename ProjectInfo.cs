using System;

namespace VoiceBookStudio.Models
{
    public class ProjectInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }
}