using System.Windows;
using VoiceBookStudio.Helpers;
using VoiceBookStudio.Utils;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Views
{
    public partial class WelcomeDialog : Window
    {
        private static string BuildWelcomeMessage()
        {
            bool jaws   = VoiceBookStudio.Utils.AppSettings.IsJawsDetected;
            bool dragon = VoiceBookStudio.Utils.AppSettings.IsDragonRunning;

            string voiceNote;
            if (jaws)
                voiceNote = "JAWS screen reader is running. JAWS will read everything in this application. " +
                            "VoiceBook's own voice is silent — JAWS is your only audio source.";
            else if (dragon)
                voiceNote = "Dragon NaturallySpeaking is running. " +
                            "VoiceBook's built-in voice will guide you through the tour. " +
                            "Use ScrollLock to give app commands, or type them in the Command box.";
            else
                voiceNote = "VoiceBook's built-in voice is reading this message to you right now. " +
                            "The microphone is on and listening. Speak commands directly at any time.";

            return
                "Welcome to VoiceBook Studio. " +
                "This is a writing application designed for complete voice control. " +
                $"{voiceNote} " +
                "Press Enter or click Start guided tour to begin.";
        }

        public WelcomeDialog()
        {
            InitializeComponent();
        }

        public WelcomeDialogViewModel ViewModel => (WelcomeDialogViewModel)DataContext;

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            // Move keyboard focus to the default action immediately.
            StartButton.Focus();

            if (DataContext is not WelcomeDialogViewModel vm) return;

            // RequestClose is wired here rather than in the ViewModel constructor so the
            // handler is always tied to this specific dialog instance.
            vm.RequestClose += _ => Close();

            // Brief delay so the window finishes rendering before we speak.
            string msg = BuildWelcomeMessage();
            _ = System.Threading.Tasks.Task.Delay(400).ContinueWith(
                _ => Dispatcher.BeginInvoke(() =>
                {
                    AnnouncementText.Text = msg;

                    // Under JAWS, the live-region TextBlock above is not reliable here:
                    // WPF only raises LiveRegionChanged if an automation peer already
                    // exists for the element, and nothing has touched this dialog's
                    // automation tree yet (no hover, no Tab) at this point in startup.
                    // UiaAnnouncer force-creates a peer before raising the notification,
                    // so JAWS hears the welcome message immediately instead of only after
                    // the user manually tabs or hovers the mouse. AnnounceIntro (SAPI)
                    // already no-ops under JAWS by design, so without this branch JAWS
                    // users would hear nothing at all until they moved focus themselves.
                    if (VoiceBookStudio.Utils.AppSettings.IsJawsDetected)
                        UiaAnnouncer.Announce(this, msg, isUrgent: true);
                    else
                        vm.AnnounceIntro(msg);
                }));
        }
    }
}
