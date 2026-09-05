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

            StartJawsWatcher(systemAnnounce, audio);
        }

        /// <summary>
        /// Re-checks JAWS's running state every 15 seconds for the life of the app and
        /// keeps both TTS services in sync with it.
        ///
        /// Startup-only detection (<see cref="RunStartupSequenceAsync"/>) has a narrow
        /// window: JAWS 2026 can take well over the retry budget to appear (Vispero
        /// account sign-in on first run), so a JAWS instance that finishes starting late
        /// is never noticed — the app keeps speaking with its own voice for the rest of
        /// the session, overlapping JAWS's own reading. This watcher catches that case
        /// (and the reverse — JAWS closed mid-session) by re-evaluating on a timer
        /// instead of trusting the one-time startup snapshot.
        /// </summary>
        private void StartJawsWatcher(SystemAnnouncementService announce, AudioFeedbackService audio)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
            timer.Tick += (_, _) =>
            {
                // A manual "JAWS is running" / "JAWS is not running" override takes
                // precedence — otherwise this timer would silently revert it on its next
                // tick, which is exactly the wrong behavior for a user who just corrected
                // a wrong auto-detection.
                if (AppSettings.IsJawsDetectionOverridden) return;

                bool jawsRunning = AssistiveTechnologyDetector.IsJawsRunning();
                if (jawsRunning == AppSettings.IsJawsDetected) return;

                AppSettings.IsJawsDetected = jawsRunning;
                audio.SetJawsDetected(jawsRunning);
                announce.SetJawsDetected(jawsRunning);
            };
            timer.Start();
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
            // When JAWS is running it handles all speech; silence both TTS services
            // so they never compete with JAWS on the audio device.
            AppSettings.IsJawsDetected  = jawsRunning;
            AppSettings.IsDragonRunning = dragonRunning;
            AppSettings.IsJSayDetected  = jSayRunning;
            audio.SetJawsDetected(jawsRunning);
            announce.SetJawsDetected(jawsRunning);

            if (!jawsRunning)
            {
                // Step 3: Prime and speak startup announcement for non-JAWS users.
                await Task.Delay(500);
                string micStatus = dragonRunning
                    ? "Microphone is controlled by Dragon. Use ScrollLock to toggle voice commands."
                    : "Built-in voice recognition is active. Say a command at any time.";
                string atStatus = AssistiveTechnologyDetector.BuildStartupStatusMessage();
                string readyMsg = $"{atStatus}{micStatus} VoiceBook Studio is ready.";
                try { await announce.SpeakAndWaitAsync(readyMsg); }
                catch { }
            }
            else
            {
                // JAWS is running — wait for it to finish its own startup speech,
                // then let it read the window title and focused control naturally.
                await Task.Delay(2000);
            }

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
