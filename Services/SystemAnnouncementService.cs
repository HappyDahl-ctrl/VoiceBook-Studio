using System;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.Services
{
    /// <summary>Priority levels for AnnounceWithPriority.</summary>
    public enum AnnouncementPriority
    {
        /// <summary>Queued behind any current speech; 500 ms delay when JAWS active.</summary>
        Normal,
        /// <summary>
        /// Interrupts any current SystemAnnouncementService speech immediately,
        /// then waits 500 ms if JAWS is active before speaking.
        /// </summary>
        Critical,
        /// <summary>
        /// Suppresses the announcement entirely. Use during tutorial sequences
        /// when JAWS is handling narration and TTS would create double-reading.
        /// </summary>
        Silent
    }

    /// <summary>
    /// Speaks system-level announcements (startup greeting, project events, tutorial steps).
    /// No-ops once JAWS is detected (see <see cref="SetJawsDetected"/>) for every method
    /// except <see cref="SpeakOnDemandAsync"/> — JAWS is the sole audio source for
    /// ambient app state, so nothing else here may compete with it. SpeakOnDemandAsync is
    /// a deliberate, narrow exception used only to read app-generated content aloud when
    /// the user explicitly asks for it (an AI response, a chapter, a library entry); see
    /// its own doc comment for why.
    ///
    /// Uses Azure Neural TTS when configured, otherwise SAPI with best available voice.
    /// </summary>
    public class SystemAnnouncementService : IDisposable
    {
        private readonly SpeechSynthesizer _sapi;
        private readonly AzureTtsService   _azure;
        private bool _disposed;
        private bool _jawsDetected;

        /// <summary>
        /// True after the user explicitly turns app voice off (the "toggle voice" /
        /// "voice off" command, or the Settings toggle). Distinct from
        /// <see cref="_jawsDetected"/> — this is a deliberate user choice rather than
        /// an auto-detection result, and it is checked by every ambient/status
        /// announcement method below (but not <see cref="SpeakOnDemandAsync"/>, which
        /// stays available for content the user explicitly asks to hear regardless).
        /// </summary>
        public bool IsMuted { get; private set; }

        // True once the SAPI audio pipeline has been exercised at least once.
        private bool _sapiPrimed;

        public SystemAnnouncementService()
        {
            _sapi  = new SpeechSynthesizer();
            _azure = new AzureTtsService();

            try { _sapi.SetOutputToDefaultAudioDevice(); }
            catch { /* no default audio device — Speak will fail silently */ }

            _sapi.Rate   = 1;
            _sapi.Volume = 90;

            SpeechTextUtils.SelectBestSapiVoice(_sapi);
            _azure.Configure();
        }

        /// <summary>
        /// Called once at startup with the JAWS detection result.
        /// When detected, all Speak methods become no-ops — JAWS handles all speech
        /// via UIA live regions and UiaAnnouncer.RaiseNotificationEvent instead.
        /// </summary>
        public void SetJawsDetected(bool detected) => _jawsDetected = detected;

        /// <summary>
        /// Called when the user explicitly toggles app voice on/off (see
        /// <see cref="IsMuted"/>). When muted, ambient/status announcements become
        /// no-ops, same as when JAWS is detected.
        /// </summary>
        public void SetMuted(bool muted) => IsMuted = muted;

        /// <summary>
        /// Primes the SAPI audio pipeline by speaking a silent phrase.
        /// Call this before the first real announcement so the audio device is
        /// already open when the startup message fires.
        /// </summary>
        public async Task PrimeAsync()
        {
            if (_jawsDetected || _sapiPrimed || _azure.IsConfigured) return;
            try
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                void Done(object? s, SpeakCompletedEventArgs e) { _sapi.SpeakCompleted -= Done; tcs.TrySetResult(true); }
                _sapi.SpeakCompleted += Done;
                _sapi.SpeakAsync(" ");
                await tcs.Task.WaitAsync(TimeSpan.FromSeconds(3));
                _sapiPrimed = true;
            }
            catch { _sapiPrimed = true; }
        }

        public void ReconfigureAzure() => _azure.Configure();

        /// <summary>
        /// Speak an announcement. No-op when JAWS is detected.
        /// Interrupts any in-progress or queued speech first so a burst of rapid
        /// events (e.g. several chapters confirmed in quick succession) cannot pile
        /// up into a stale backlog that plays back after the app has moved on.
        /// </summary>
        public void Speak(string text)
        {
            if (_jawsDetected || IsMuted || _disposed || string.IsNullOrWhiteSpace(text)) return;
            text = SpeechTextUtils.SanitizeForSpeech(text);
            StopSpeaking();
            ActuallySpeak(text);
        }

        /// <summary>
        /// Speak with explicit priority control.
        ///   Critical — interrupts any current speech immediately.
        ///   Normal   — queues normally.
        ///   Silent   — suppresses the announcement entirely.
        /// No-op when JAWS is detected — JAWS handles all speech.
        /// </summary>
        public void AnnounceWithPriority(string message, AnnouncementPriority priority)
        {
            if (priority == AnnouncementPriority.Silent) return;
            if (_jawsDetected || IsMuted || _disposed || string.IsNullOrWhiteSpace(message)) return;
            message = SpeechTextUtils.SanitizeForSpeech(message);
            if (priority == AnnouncementPriority.Critical)
                StopSpeaking();
            ActuallySpeak(message);
        }

        /// <summary>
        /// Speaks text synchronously (blocks until the utterance is complete).
        /// Used only for the app-closing goodbye so the message finishes before
        /// the process exits. Do not use this for anything else — it blocks the
        /// UI thread. No-op when JAWS is detected, same as every other method on
        /// this class — JAWS is closing its own reading of the window and must
        /// not be talked over during shutdown either.
        /// </summary>
        public void SpeakSync(string text)
        {
            if (_jawsDetected || IsMuted || _disposed || string.IsNullOrWhiteSpace(text)) return;
            text = SpeechTextUtils.SanitizeForSpeech(text);
            if (_azure.IsConfigured)
            {
                // Azure is async-only; fall back to SAPI for the goodbye so
                // we can guarantee the utterance completes before exit.
                using var sapi = new SpeechSynthesizer();
                try
                {
                    sapi.SetOutputToDefaultAudioDevice();
                    SpeechTextUtils.SelectBestSapiVoice(sapi);
                    sapi.Speak(text);
                }
                catch { /* non-fatal — process is exiting */ }
                return;
            }
            try { _sapi.Speak(text); }
            catch { /* non-fatal */ }
        }

        public void StopSpeaking()
        {
            if (_disposed) return;
            if (_azure.IsConfigured)
                _azure.StopSpeaking();
            else
                _sapi.SpeakAsyncCancelAll();
        }

        /// <summary>
        /// Speak synchronously and await completion.
        /// Used for startup announcements. No-op when JAWS is detected.
        /// </summary>
        public async Task SpeakAndWaitAsync(string text)
        {
            if (_jawsDetected || IsMuted) return;
            await SpeakAndWaitCoreAsync(text);
        }

        /// <summary>
        /// Speaks on-demand narration of app-held content the user explicitly asked to
        /// hear — an AI response, a chapter, a library entry — via "read response",
        /// Space bar, or similar. Deliberately bypasses the JAWS silence rule every
        /// other method on this class enforces: JAWS's own reading commands ("Say All",
        /// read line) aren't built to read one long, resumable block of app-generated
        /// text with real stop/resume, so this is a narrow, explicit exception. Every
        /// other announcement in the app — status changes, navigation, dialogs — stays
        /// JAWS's job exactly as before. Callers should make it clear via a lead-in
        /// phrase (see MainViewModel.ContinueReadingAsync) that the app's own voice is
        /// taking over for this one purpose, so it's never ambiguous which voice is
        /// speaking.
        /// </summary>
        public async Task SpeakOnDemandAsync(string text) => await SpeakAndWaitCoreAsync(text);

        private async Task SpeakAndWaitCoreAsync(string text)
        {
            if (_disposed || string.IsNullOrWhiteSpace(text)) return;
            text = SpeechTextUtils.SanitizeForSpeech(text);

            if (_azure.IsConfigured)
            {
                await _azure.SpeakAndWaitAsync(text);
                return;
            }

            // Use SpeakAsync + TaskCompletionSource — calling the synchronous Speak()
            // from an MTA thread-pool thread is unreliable because SAPI's COM audio
            // objects expect STA context. SpeakAsync is safe from any thread.
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnCompleted(object? sender, SpeakCompletedEventArgs e)
            {
                _sapi.SpeakCompleted -= OnCompleted;
                tcs.TrySetResult(true);
            }

            _sapi.SpeakCompleted += OnCompleted;
            try
            {
                _sapi.SpeakAsync(text);
                await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));
            }
            catch
            {
                _sapi.SpeakCompleted -= OnCompleted;
            }
        }

        // ----------------------------------------------------------------
        // Private helpers
        // ----------------------------------------------------------------

        private void ActuallySpeak(string text)
        {
            if (_disposed) return;
            if (_azure.IsConfigured)
                _azure.Speak(text);
            else
                _sapi.SpeakAsync(text);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _sapi.SpeakAsyncCancelAll();
                _sapi.Dispose();
                _azure.Dispose();
                _disposed = true;
            }
        }
    }
}
