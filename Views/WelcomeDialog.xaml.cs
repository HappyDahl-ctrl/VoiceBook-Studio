using System.Windows;
using VoiceBookStudio.Helpers;
using VoiceBookStudio.Utils;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Views
{
    public partial class WelcomeDialog : Window
    {
        // Exact welcome message text. Spoken by TTS (no JAWS) or raised as a UIA
        // notification (JAWS present) after a short delay so JAWS finishes reading
        // the window opening before the announcement fires.
        private const string WelcomeMessage =
            "Welcome to VoiceBook Studio. " +
            "This is a writing application designed for complete voice control. " +
            "JAWS will read all controls. Dragon commands are active. " +
            "Say click start guided tour to begin the introduction, " +
            "or say click skip tour to go straight to the application.";

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

            // Delay so JAWS finishes announcing the window title and controls before
            // the welcome announcement fires. 900 ms matches the original timing.
            _ = System.Threading.Tasks.Task.Delay(900).ContinueWith(
                _ => Dispatcher.BeginInvoke(() =>
                {
                    // Set the live-region TextBlock text. Changing Text on a
                    // LiveSetting=Assertive element raises LiveRegionChanged in UIA,
                    // which JAWS reads immediately.
                    AnnouncementText.Text = WelcomeMessage;

                    if (AppSettings.IsJawsDetected)
                    {
                        // Belt-and-suspenders: explicitly raise a UIA notification so
                        // JAWS announces regardless of whether its live-region polling
                        // fired for the text change.
                        UiaAnnouncer.Announce(AnnouncementText, WelcomeMessage, isUrgent: false);
                    }
                    else
                    {
                        // No screen reader — speak the welcome message via SAPI.
                        vm.AnnounceIntro(WelcomeMessage);
                    }
                }));
        }
    }
}
