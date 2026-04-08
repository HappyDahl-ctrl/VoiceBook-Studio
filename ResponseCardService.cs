using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.Services
{
    public class ResponseCardService
    {
        // User data lives in %APPDATA%\VoiceBookStudio\ResponseCards\cards.json
        // so it survives app reinstalls and stays outside the (possibly read-only) install dir.
        private static readonly string DefaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VoiceBookStudio", "ResponseCards", "cards.json");

        private readonly string _path;

        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public ResponseCardService(string? path = null)
        {
            _path = path ?? DefaultPath;
        }

        public List<ResponseCard> Load()
        {
            if (!File.Exists(_path)) return new List<ResponseCard>();

            try
            {
                string json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<List<ResponseCard>>(json, _opts)
                       ?? new List<ResponseCard>();
            }
            catch
            {
                return new List<ResponseCard>();
            }
        }

        public void Save(IEnumerable<ResponseCard> cards)
        {
            string? dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(cards, _opts);
            File.WriteAllText(_path, json);
        }
    }
}
