using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using VoiceBookStudio.Services;

namespace VoiceBookStudio.ViewModels
{
    public class WelcomeDialogViewModel
    {
        private readonly SystemAnnouncementService _announcer;

        /// <summary>True when the user chose to start the guided tour.</summary>
        public bool StartRequested { get; private set; }

        /// <summary>Raised when a button is pressed and the dialog should close.</summary>
        public System.Action<bool>? RequestClose;

        public WelcomeDialogViewModel(SystemAnnouncementService announcer)
        {
            _announcer = announcer;
        }

        /// <summary>
        /// Speaks the supplied welcome message via TTS.
        /// Called from Window_ContentRendered when JAWS is NOT running.
        /// </summary>
        public void AnnounceIntro(string message) => _announcer.Speak(message);

        /// <summary>
        /// Speaks the default welcome introduction via TTS.
        /// Kept for backward compatibility with the Help menu's ShowWelcome command.
        /// </summary>
        public void AnnounceIntro() =>
            _announcer.Speak(
                "Welcome to VoiceBook Studio. This is your first time here. " +
                "I will walk you through everything you need to know. " +
                "Press Start guided tour or say click start guided tour to begin. " +
                "Press Skip tour to go straight to writing.");

        public ICommand StartTutorialCommand => new RelayCommand(StartTutorial);
        public ICommand SkipTutorialCommand  => new RelayCommand(SkipTutorial);

        private void StartTutorial()
        {
            StartRequested = true;
            RequestClose?.Invoke(true);
        }

        private void SkipTutorial()
        {
            RequestClose?.Invoke(false);
        }
    }
}
