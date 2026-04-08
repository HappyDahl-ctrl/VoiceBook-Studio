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
    public class ResponseCardViewModel : INotifyPropertyChanged
    {
        private readonly ResponseCardService _service;
        private readonly SystemAnnouncementService _announcer;

        private List<ResponseCard> _allCards = new();

        // ----------------------------------------------------------------
        // Collections
        // ----------------------------------------------------------------

        /// <summary>Cards visible in the list (filtered by SelectedCategory).</summary>
        public ObservableCollection<ResponseCard> Cards { get; } = new();

        /// <summary>"All" + distinct categories found in the card collection.</summary>
        public ObservableCollection<string> Categories { get; } = new();

        // ----------------------------------------------------------------
        // Selected card
        // ----------------------------------------------------------------

        private ResponseCard? _selectedCard;
        public ResponseCard? SelectedCard
        {
            get => _selectedCard;
            set
            {
                if (_selectedCard == value) return;
                _selectedCard = value;
                Notify();
                Notify(nameof(PreviewText));
                ((RelayCommand)InsertCardCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCardCommand).RaiseCanExecuteChanged();
            }
        }

        public string PreviewText => _selectedCard?.Content ?? string.Empty;

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

        public ICommand InsertCardCommand { get; }
        public ICommand DeleteCardCommand { get; }

        // ----------------------------------------------------------------
        // Event raised when caller should insert a card into the editor
        // ----------------------------------------------------------------

        public event EventHandler<ResponseCard>? InsertCardRequested;

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        public ResponseCardViewModel(ResponseCardService service, SystemAnnouncementService announcer)
        {
            _service  = service;
            _announcer = announcer;

            InsertCardCommand = new RelayCommand(InsertCard, () => _selectedCard != null);
            DeleteCardCommand = new RelayCommand(DeleteCard, () => _selectedCard != null);

            LoadCards();
        }

        // ----------------------------------------------------------------
        // Public API (called by MainViewModel / VoiceCommandRouter)
        // ----------------------------------------------------------------

        /// <summary>Add a brand-new card and persist.</summary>
        public void AddCard(ResponseCard card)
        {
            _allCards.Add(card);
            RebuildCategories();
            ApplyFilter();
            SaveCards();
            _announcer.Speak($"Card saved: {card.Title}");
        }

        /// <summary>Voice command: "Insert card 2".</summary>
        public void InsertCardByNumber(int oneBased)
        {
            int idx = oneBased - 1;
            if (idx < 0 || idx >= Cards.Count)
            {
                _announcer.Speak($"No card number {oneBased}.");
                return;
            }
            SelectedCard = Cards[idx];
            InsertCard();
        }

        /// <summary>Voice command: "Delete card 2".</summary>
        public void DeleteCardByNumber(int oneBased)
        {
            int idx = oneBased - 1;
            if (idx < 0 || idx >= Cards.Count)
            {
                _announcer.Speak($"No card number {oneBased}.");
                return;
            }
            SelectedCard = Cards[idx];
            DeleteCard();
        }

        /// <summary>Voice command: "Show Editing cards".</summary>
        public void FilterByCategory(string category)
        {
            string? match = Categories.FirstOrDefault(c =>
                string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
            SelectedCategory = match ?? "All";
            _announcer.Speak($"Showing {SelectedCategory} cards.");
        }

        // ----------------------------------------------------------------
        // Private helpers
        // ----------------------------------------------------------------

        private void LoadCards()
        {
            _allCards = _service.Load();
            RebuildCategories();
            ApplyFilter();
        }

        private void RebuildCategories()
        {
            Categories.Clear();
            Categories.Add("All");
            foreach (string cat in _allCards
                         .Select(c => c.Category)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(c => c))
            {
                Categories.Add(cat);
            }
        }

        private void ApplyFilter()
        {
            ResponseCard? prev = _selectedCard;
            Cards.Clear();

            IEnumerable<ResponseCard> source = _selectedCategory == "All"
                ? _allCards
                : _allCards.Where(c => string.Equals(c.Category, _selectedCategory,
                                            StringComparison.OrdinalIgnoreCase));

            foreach (var c in source)
                Cards.Add(c);

            // Restore selection if still visible
            SelectedCard = Cards.Contains(prev!) ? prev : null;
        }

        private void InsertCard()
        {
            if (_selectedCard == null) return;
            InsertCardRequested?.Invoke(this, _selectedCard);
            // Voice announcement is in MainViewModel.InsertCardAtCursor so it
            // runs regardless of whether the command was triggered by button or voice.
        }

        private void DeleteCard()
        {
            if (_selectedCard == null) return;
            string title = _selectedCard.Title;
            _allCards.Remove(_selectedCard);
            Cards.Remove(_selectedCard);
            SelectedCard = null;
            RebuildCategories();
            ApplyFilter();
            SaveCards();
            _announcer.Speak($"Card deleted: {title}");
        }

        private void SaveCards() => _service.Save(_allCards);

        // ----------------------------------------------------------------
        // INotifyPropertyChanged
        // ----------------------------------------------------------------

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notify([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
