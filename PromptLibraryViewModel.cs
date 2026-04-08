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

        /// <summary>Label shown in the list: "F1 — Expand with sensory details"</summary>
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

        /// <summary>Prompts visible in the list (filtered by SelectedCategory).</summary>
        public ObservableCollection<PromptItemViewModel> Prompts    { get; } = new();

        /// <summary>"All" + distinct categories from the prompt file.</summary>
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
        // Command + event
        // ----------------------------------------------------------------

        public ICommand UsePromptCommand { get; }

        /// <summary>
        /// Fired when the user clicks Use Prompt or says "Use prompt X".
        /// Payload is the full prompt content string.
        /// MainViewModel subscribes and loads it into ChatInputText.
        /// </summary>
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
        // Public API (voice router)
        // ----------------------------------------------------------------

        /// <summary>
        /// Find a prompt by ID (e.g. "F5") and fire PromptSelected.
        /// Called by VoiceCommandRouter → MainViewModel.TryUsePromptById.
        /// </summary>
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

            // Restore selection if still visible after filter change
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
