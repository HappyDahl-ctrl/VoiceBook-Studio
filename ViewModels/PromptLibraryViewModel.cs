using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VoiceBookStudio.Models;
using VoiceBookStudio.Services;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.ViewModels
{
    public class PromptItemViewModel
    {
        public PromptItem Model { get; }

        public string Number   => Model.Id;
        public string Title    => Model.Title;
        public string Category => Model.Category;

        public string DisplayLabel => $"{Model.Id} — {Model.Title}";

        public string Preview => Model.Content.Length > 60
            ? Model.Content[..60] + "…"
            : Model.Content;

        public PromptItemViewModel(PromptItem model) { Model = model; }
    }

    public class PromptLibraryViewModel : INotifyPropertyChanged
    {
        private readonly PromptLibraryService      _service;
        private readonly SystemAnnouncementService _announcer;

        private List<PromptItemViewModel> _allPrompts = new();

        // ----------------------------------------------------------------
        // Collections
        // ----------------------------------------------------------------

        public ObservableCollection<PromptItemViewModel> Prompts    { get; } = new();
        public ObservableCollection<string>              Categories { get; } = new();

        // ----------------------------------------------------------------
        // Selection
        // ----------------------------------------------------------------

        private PromptItemViewModel? _selectedPrompt;
        public PromptItemViewModel? SelectedPrompt
        {
            get => _selectedPrompt;
            set
            {
                if (_selectedPrompt == value) return;
                _selectedPrompt = value;
                Notify();
                ((RelayCommand)UsePromptCommand).RaiseCanExecuteChanged();
            }
        }

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
        // Commands + event
        // ----------------------------------------------------------------

        public ICommand UsePromptCommand { get; }

        public event EventHandler<string>? PromptSelected;

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        public PromptLibraryViewModel(PromptLibraryService service, SystemAnnouncementService announcer)
        {
            _service   = service;
            _announcer = announcer;

            UsePromptCommand = new RelayCommand(UsePrompt, () => _selectedPrompt != null);

            LoadPrompts();
        }

        // ----------------------------------------------------------------
        // Voice navigation
        // ----------------------------------------------------------------

        /// <summary>Returns items for the reading controller: category letters and names.</summary>
        public IEnumerable<string> GetCategoryReadingList()
        {
            var groups = _allPrompts
                .GroupBy(p => char.ToUpper(p.Number.Length > 0 ? p.Number[0] : '?'))
                .OrderBy(g => g.Key);

            var items = new List<string>();
            foreach (var g in groups)
            {
                string name  = g.First().Category;
                int    count = g.Count();
                items.Add($"{g.Key}: {name}. {count} {(count == 1 ? "prompt" : "prompts")}.");
            }
            if (items.Count == 0)
                items.Add("No prompts loaded.");
            else
                items.Add("Say Read Prompt followed by a letter to hear prompts in any category.");
            return items;
        }

        /// <summary>Returns items for the reading controller: prompt IDs and titles in one category.</summary>
        public IEnumerable<string> GetCategoryEntryList(string letter)
        {
            letter = letter.ToUpper();
            var matches = _allPrompts
                .Where(p => p.Number.Length > 0 &&
                            char.ToUpper(p.Number[0]).ToString() == letter)
                .ToList();

            if (matches.Count == 0)
                return new[] { $"No prompts found in category {letter}." };

            var items = new List<string>();
            foreach (var p in matches)
                items.Add($"{p.Number}: {p.Title}.");
            items.Add($"Say Use Prompt followed by an ID such as {matches[0].Number} to load any prompt.");
            return items;
        }

        // ----------------------------------------------------------------
        // Public API
        // ----------------------------------------------------------------

        public void SelectById(string id)
        {
            var match = _allPrompts.FirstOrDefault(p =>
                string.Equals(p.Number, id, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                _announcer.Speak($"Prompt {id} not found.");
                return;
            }

            SelectedPrompt = match;
            UsePrompt();
        }

        /// <summary>
        /// Adds a new prompt to the library and persists.
        /// Returns the auto-generated ID.
        /// </summary>
        public string AddPrompt(string categoryLetter, string categoryName, string title, string content)
        {
            string id = _service.NextIdForLetter(categoryLetter);
            var newItem = new PromptItem
            {
                Id       = id,
                Category = categoryName,
                Title    = title,
                Content  = content
            };
            _service.AddAndSave(newItem);
            _allPrompts.Add(new PromptItemViewModel(newItem));
            RebuildCategories();
            ApplyFilter();
            return id;
        }

        /// <summary>Returns existing categories for the AddPromptDialog.</summary>
        public List<(string Letter, string Name)> GetExistingCategories() =>
            _service.GetCategories();

        public string GetNextAvailableLetter() => _service.NextAvailableLetter();

        // ----------------------------------------------------------------
        // Private helpers
        // ----------------------------------------------------------------

        private void LoadPrompts()
        {
            _allPrompts = _service.LoadPrompts()
                                  .Select(p => new PromptItemViewModel(p))
                                  .ToList();
            RebuildCategories();
            ApplyFilter();
        }

        private void RebuildCategories()
        {
            Categories.Clear();
            Categories.Add("All");
            foreach (string cat in _allPrompts
                         .Select(p => p.Category)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(c => c))
            {
                Categories.Add(cat);
            }
        }

        private void ApplyFilter()
        {
            PromptItemViewModel? prev = _selectedPrompt;
            Prompts.Clear();

            IEnumerable<PromptItemViewModel> source = _selectedCategory == "All"
                ? _allPrompts
                : _allPrompts.Where(p => string.Equals(p.Category, _selectedCategory,
                                             StringComparison.OrdinalIgnoreCase));

            foreach (var p in source)
                Prompts.Add(p);

            SelectedPrompt = Prompts.Contains(prev!) ? prev : null;
        }

        private void UsePrompt()
        {
            if (_selectedPrompt == null) return;
            AppSettings.LastUsedPromptId = _selectedPrompt.Number;
            PromptSelected?.Invoke(this, _selectedPrompt.Model.Content);
            _announcer.Speak($"Prompt loaded: {_selectedPrompt.Title}");
        }

        // ----------------------------------------------------------------
        // INotifyPropertyChanged
        // ----------------------------------------------------------------

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notify([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
