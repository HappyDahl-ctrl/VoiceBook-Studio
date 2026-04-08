using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Handles saving and loading VoiceBook projects to/from .vbk files.
    /// Format is UTF-8 JSON, human-readable and version-tagged.
    /// </summary>
    public class ProjectService
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public const string FileFilter = "VoiceBook Project (*.vbk)|*.vbk|All Files (*.*)|*.*";
        public const string FileExtension = ".vbk";

        /// <summary>
        /// Saves a project to the specified file path.
        /// </summary>
        public async Task SaveAsync(VoiceBookProject project, string filePath)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path required.", nameof(filePath));

            project.MarkModified();
            project.NormaliseSortOrder();

            string directory = Path.GetDirectoryName(filePath)!;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(project, _options);
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// Loads a project from the specified file path.
        /// </summary>
        public async Task<VoiceBookProject> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Project file not found: {filePath}");

            string json = await File.ReadAllTextAsync(filePath);
            var project = JsonSerializer.Deserialize<VoiceBookProject>(json, _options)
                ?? throw new InvalidDataException("Project file could not be read.");

            // Ensure chapters are sorted correctly after load.
            project.Chapters.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return project;
        }

        /// <summary>
        /// Creates a new blank project with a default first chapter.
        /// </summary>
        public VoiceBookProject CreateNew(string title = "Untitled Project")
        {
            var project = new VoiceBookProject { Title = title };
            project.Chapters.Add(new BookChapter
            {
                Title = "Chapter 1",
                SortOrder = 0
            });
            return project;
        }

        /// <summary>
        /// Scans the given projects folder for recent projects (.vbsproj files).
        /// Returns an ordered list (most recently modified first).
        /// </summary>
        public System.Collections.Generic.List<Models.ProjectInfo> GetRecentProjects(string folderPath)
        {
            var list = new System.Collections.Generic.List<Models.ProjectInfo>();
            if (string.IsNullOrWhiteSpace(folderPath)) return list;

            try
            {
                if (!System.IO.Directory.Exists(folderPath))
                    System.IO.Directory.CreateDirectory(folderPath);

                var files = System.IO.Directory.GetFiles(folderPath, "*.vbsproj");
                foreach (var f in files)
                {
                    var info = new System.IO.FileInfo(f);
                    list.Add(new Models.ProjectInfo
                    {
                        Name = System.IO.Path.GetFileNameWithoutExtension(f),
                        Path = f,
                        LastModified = info.LastWriteTime
                    });
                }

                list.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));
            }
            catch { }

            return list;
        }
    }
}
