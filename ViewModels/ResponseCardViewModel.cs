using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceBookStudio.Models;
using VoiceBookStudio.Services;

namespace VoiceBookStudio.ViewModels
{
    public partial class ResponseCardViewModel : ObservableObject
    {
        private readonly ResponseCardService _service;

        private List<ResponseCard> _allCards = new();

        // Maps lowercase category name → letter (A, B, C…), rebuilt after each change.
        private Dictionary<string, string> _categoryLetterMap = new();

        // ----------------------------------------------------------------
        // Collections
        // ----------------------------------------------------------------

        public ObservableCollection<ResponseCard> Cards      { get; } = new();
        public ObservableCollection<string>       Categories { get; } = new();

        // ----------------------------------------------------------------
        // Selected card
        // ----------------------------------------------------------------

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PreviewText))]
        [NotifyCanExecuteChangedFor(nameof(InsertCardCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteCardCommand))]
        private ResponseCard? _selectedCard;

        public string PreviewText => SelectedCard?.Content ?? string.Empty;

        // ----------------------------------------------------------------
        // Category filter
        // ----------------------------------------------------------------

        [ObservableProperty]
        private string _selectedCategory = "All";

        partial void OnSelectedCategoryChanged(string value) => ApplyFilter();

        // ----------------------------------------------------------------
        // Events
        // ----------------------------------------------------------------

        public event EventHandler<ResponseCard>? InsertCardRequested;

        /// <summary>
        /// Raised for spoken feedback the ViewModel wants announced. MainViewModel
        /// forwards this to LiveAnnounce so JAWS and non-JAWS users both hear it
        /// through the app's single announcement path (avoids double-speech).
        /// </summary>
        public event Action<string>? AnnouncementRequested;

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        public ResponseCardViewModel(ResponseCardService service)
        {
            _service = service;
            LoadCards();
        }

        // ----------------------------------------------------------------
        // Public API
        // ----------------------------------------------------------------

        public void Reload() => LoadCards();

        public void AddCard(ResponseCard card)
        {
            _allCards.Add(card);
            RebuildCategories();
            ApplyFilter();
            SaveCards();
            AnnouncementRequested?.Invoke($"Card saved: {card.Title}");
        }

        public void InsertCardByNumber(int oneBased)
        {
            int idx = oneBased - 1;
            if (idx < 0 || idx >= Cards.Count)
            {
                AnnouncementRequested?.Invoke($"No card number {oneBased}.");
                return;
            }
            SelectedCard = Cards[idx];
            InsertCard();
        }

        /// <summary>Insert a card by letter+number ID (e.g. "A1", "B3").</summary>
        public void InsertCardByLetterNumber(string id)
        {
            if (id.Length < 2) return;
            string letter = id[0].ToString().ToUpper();
            if (!int.TryParse(id[1..], out int num)) return;

            // Find category by letter
            string? catKey = _categoryLetterMap
                .FirstOrDefault(kv => string.Equals(kv.Value, letter, StringComparison.OrdinalIgnoreCase))
                .Key;

            if (catKey == null)
            {
                AnnouncementRequested?.Invoke($"No card category {letter}.");
                return;
            }

            string catName = catKey; // key is lowercase name
            var cards = _allCards
                .Where(c => string.Equals(c.Category, catName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            int idx = num - 1;
            if (idx < 0 || idx >= cards.Count)
            {
                AnnouncementRequested?.Invoke($"No card {id}.");
                return;
            }

            SelectedCard = cards[idx];
            InsertCard();
        }

        public void DeleteCardByNumber(int oneBased)
        {
            int idx = oneBased - 1;
            if (idx < 0 || idx >= Cards.Count)
            {
                AnnouncementRequested?.Invoke($"No card number {oneBased}.");
                return;
            }
            SelectedCard = Cards[idx];
            DeleteCard();
        }

        public void FilterByCategory(string category)
        {
            string? match = Categories.FirstOrDefault(c =>
                string.Equals(c, category, StringComparison.OrdinalIgnoreCase));
            SelectedCategory = match ?? "All";
            AnnouncementRequested?.Invoke($"Showing {SelectedCategory} cards.");
        }

        // ----------------------------------------------------------------
        // Voice navigation
        // ----------------------------------------------------------------

        public IEnumerable<string> GetCategoryReadingList()
        {
            if (_categoryLetterMap.Count == 0)
                return new[] { "Your card library is empty. Save an AI response as a card to get started." };

            var items = new List<string>();
            foreach (var kv in _categoryLetterMap.OrderBy(kv => kv.Value))
            {
                string catName = kv.Key;
                int    count   = _allCards.Count(c =>
                    string.Equals(c.Category, catName, StringComparison.OrdinalIgnoreCase));
                items.Add($"{kv.Value}: {TitleCase(catName)}. {count} {(count == 1 ? "card" : "cards")}.");
            }
            items.Add("Say Read Cards followed by a letter to hear cards in any category.");
            return items;
        }

        public IEnumerable<string> GetCategoryEntryList(string letter)
        {
            letter = letter.ToUpper();
            string? catKey = _categoryLetterMap
                .FirstOrDefault(kv => string.Equals(kv.Value, letter, StringComparison.OrdinalIgnoreCase))
                .Key;

            if (catKey == null)
                return new[] { $"No card category {letter}. Say What's in my card library to see all categories." };

            var cards = _allCards
                .Where(c => string.Equals(c.Category, catKey, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (cards.Count == 0)
                return new[] { $"No cards in category {letter}." };

            var items = new List<string>();
            for (int i = 0; i < cards.Count; i++)
                items.Add($"{letter}{i + 1}: {cards[i].Title}.");
            items.Add($"Say Insert Card followed by an ID such as {letter}1 to insert any card.");
            return items;
        }

        /// <summary>Returns existing category names (without "All") for the SaveCardDialog.</summary>
        public IEnumerable<string> GetExistingCategoryNames() =>
            _allCards.Select(c => c.Category)
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(c => c);

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
            // Rebuild letter map: sort category names alphabetically, assign A, B, C…
            _categoryLetterMap.Clear();
            char letter = 'A';
            foreach (string cat in _allCards
                         .Select(c => c.Category)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(c => c))
            {
                _categoryLetterMap[cat.ToLower()] = letter.ToString();
                letter++;
            }

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
            ResponseCard? prev = SelectedCard;
            Cards.Clear();

            IEnumerable<ResponseCard> source = SelectedCategory == "All"
                ? _allCards
                : _allCards.Where(c => string.Equals(c.Category, SelectedCategory,
                                            StringComparison.OrdinalIgnoreCase));

            foreach (var c in source)
                Cards.Add(c);

            SelectedCard = Cards.Contains(prev!) ? prev : null;
        }

        [RelayCommand(CanExecute = nameof(CanModifyCard))]
        private void InsertCard()
        {
            if (SelectedCard == null) return;
            InsertCardRequested?.Invoke(this, SelectedCard);
        }

        [RelayCommand(CanExecute = nameof(CanModifyCard))]
        private void DeleteCard()
        {
            if (SelectedCard == null) return;
            string title = SelectedCard.Title;
            _allCards.Remove(SelectedCard);
            Cards.Remove(SelectedCard);
            SelectedCard = null;
            RebuildCategories();
            ApplyFilter();
            SaveCards();
            AnnouncementRequested?.Invoke($"Card deleted: {title}");
        }

        private bool CanModifyCard() => SelectedCard != null;

        private void SaveCards() => _service.Save(_allCards);

        private static string TitleCase(string s) =>
            string.IsNullOrEmpty(s) ? s :
            char.ToUpper(s[0]) + s[1..];
    }
}
