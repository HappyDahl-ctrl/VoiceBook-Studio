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

            // Step 2: Propagate detection results to shared settings and services.
            // AudioFeedbackService.SetJawsDetected silences general app TTS so JAWS
            // handles all feedback — only SystemAnnouncementService keeps speaking.
            AppSettings.IsJawsDetected  = jawsRunning;
            AppSettings.IsDragonRunning = dragonRunning;
            AppSettings.IsJSayDetected  = jSayRunning;
            if (jawsRunning)
                audio.SetJawsDetected(true);

            // Step 3: Speak AT status. This always speaks regardless of JAWS state
            // because the user must know whether JAWS itself is working before they
            // can rely on it for anything else.
            string atStatus = AssistiveTechnologyDetector.BuildStartupStatusMessage();
            try { await announce.SpeakAndWaitAsync(atStatus); }
            catch { }

            // Step 4: Wait for JAWS to finish its own startup speech before continuing.
            await Task.Delay(2000);

            // Step 5: Announce microphone state.
            // NOTE: When Dragon is not running, MainViewModel.InitialiseAsync() starts
            // the built-in Windows Speech Recognition engine. The message must reflect
            // this so accessibility users know voice commands ARE available.
            string micStatus = dragonRunning
                ? "Microphone is controlled by Dragon. Speak your commands naturally."
                : "Dragon was not detected. VoiceBook built-in voice recognition is starting. " +
                  "You can say a command at any time after the ready announcement.";
            try { await announce.SpeakAndWaitAsync(micStatus); }
            catch { }

            // Step 6: Pause before the ready announcement.
            await Task.Delay(1500);

            // Step 7: Final ready announcement — matches user manual section 4 wording.
            try { await announce.SpeakAndWaitAsync("VoiceBook Studio ready. Focus is on the chapter list."); }
            catch { }

            // Step 8: Show and activate the window, then move keyboard focus to the
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
