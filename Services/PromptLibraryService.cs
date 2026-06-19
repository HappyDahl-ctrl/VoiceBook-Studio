using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            try
            {
                string json = File.ReadAllText(p);
                return JsonSerializer.Deserialize<List<PromptItem>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<PromptItem>();
            }
            catch { return new List<PromptItem>(); }
        }

        /// <summary>
        /// Appends a new prompt to the JSON file and saves.
        /// Auto-generates the next sequential ID within the category letter.
        /// </summary>
        public void AddAndSave(PromptItem newPrompt, string? path = null)
        {
            string p = path ?? DefaultPromptsPath;
            var all = LoadPrompts(p);
            all.Add(newPrompt);
            File.WriteAllText(p, JsonSerializer.Serialize(all,
                new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// Returns the next available ID for the given category letter.
        /// E.g. if A1–A5 exist, returns "A6".
        /// </summary>
        public string NextIdForLetter(string letter, string? path = null)
        {
            letter = letter.ToUpper();
            var all = LoadPrompts(path);
            int max = all
                .Where(p => p.Id.StartsWith(letter, System.StringComparison.OrdinalIgnoreCase))
                .Select(p =>
                {
                    int n = 0;
                    int.TryParse(p.Id[letter.Length..], out n);
                    return n;
                })
                .DefaultIfEmpty(0)
                .Max();
            return $"{letter}{max + 1}";
        }

        /// <summary>
        /// Returns the next unused category letter (first letter not present in the loaded prompts).
        /// </summary>
        public string NextAvailableLetter(string? path = null)
        {
            var all = LoadPrompts(path);
            var usedLetters = all
                .Where(p => p.Id.Length > 0 && char.IsLetter(p.Id[0]))
                .Select(p => char.ToUpper(p.Id[0]))
                .ToHashSet();

            for (char c = 'A'; c <= 'Z'; c++)
                if (!usedLetters.Contains(c))
                    return c.ToString();

            return "Z";
        }

        /// <summary>
        /// Gets distinct categories (letter → name) from the loaded prompts.
        /// Sorted by letter.
        /// </summary>
        public List<(string Letter, string Name)> GetCategories(string? path = null)
        {
            return LoadPrompts(path)
                .Where(p => p.Id.Length > 0 && char.IsLetter(p.Id[0]))
                .GroupBy(p => char.ToUpper(p.Id[0]).ToString())
                .Select(g => (Letter: g.Key, Name: g.First().Category))
                .OrderBy(x => x.Letter)
                .ToList();
        }
    }

    public class PromptItem
    {
        public string Id       { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Title    { get; set; } = string.Empty;
        public string Content  { get; set; } = string.Empty;
    }
}
