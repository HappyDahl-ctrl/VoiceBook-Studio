using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.Services
{
    public class FeedbackLibraryService
    {
        private static readonly string DefaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VoiceBookStudio", "Feedback", "feedback.json");

        // Maps feedbackType string → (letter, display name)
        private static readonly Dictionary<string, (string Letter, string Name)> TypeMap = new()
        {
            ["comprehensive"] = ("A", "Comprehensive"),
            ["book"]          = ("A", "Comprehensive"),
            ["pacing"]        = ("B", "Pacing"),
            ["dialogue"]      = ("C", "Dialogue"),
            ["style"]         = ("D", "Style"),
            ["structure"]     = ("E", "Structure"),
        };

        // All known category letters and names (A–E fixed).
        public static readonly IReadOnlyDictionary<string, string> CategoryNames =
            new Dictionary<string, string>
            {
                ["A"] = "Comprehensive",
                ["B"] = "Pacing",
                ["C"] = "Dialogue",
                ["D"] = "Style",
                ["E"] = "Structure",
            };

        private string _path;

        public FeedbackLibraryService(string? path = null) => _path = path ?? DefaultPath;

        public void SetProjectFolder(string projectFolderPath)
        {
            _path = string.IsNullOrEmpty(projectFolderPath)
                ? DefaultPath
                : Path.Combine(projectFolderPath, "Feedback", "feedback.json");
        }

        public List<FeedbackEntry> Load()
        {
            if (!File.Exists(_path)) return new();
            try
            {
                var entries = JsonSerializer.Deserialize<List<FeedbackEntry>>(
                    File.ReadAllText(_path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new();

                // Sanitize any null fields that could come from old JSON saved without
                // defaults (older versions wrote explicit nulls for empty fields).
                foreach (var e in entries)
                {
                    e.Id             ??= string.Empty;
                    e.CategoryLetter ??= "A";
                    e.ChapterTitle   ??= string.Empty;
                    e.Text           ??= string.Empty;
                }
                return entries;
            }
            catch { return new(); }
        }

        public void Save(List<FeedbackEntry> entries)
        {
            string? dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(_path, JsonSerializer.Serialize(entries,
                new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>Creates a new entry with the correct letter and next sequential number.</summary>
        public FeedbackEntry CreateEntry(string feedbackType, string chapterTitle, string text,
                                          List<FeedbackEntry> existing)
        {
            var (letter, _) = TypeMap.GetValueOrDefault(feedbackType.ToLower(), ("A", "Comprehensive"));
            int nextNum = existing.Count(e => e.CategoryLetter == letter) + 1;
            return new FeedbackEntry
            {
                Id             = $"{letter}{nextNum}",
                CategoryLetter = letter,
                ChapterTitle   = chapterTitle,
                CreatedAt      = DateTime.Now,
                Text           = text
            };
        }

        public static (string Letter, string Name) GetCategory(string feedbackType) =>
            TypeMap.GetValueOrDefault(feedbackType.ToLower(), ("A", "Comprehensive"));
    }
}
