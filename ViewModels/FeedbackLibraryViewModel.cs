using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using VoiceBookStudio.Models;
using VoiceBookStudio.Services;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.ViewModels
{
    public class FeedbackEntryViewModel
    {
        public FeedbackEntry Model { get; }

        public string DisplayLabel =>
            $"{Model.Id ?? "?"} — {Model.ChapterTitle ?? "Unknown chapter"}";

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

    public class FeedbackLibraryViewModel : INotifyPropertyChanged
    {
        private readonly FeedbackLibraryService      _service;
        private readonly SystemAnnouncementService   _announcer;
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

        private FeedbackEntryViewModel? _selectedEntry;
        public FeedbackEntryViewModel? SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (_selectedEntry == value) return;
                _selectedEntry = value;
                Notify();
                Notify(nameof(HasSelectedEntry));
                Notify(nameof(SelectedEntryPreview));
                ((RelayCommand)ReadEntryCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteEntryCommand).RaiseCanExecuteChanged();
            }
        }

        public bool   HasSelectedEntry    => _selectedEntry != null;
        public string SelectedEntryPreview => _selectedEntry?.PreviewText ?? string.Empty;

        // ----------------------------------------------------------------
        // Category filter
        // ----------------------------------------------------------------

        private string _selectedCategory = "All";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;
                Notify();
                ApplyFilter();
            }
        }

        // ----------------------------------------------------------------
        // Commands
        // ----------------------------------------------------------------

        public ICommand ReadEntryCommand   { get; }
        public ICommand DeleteEntryCommand { get; }

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        public FeedbackLibraryViewModel(FeedbackLibraryService      service,
                                        SystemAnnouncementService   announcer,
                                        Action<IEnumerable<string>> startReading)
        {
            _service      = service;
            _announcer    = announcer;
            _startReading = startReading;

            ReadEntryCommand   = new RelayCommand(ReadEntry,   () => _selectedEntry != null);
            DeleteEntryCommand = new RelayCommand(DeleteEntry, () => _selectedEntry != null);

            LoadEntries();
        }

        // ----------------------------------------------------------------
        // Public API
        // ----------------------------------------------------------------

        public void Reload() => LoadEntries();

        /// <summary>Called by MainViewModel after every feedback run to auto-persist.</summary>
        public void AddEntry(string feedbackType, string chapterTitle, string text)
        {
            var entry = _service.CreateEntry(feedbackType, chapterTitle, text, _allEntries);
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
            string? prevId = _selectedEntry?.Model.Id;
            Entries.Clear();

            IEnumerable<FeedbackEntry> source = _selectedCategory == "All"
                ? _allEntries
                : _allEntries.Where(e => _selectedCategory.StartsWith(e.CategoryLetter));

            foreach (var e in source.OrderBy(e => e.CategoryLetter).ThenBy(e => e.CreatedAt))
                Entries.Add(new FeedbackEntryViewModel(e));

            SelectedEntry = prevId == null ? null
                : Entries.FirstOrDefault(vm => vm.Model.Id == prevId);
        }

        private void ReadEntry()
        {
            if (_selectedEntry == null) return;
            _startReading(GetEntryText(_selectedEntry.Model.Id));
        }

        private void DeleteEntry()
        {
            if (_selectedEntry == null) return;
            string id = _selectedEntry.Model.Id;
            _allEntries.RemoveAll(e => e.Id == id);
            _service.Save(_allEntries);
            RebuildCategories();
            ApplyFilter();
            _announcer.Speak("Feedback entry deleted.");
        }

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

        // ----------------------------------------------------------------
        // INotifyPropertyChanged
        // ----------------------------------------------------------------

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notify([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
