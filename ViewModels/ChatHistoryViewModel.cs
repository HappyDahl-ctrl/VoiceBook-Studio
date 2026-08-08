using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using VoiceBookStudio.Models;
using VoiceBookStudio.Services;

namespace VoiceBookStudio.ViewModels
{
    public class ChatExchangeViewModel
    {
        public ChatExchange Model { get; }

        public string DisplayLabel
        {
            get
            {
                string preview = (Model.UserMessage ?? string.Empty).Trim();
                if (preview.Length > 60) preview = preview[..60] + "…";
                if (string.IsNullOrWhiteSpace(preview)) preview = "(no message)";
                return $"{Model.CreatedAt:MMM d, h:mm tt} — {preview}";
            }
        }

        public ChatExchangeViewModel(ChatExchange model) { Model = model; }
    }

    /// <summary>Backs the AI Assistant panel's chat history list: past exchanges for the
    /// current project only, newest last, selectable to expand the full exchange back into view.</summary>
    public partial class ChatHistoryViewModel : ObservableObject
    {
        private readonly ChatHistoryService _service;
        private List<ChatExchange> _allExchanges = new();

        public ObservableCollection<ChatExchangeViewModel> Exchanges { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedExchange))]
        private ChatExchangeViewModel? _selectedExchange;

        public bool HasSelectedExchange => SelectedExchange != null;

        partial void OnSelectedExchangeChanged(ChatExchangeViewModel? value)
        {
            if (value != null)
                ExchangeSelected?.Invoke(value.Model);
        }

        /// <summary>Raised when the user selects a past exchange, so MainViewModel can show
        /// its response in the same AI response box the live chat uses — Save as Card and
        /// Insert then act on it exactly as they would a fresh response.</summary>
        public event Action<ChatExchange>? ExchangeSelected;

        public ChatHistoryViewModel(ChatHistoryService service)
        {
            _service = service;
            Load();
        }

        /// <summary>Called when a project opens/switches so history reflects the newly opened project.</summary>
        public void Reload() => Load();

        public void Append(string userMessage, string aiResponse)
        {
            var exchange = new ChatExchange { UserMessage = userMessage, AiResponse = aiResponse };
            _allExchanges.Add(exchange);
            Exchanges.Add(new ChatExchangeViewModel(exchange));
            _service.Save(_allExchanges);
        }

        private void Load()
        {
            _allExchanges = _service.Load();
            Exchanges.Clear();
            foreach (var e in _allExchanges.OrderBy(e => e.CreatedAt))
                Exchanges.Add(new ChatExchangeViewModel(e));
            SelectedExchange = null;
        }
    }
}
