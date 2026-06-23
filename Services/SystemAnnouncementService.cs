using System;
using System.Linq;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
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
    /// Always speaks regardless of JAWS — these are critical notifications that must be
    /// delivered even if JAWS is busy or the user has JAWS set to a quiet mode.
    ///
    /// When JAWS is detected, each announcement is preceded by a 500 ms pause so
    /// this service does not clash with whatever JAWS is currently reading.
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

            SelectBestSapiVoice();
            _azure.Configure();
        }

        /// <summary>
        /// Called once at startup with the JAWS detection result.
        /// When detected, each Speak call is preceded by a 500 ms pause to avoid
        /// clashing with JAWS's current utterance.
        /// </summary>
        public void SetJawsDetected(bool detected) => _jawsDetected = detected;

        /// <summary>
        /// Primes the SAPI audio pipeline by speaking a silent phrase.
        /// Call this before the first real announcement so the audio device is
        /// already open when the startup message fires.
        /// </summary>
        public async Task PrimeAsync()
        {
            if (_sapiPrimed || _azure.IsConfigured) return;
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
        /// Speak an announcement. Always fires even when JAWS is running.
        /// Adds a 500 ms pre-speech pause when JAWS is detected.
        /// </summary>
        public void Speak(string text)
        {
            if (_disposed || string.IsNullOrWhiteSpace(text)) return;
            text = SanitizeForSpeech(text);
            if (_jawsDetected)
                _ = Task.Run(async () => { await Task.Delay(500); ActuallySpeak(text); });
            else
                ActuallySpeak(text);
        }

        /// <summary>
        /// Speak with explicit priority control.
        ///   Critical — interrupts any current speech immediately, then 500 ms pause if JAWS.
        ///   Normal   — queues normally, 500 ms pause if JAWS.
        ///   Silent   — suppresses the announcement entirely.
        /// </summary>
        public void AnnounceWithPriority(string message, AnnouncementPriority priority)
        {
            if (priority == AnnouncementPriority.Silent) return;
            if (_disposed || string.IsNullOrWhiteSpace(message)) return;
            message = SanitizeForSpeech(message);
            if (priority == AnnouncementPriority.Critical)
                StopSpeaking();
            if (_jawsDetected)
                _ = Task.Run(async () => { await Task.Delay(500); ActuallySpeak(message); });
            else
                ActuallySpeak(message);
        }

        /// <summary>
        /// Speaks text synchronously (blocks until the utterance is complete).
        /// Used only for the app-closing goodbye so the message finishes before
        /// the process exits. Do not use this for anything else — it blocks the
        /// UI thread. No JAWS delay is applied since we are already shutting down.
        /// </summary>
        public void SpeakSync(string text)
        {
            if (_disposed || string.IsNullOrWhiteSpace(text)) return;
            text = SanitizeForSpeech(text);
            if (_azure.IsConfigured)
            {
                // Azure is async-only; fall back to SAPI for the goodbye so
                // we can guarantee the utterance completes before exit.
                using var sapi = new SpeechSynthesizer();
                try
                {
                    sapi.SetOutputToDefaultAudioDevice();
                    SelectBestSapiVoice(sapi);
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
            if (_disposed || string.IsNullOrWhiteSpace(text)) return;
            text = SanitizeForSpeech(text);

            if (_jawsDetected) await Task.Delay(500);

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

        /// <summary>
        /// Strips characters that cause SAPI to mispronounce or stutter:
        /// markdown formatting, parentheses, square brackets. Replaces slashes
        /// with "or" and em-dashes with commas for natural pacing.
        /// </summary>
        private static string SanitizeForSpeech(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Dashes used as separators → comma (natural SAPI pause)
            input = Regex.Replace(input, @"\s+[-–—]+\s+", ", ");

            // Forward slash → "or"
            input = input.Replace("/", " or ");

            // Strip markdown formatting characters
            input = input.Replace("#", "")
                         .Replace("*", "")
                         .Replace("_", "")
                         .Replace("`", "")
                         .Replace("~", "");

            // Strip parentheses and square brackets — keep inner content
            input = Regex.Replace(input, @"[\(\)\[\]]", "");

            // Collapse runs of whitespace created by stripping
            input = Regex.Replace(input, @"  +", " ");

            return input.Trim();
        }

        private void SelectBestSapiVoice() => SelectBestSapiVoice(_sapi);

        private static void SelectBestSapiVoice(SpeechSynthesizer synth)
        {
            var voices = synth.GetInstalledVoices()
                              .Where(v => v.Enabled)
                              .Select(v => v.VoiceInfo.Name)
                              .ToList();
            if (voices.Count == 0) return;

            string[] priorities =
            [
                "Natural", "Aria", "Jenny", "Guy", "Davis", "Jane",
                "Jason", "Zira", "Hazel", "Susan"
            ];

            foreach (var pref in priorities)
            {
                var match = voices.FirstOrDefault(
                    v => v.Contains(pref, StringComparison.OrdinalIgnoreCase));
                if (match != null) { synth.SelectVoice(match); return; }
            }
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
