using System.Collections.ObjectModel;
using System.Windows.Input;
using VoiceBookStudio.Services;
using VoiceBookStudio.Models;
using VoiceBookStudio.Utils;
using System;

namespace VoiceBookStudio.ViewModels
{
    public class ChapterConfirmationViewModel
    {
        private readonly SystemAnnouncementService _announcer;

        public ObservableCollection<DetectedChapter> Detected { get; } = new();

        public ICommand AcceptAllCommand => new RelayCommand(AcceptAll);
        public ICommand ImportSingleCommand => new RelayCommand(ImportSingle);
        public ICommand CancelCommand => new RelayCommand(Cancel);

        public ChapterConfirmationViewModel(SystemAnnouncementService announcer)
        {
            _announcer = announcer;
        }

        private void AcceptAll() { }
        private void ImportSingle() { }
        private void Cancel() { }
    }
}
