using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.Services
{
    public class PromptLibraryService
    {
        private const string DefaultPromptsPath = "Data/PromptLibrary/prompts.json";

        public List<PromptItem> LoadPrompts(string? path = null)
        {
            string p = path ?? DefaultPromptsPath;
            if (!File.Exists(p)) return new List<PromptItem>();
            var json = File.ReadAllText(p);
            return JsonSerializer.Deserialize<List<PromptItem>>(json) ?? new List<PromptItem>();
        }
    }

    public class PromptItem
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
