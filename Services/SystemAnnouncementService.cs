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
    /// No-ops entirely once JAWS is detected (see <see cref="SetJawsDetected"/>) — JAWS is
    /// the sole audio source in that mode, so nothing here may compete with it.
    ///
    /// Uses Azure Neural TTS when configured, otherwise SAPI with best available voice.
    /// </summary>
    public class SystemAnnouncementService : IDisposable
    {
        private readonly SpeechSynthesizer _sapi;
        private readonly AzureTtsService   _azure;
        private bool _disposed;
        private bool _jawsDetected;

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
            if (_jawsDetected || _disposed || string.IsNullOrWhiteSpace(text)) return;
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
            if (_jawsDetected || _disposed || string.IsNullOrWhiteSpace(message)) return;
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
            if (_jawsDetected || _disposed || string.IsNullOrWhiteSpace(text)) return;
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
        /// Used for startup announcements. Adds 500 ms pre-speech delay when JAWS active.
        /// </summary>
        public async Task SpeakAndWaitAsync(string text)
        {
            if (_jawsDetected || _disposed || string.IsNullOrWhiteSpace(text)) return;
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
