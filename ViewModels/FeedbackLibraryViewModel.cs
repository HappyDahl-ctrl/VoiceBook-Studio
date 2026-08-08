using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceBookStudio.Models;
using VoiceBookStudio.Services;

namespace VoiceBookStudio.ViewModels
{
    public class FeedbackEntryViewModel
    {
        public FeedbackEntry Model { get; }

        public string DisplayLabel => string.IsNullOrWhiteSpace(Model.Title)
            ? $"{Model.Id ?? "?"} — {Model.ChapterTitle ?? "Unknown chapter"}"
            : $"{Model.Id ?? "?"} — {Model.Title}";

        public string DateDisplay =>
            Model.CreatedAt.ToString("MMMM d yyyy, h:mm tt");

        public string PreviewText
        {
            get
            {
                string text = Model.Text ?? string.Empty;
                return text.Length > 300 ? text[..300] + "…" : text;
            }
        }

        public FeedbackEntryViewModel(FeedbackEntry model) { Model = model; }
    }

    public partial class FeedbackLibraryViewModel : ObservableObject
    {
        private readonly FeedbackLibraryService      _service;
        private readonly Action<IEnumerable<string>> _startReading;

        private List<FeedbackEntry> _allEntries = new();

        // ----------------------------------------------------------------
        // Collections
        // ----------------------------------------------------------------

        public ObservableCollection<FeedbackEntryViewModel> Entries    { get; } = new();
        public ObservableCollection<string>                 Categories { get; } = new();

