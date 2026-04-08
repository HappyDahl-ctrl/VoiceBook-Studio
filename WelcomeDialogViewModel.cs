using System.Windows.Input;
using VoiceBookStudio.Utils;
using VoiceBookStudio.Services;

namespace VoiceBookStudio.ViewModels
{
    public class WelcomeDialogViewModel
    {
        private readonly SystemAnnouncementService _announcer;

        public WelcomeDialogViewModel(SystemAnnouncementService announcer)
        {
            _announcer = announcer;
            ShowOnStartup = AppSettings.ShowWelcomeOnStartup;
            // Announce the welcome dialog content so JAWS users also hear it
            _announcer.Speak("Welcome to VoiceBook Studio. Press Start Tutorial to begin, or Skip Tutorial to continue to the application.");
        }

        public bool ShowOnStartup { get; set; }

        public ICommand StartTutorialCommand => new RelayCommand(StartTutorial);
        public ICommand SkipTutorialCommand => new RelayCommand(SkipTutorial);

        private void StartTutorial()
        {
            AppSettings.ShowWelcomeOnStartup = ShowOnStartup;
        }

        private void SkipTutorial()
        {
            AppSettings.ShowWelcomeOnStartup = ShowOnStartup;
        }
    }
}
