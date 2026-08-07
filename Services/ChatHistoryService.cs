using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.Services
{
    /// <summary>Persists the AI Assistant chat history. Project-scoped via SetProjectFolder,
    /// same pattern as FeedbackLibraryService/ResponseCardService, so history for one
    /// project/book never appears in another.</summary>
    public class ChatHistoryService
    {
        private static readonly string DefaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VoiceBookStudio", "ChatHistory", "history.json");

        private string _path;

        public ChatHistoryService(string? path = null) => _path = path ?? DefaultPath;

        public void SetProjectFolder(string projectFolderPath)
        {
            _path = string.IsNullOrEmpty(projectFolderPath)
                ? DefaultPath
                : Path.Combine(projectFolderPath, "ChatHistory", "history.json");
        }

        public List<ChatExchange> Load()
        {
            if (!File.Exists(_path)) return new();
            try
            {
                return JsonSerializer.Deserialize<List<ChatExchange>>(
                    File.ReadAllText(_path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            catch { return new(); }
        }

        public void Save(IEnumerable<ChatExchange> exchanges)
        {
            string? dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(_path, JsonSerializer.Serialize(exchanges,
                new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