        // ----------------------------------------------------------------
        // Selection
        // ----------------------------------------------------------------

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedEntry))]
        [NotifyPropertyChangedFor(nameof(SelectedEntryPreview))]
        [NotifyCanExecuteChangedFor(nameof(ReadEntryCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteEntryCommand))]
        private FeedbackEntryViewModel? _selectedEntry;

        public bool   HasSelectedEntry     => SelectedEntry != null;
        public string SelectedEntryPreview => SelectedEntry?.PreviewText ?? string.Empty;

        // ----------------------------------------------------------------
        // Category filter
        // ----------------------------------------------------------------

        [ObservableProperty]
        private string _selectedCategory = "All";

        partial void OnSelectedCategoryChanged(string value) => ApplyFilter();

        /// <summary>
        /// Raised for spoken feedback the ViewModel wants announced. MainViewModel
        /// forwards this to LiveAnnounce so JAWS and non-JAWS users both hear it
        /// through the app's single announcement path (avoids double-speech).
        /// </summary>
        public event Action<string>? AnnouncementRequested;

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        public FeedbackLibraryViewModel(FeedbackLibraryService      service,
                                        Action<IEnumerable<string>> startReading)
        {
            _service      = service;
            _startReading = startReading;

            LoadEntries();
        }

        // ----------------------------------------------------------------
        // Public API
        // ----------------------------------------------------------------

        public void Reload() => LoadEntries();

        /// <summary>Called by MainViewModel after every feedback run to auto-persist.</summary>
        public void AddEntry(string feedbackType, string chapterTitle, string text, string title = "")
        {
            var entry = _service.CreateEntry(feedbackType, chapterTitle, text, _allEntries, title);
            _allEntries.Add(entry);
            _service.Save(_allEntries);
            RebuildCategories();
            ApplyFilter();
        }

        public IEnumerable<string> GetCategoryReadingList()
        {
            var items = new List<string>();
            foreach (var kv in FeedbackLibraryService.CategoryNames.OrderBy(kv => kv.Key))
            {
                int count = _allEntries.Count(e => e.CategoryLetter == kv.Key);
                if (count > 0)
                    items.Add($"{kv.Key}: {kv.Value}. {count} {(count == 1 ? "entry" : "entries")}.");
            }
            if (items.Count == 0)
                items.Add("Your feedback library is empty. Run any AI analysis to save feedback here automatically.");
            else
                items.Add("Say Read Feedback followed by a letter to hear entries in any category.");
            return items;
        }

        public IEnumerable<string> GetCategoryEntryList(string letter)
        {
            letter = letter.ToUpper();
            if (!FeedbackLibraryService.CategoryNames.TryGetValue(letter, out string? catName))
                return new[] { $"No feedback category {letter}." };

            var entries = _allEntries
                .Where(e => e.CategoryLetter == letter)
                .OrderBy(e => e.Id)
                .ToList();

            if (entries.Count == 0)
                return new[] { $"No {catName} feedback saved yet." };

            var items = new List<string>();
            foreach (var e in entries)
                items.Add($"{e.Id}: {e.ChapterTitle}, {e.CreatedAt:MMMM d yyyy}.");
            items.Add($"Say Read followed by an ID such as {entries[0].Id} to hear the full feedback text.");
            return items;
        }

        public IEnumerable<string> GetEntryText(string id)
        {
            var entry = _allEntries.FirstOrDefault(e =>
                string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase));

            if (entry == null)
                return new[] { $"No feedback entry {id} found." };

            string catName = FeedbackLibraryService.CategoryNames.GetValueOrDefault(
                entry.CategoryLetter, entry.CategoryLetter);
            string header = $"{entry.Id}: {catName} feedback. {entry.ChapterTitle}. {entry.CreatedAt:MMMM d yyyy}.";

            var chunks = new List<string> { header };
            chunks.AddRange(SplitIntoChunks(entry.Text ?? string.Empty, 600));
            return chunks;
        }

        // ----------------------------------------------------------------
        // Private helpers
        // ----------------------------------------------------------------

        private void LoadEntries()
        {
            _allEntries = _service.Load();
            RebuildCategories();
            ApplyFilter();
        }

        private void RebuildCategories()
        {
            Categories.Clear();
            Categories.Add("All");
            foreach (var kv in FeedbackLibraryService.CategoryNames.OrderBy(k => k.Key))
            {
                if (_allEntries.Any(e => e.CategoryLetter == kv.Key))
                    Categories.Add($"{kv.Key}: {kv.Value}");
            }
        }

        private void ApplyFilter()
        {
            string? prevId = SelectedEntry?.Model.Id;
            Entries.Clear();

            IEnumerable<FeedbackEntry> source = SelectedCategory == "All"
                ? _allEntries
                : _allEntries.Where(e => SelectedCategory.StartsWith(e.CategoryLetter));

            foreach (var e in source.OrderBy(e => e.CategoryLetter).ThenBy(e => e.CreatedAt))
                Entries.Add(new FeedbackEntryViewModel(e));

            SelectedEntry = prevId == null ? null
                : Entries.FirstOrDefault(vm => vm.Model.Id == prevId);
        }

        [RelayCommand(CanExecute = nameof(CanModifyEntry))]
        private void ReadEntry()
        {
            if (SelectedEntry == null) return;
            _startReading(GetEntryText(SelectedEntry.Model.Id));
        }

        [RelayCommand(CanExecute = nameof(CanModifyEntry))]
        private void DeleteEntry()
        {
            if (SelectedEntry == null) return;
            string id = SelectedEntry.Model.Id;
            _allEntries.RemoveAll(e => e.Id == id);
            _service.Save(_allEntries);
            RebuildCategories();
            ApplyFilter();
            AnnouncementRequested?.Invoke("Feedback entry deleted.");
        }

        private bool CanModifyEntry() => SelectedEntry != null;

        private static IEnumerable<string> SplitIntoChunks(string text, int maxChars)
        {
            text ??= string.Empty;
            if (text.Length <= maxChars) return new[] { text };

            var chunks    = new List<string>();
            var sentences = text.Split(new[] { ". ", ".\n", "!\n", "?\n" },
                                       StringSplitOptions.None);
            var current   = new StringBuilder();

            foreach (string sentence in sentences)
            {
                if (current.Length + sentence.Length > maxChars && current.Length > 0)
                {
                    chunks.Add(current.ToString().Trim());
                    current.Clear();
                }
                current.Append(sentence);
                if (!sentence.EndsWith('.') && !sentence.EndsWith('!') && !sentence.EndsWith('?'))
                    current.Append(". ");
            }
            if (current.Length > 0)
                chunks.Add(current.ToString().Trim());

            return chunks.Count > 0 ? chunks : new[] { text };
        }
    }
}
