using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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

            // Opt this user out of Windows "Communications" audio ducking.
            // When SpeechRecognitionEngine opens a mic-capture session Windows
            // treats it as a call and reduces all other app audio by 80 percent,
            // making VoiceBook's SAPI announcements inaudible. Setting this to 3
            // is the same change as Control Panel > Sound > Communications > Do nothing,
            // and is fully visible and reversible from that same dialog.
            try
            {
                using var duckingKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                    @"SOFTWARE\Microsoft\Multimedia\Audio");
                duckingKey?.SetValue("UserDuckingPreference", 3, Microsoft.Win32.RegistryValueKind.DWord);
            }
            catch { /* non-fatal */ }

            // Load persisted settings before any service reads them.
            AppSettings.Load();
            AppSettings.LoadJsonSettings();

            var projectService = new ProjectService();
            var audio          = new AudioFeedbackService();
            var systemAnnounce = new SystemAnnouncementService();
            var appSounds      = new AppSoundService();
            var aiService      = new AiService();

            var mainVm     = new MainViewModel(projectService, audio, aiService, systemAnnounce, appSounds);
            var mainWindow = new MainWindow(appSounds) { DataContext = mainVm };
            MainWindow = mainWindow;

            // InitialiseAsync runs after ContentRendered. It handles mic activation,
            // the welcome dialog, and the first-launch tutorial — things that must
            // happen after the window is visible and its automation peers are live.
            mainWindow.ContentRendered += async (_, _) => await mainVm.InitialiseAsync();

            // Run the startup announcement sequence on the UI thread without blocking it.
            // Show() is called at the end of the sequence (step 8) so the window appears
            // only after the user has heard the full AT-status announcement. This ensures
            // JAWS receives the spoken status even before it begins reading the window.
            _ = Dispatcher.InvokeAsync(
                async () => await RunStartupSequenceAsync(mainWindow, systemAnnounce, audio),
                DispatcherPriority.Normal);
        }

        private static async Task RunStartupSequenceAsync(
            MainWindow                window,
            SystemAnnouncementService announce,
            AudioFeedbackService      audio)
        {
            // Prime the SAPI audio device before the first audible announcement.
            // The first SpeakAsync call opens the audio pipeline; without this the
            // first real utterance is often clipped or delayed by several hundred ms.
            try { await announce.PrimeAsync(); }
            catch { }

            // Step 1: Detect assistive technologies.
            bool jawsRunning   = await AssistiveTechnologyDetector.IsJawsRunningWithRetry();
            bool dragonRunning = AssistiveTechnologyDetector.IsDragonRunning();
            bool jSayRunning   = AssistiveTechnologyDetector.IsJSayRunning();

            // Step 2: Propagate detection results to shared settings.
            // The app always speaks its own feedback regardless of what AT is running —
            // JAWS/Dragon users can mute the app voice in Settings if they prefer silence.
            AppSettings.IsJawsDetected  = jawsRunning;
            AppSettings.IsDragonRunning = dragonRunning;
            AppSettings.IsJSayDetected  = jSayRunning;

            // Step 3: Wait for JAWS to finish its own startup speech before we speak.
            // Without this pause JAWS and the app talk over each other at launch.
            await Task.Delay(2000);

            // Step 4: Single startup announcement — only mention AT that IS running,
            // then confirm mic mode and ready state.
            string atStatus  = AssistiveTechnologyDetector.BuildStartupStatusMessage(); // empty string if nothing detected
            string micStatus = dragonRunning
                ? "Microphone is controlled by Dragon. Use ScrollLock to toggle voice commands."
                : "Built-in voice recognition is active. Say a command at any time.";
            string readyMsg = $"{atStatus}{micStatus} VoiceBook Studio is ready.";
            try { await announce.SpeakAndWaitAsync(readyMsg); }
            catch { }

            // Step 5: Show and activate the window, then move keyboard focus to the
            // chapter list. Activate() is required on Windows 11 because when JAWS or
            // Dragon owns the foreground at launch time, Show() alone does not bring the
            // window to the front. Task.Yield() hands control back to the dispatcher for
            // one frame so Window.Loaded fires and wires up all event handlers before
            // FocusChapterList runs.
            window.Show();
            window.Activate();
            await Task.Yield();
            window.FocusChapterList();
        }
    }
}
