using System.Diagnostics;
using System.Windows;
using VoiceBookStudio.Services;
using VoiceBookStudio.Utils;
using VoiceBookStudio.ViewModels;
using VoiceBookStudio.Views;

namespace VoiceBookStudio
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Detect JAWS before any audio service initialises
            AppSettings.IsJawsDetected =
                Process.GetProcessesByName("jfw").Length > 0 ||
                Process.GetProcessesByName("jaws").Length > 0;

            // Load persisted settings (welcome/tutorial prefs)
            AppSettings.Load();

            var projectService = new ProjectService();
            var audio          = new AudioFeedbackService();
            var systemAnnounce = new SystemAnnouncementService();
            var aiService      = new AiService();

            var mainVm     = new MainViewModel(projectService, audio, aiService, systemAnnounce);
            var mainWindow = new MainWindow { DataContext = mainVm };
            MainWindow     = mainWindow;
            mainWindow.Show();

            // InitialiseAsync handles the welcome dialog, tutorial, and startup announcement.
            // Run it after the window is fully rendered so Owner references work.
            mainWindow.Loaded += async (_, _) => await mainVm.InitialiseAsync();
        }
    }
}
