using System;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Provides text-to-speech audio feedback for user actions.
    ///
    /// Voice priority:
    ///   1. Azure Neural TTS (when configured) — most natural, human-like
    ///   2. Windows SAPI with best available voice — prefers any installed
    ///      neural/natural voice (e.g. Microsoft Aria Natural), then standard voices
    ///
    /// When JAWS is detected, this service produces NO audio output and never
    /// initializes a SpeechSynthesizer, because a SAPI instance competing with
    /// JAWS on the audio device causes JAWS stuttering. JAWS handles all speech
    /// via UIA live regions instead.
    ///
    /// AppSoundService (sine-wave audio cues) remains active when JAWS is running;
    /// tones are non-speech and do not share JAWS's speech audio channel.
    /// </summary>
    public class AudioFeedbackService : IDisposable
    {
        // Lazy-initialized — never created when JAWS is detected.
        private SpeechSynthesizer? _sapi;
        private readonly object    _sapiLock = new();

        // Tracks the most-recently-started SAPI Prompt so WaitForCurrentSpeechAsync()
        // can filter SpeakCompleted events by identity and avoid acting on stale events
        // from previously-cancelled utterances.
        private volatile Prompt? _currentPrompt;

        private readonly AzureTtsService _azure;
        private bool _disposed;

        // Backing fields hold Rate/Volume before _sapi is created.
        private int _rate   = 1;
        private int _volume = 80;

        /// <summary>Whether app TTS is active. Toggle from Settings menu.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Speech rate for SAPI fallback (-10 to 10).</summary>
        public int Rate
        {
            get => _sapi?.Rate ?? _rate;
            set
            {
                _rate = Math.Clamp(value, -10, 10);
                if (_sapi != null) _sapi.Rate = _rate;
            }
        }

        /// <summary>Volume for SAPI fallback (0–100).</summary>
        public int Volume
        {
            get => _sapi?.Volume ?? _volume;
            set
            {
                _volume = Math.Clamp(value, 0, 100);
                if (_sapi != null) _sapi.Volume = _volume;
            }
        }

        public AudioFeedbackService()
        {
            _azure = new AzureTtsService();
            _azure.Configure();
            // _sapi intentionally not created here. EnsureSapi() creates it on first
            // Speak call only when JAWS is not detected.
        }

        /// <summary>
        /// Reconfigure Azure TTS after the user saves new credentials.
        /// </summary>
        public void ReconfigureAzure() => _azure.Configure();

        /// <summary>
        /// Called once at startup with the JAWS detection result.
        /// When detected == true: disposes any existing SAPI instance immediately and
        /// permanently blocks future creation. All Speak calls become no-ops.
        /// When detected == false: SAPI will be created lazily on first Speak call.
        /// </summary>
        public void SetJawsDetected(bool detected)
        {
            AppSettings.IsJawsDetected = detected;
            if (detected)
            {
                lock (_sapiLock)
                {
                    if (_sapi != null)
                    {
                        _sapi.SpeakAsyncCancelAll();
                        _sapi.Dispose();
                        _sapi = null;
                    }
                }
            }
        }

        /// <summary>True when JAWS is in the running process list.</summary>
        public static bool IsJawsRunning() =>
            Process.GetProcessesByName("jfw").Length  > 0
            || Process.GetProcessesByName("jaws").Length > 0;

        /// <summary>
        /// Speak asynchronously and interruptibly. Cancels any in-progress or
        /// queued speech before starting (max one pending utterance).
        /// No-op when JAWS is detected.
        /// </summary>
        public void Speak(string text)
        {
            if (AppSettings.IsJawsDetected) return;
            if (!IsEnabled || _disposed || string.IsNullOrWhiteSpace(text)) return;
            text = SanitizeForSpeech(text);
            if (_azure.IsConfigured)
            {
                _azure.StopSpeaking();
                _azure.Speak(text);
            }
            else
            {
                var sapi = EnsureSapi();
                sapi.SpeakAsyncCancelAll(); // interrupt current + discard queue
                _currentPrompt = sapi.SpeakAsync(text);
            }
        }

        /// <summary>
        /// Awaits completion of the most-recently-started fire-and-forget Speak() call.
        /// Returns immediately when nothing is speaking, when JAWS is detected, or when
        /// the synthesizer has not yet been used. Any exception degrades to an immediate
        /// return so a TTS failure never blocks tutorial progression.
        ///
        /// SAPI safeguards:
        ///   - Subscribes to SpeakCompleted BEFORE checking synthesizer state to close the
        ///     race window where speech ends between the check and the subscription.
        ///   - Filters events by Prompt identity so stale SpeakCompleted events from
        ///     previously-cancelled utterances do not trigger a premature return.
        ///   - SpeakCompleted fires for both natural completion and cancellation
        ///     (SpeakAsyncCancelAll), so interruption is handled correctly.
        ///   - Falls back to a 10-second timeout to prevent freezing on any SAPI edge case.
        /// </summary>
        public async Task WaitForCurrentSpeechAsync()
        {
            if (AppSettings.IsJawsDetected) return;
            if (!IsEnabled || _disposed) return;

            if (_azure.IsConfigured)
            {
                await _azure.WaitForCurrentSpeechAsync().ConfigureAwait(false);
                return;
            }

            var sapi   = _sapi;
            var prompt = _currentPrompt;
            if (sapi == null || prompt == null) return;

            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void OnCompleted(object? s, SpeakCompletedEventArgs e)
            {
                // Ignore events for previously-cancelled utterances; only act on ours.
                if (!ReferenceEquals(e.Prompt, prompt)) return;
                sapi.SpeakCompleted -= OnCompleted;
                // Fires for both natural completion and SpeakAsyncCancelAll interruption.
                tcs.TrySetResult(true);
            }

            // Subscribe FIRST, then check state, so we cannot miss the completion event
            // if speech finishes in the gap between the two operations.
            sapi.SpeakCompleted += OnCompleted;

            // If speech already ended before we subscribed, Prompt.IsCompleted is true.
            // Complete the TCS immediately rather than waiting for an event that won't fire.
            if (prompt.IsCompleted)
            {
                sapi.SpeakCompleted -= OnCompleted;
                return;
            }

            try
            {
                await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                // Edge-case fallback: advance slightly early rather than freezing.
                sapi.SpeakCompleted -= OnCompleted;
            }
        }

        /// <summary>Immediately halt all speech.</summary>
        public void Stop()
        {
            if (_disposed) return;
            if (_azure.IsConfigured)
                _azure.StopSpeaking();
            else
                _sapi?.SpeakAsyncCancelAll();
        }

        /// <summary>Alias for Stop(). Preserves existing call sites.</summary>
        public void StopSpeaking() => Stop();

        /// <summary>
        /// Speak a single utterance and await its completion.
        /// Used for startup greetings. No-op when JAWS is detected.
        /// </summary>
        public async Task SpeakAndWaitAsync(string text)
        {
            if (AppSettings.IsJawsDetected) return;
            if (!IsEnabled || _disposed || string.IsNullOrWhiteSpace(text)) return;
            text = SanitizeForSpeech(text);
            if (_azure.IsConfigured)
            {
                await _azure.SpeakAndWaitAsync(text);
                return;
            }
            var sapi = EnsureSapi();
            await SpeakSapiAndWaitAsync(sapi, text, CancellationToken.None);
        }

        /// <summary>
        /// Reads multi-sentence text aloud, pausing 250 ms between sentences.
        /// Returns immediately when the cancellation token is signalled, so the
        /// caller can wire "stop reading" to cancel the token.
        /// No-op when JAWS is detected.
        /// </summary>
        public async Task ReadTextAloud(string text, CancellationToken token)
        {
            if (AppSettings.IsJawsDetected) return;
            if (!IsEnabled || _disposed || string.IsNullOrWhiteSpace(text)) return;

            string[] sentences = SplitIntoSentences(SanitizeForSpeech(text));

            foreach (string sentence in sentences)
            {
                if (token.IsCancellationRequested) return;
                if (string.IsNullOrWhiteSpace(sentence)) continue;

                bool completed;
                if (_azure.IsConfigured)
                {
                    // Register cancellation so StopSpeaking fires when token is cancelled.
                    using var reg = token.Register(() => _azure.StopSpeaking());
                    await _azure.SpeakAndWaitAsync(sentence);
                    completed = !token.IsCancellationRequested;
                }
                else
                {
                    completed = await SpeakSapiAndWaitAsync(EnsureSapi(), sentence, token);
                }

                if (!completed || token.IsCancellationRequested) return;

                try { await Task.Delay(250, token); }
                catch (OperationCanceledException) { return; }
            }
        }

        // ----------------------------------------------------------------
        // Private helpers
        // ----------------------------------------------------------------

        private SpeechSynthesizer EnsureSapi()
        {
            if (_sapi != null) return _sapi;
            lock (_sapiLock)
            {
                if (_sapi != null) return _sapi;
                _sapi = new SpeechSynthesizer();
                _sapi.SetOutputToDefaultAudioDevice();
                _sapi.Rate   = _rate;
                _sapi.Volume = _volume;
                SelectBestSapiVoice();
                return _sapi;
            }
        }

        /// <summary>
        /// Speaks one utterance via SAPI and awaits completion.
        /// Returns true when speech completed normally, false when cancelled or interrupted.
        /// Uses Prompt identity to distinguish completion events for our utterance
        /// from stale events firing from a prior SpeakAsyncCancelAll call.
        /// </summary>
        private static async Task<bool> SpeakSapiAndWaitAsync(
            SpeechSynthesizer sapi, string text, CancellationToken token)
        {
            if (token.IsCancellationRequested) return false;

            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            Prompt? ourPrompt = null;

            void OnCompleted(object? s, SpeakCompletedEventArgs e)
            {
                // Ignore SpeakCompleted events that belong to previously cancelled
                // utterances; only act on the event for our own Prompt.
                if (!ReferenceEquals(e.Prompt, ourPrompt)) return;
                sapi.SpeakCompleted -= OnCompleted;
                tcs.TrySetResult(!e.Cancelled);
            }

            // Cancel previous speech before subscribing so no stale
            // SpeakCompleted event fires after we attach OnCompleted.
            sapi.SpeakAsyncCancelAll();
            sapi.SpeakCompleted += OnCompleted;
            ourPrompt = sapi.SpeakAsync(text);

            using var reg = token.Register(() =>
            {
                sapi.SpeakCompleted -= OnCompleted;
                sapi.SpeakAsyncCancelAll();
                tcs.TrySetResult(false);
            });

            try
            {
                return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));
            }
            catch
            {
                sapi.SpeakAsyncCancelAll();
                return false;
            }
            finally
            {
                reg.Dispose();
                sapi.SpeakCompleted -= OnCompleted;
            }
        }

        private static string[] SplitIntoSentences(string text) =>
            Regex.Split(text.Trim(), @"(?<=[.!?])\s+");

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

        /// <summary>
        /// Picks the best available SAPI voice in priority order:
        ///   1. Any voice containing "Natural" (Windows 11 neural offline voices)
        ///   2. Aria, Jenny, Guy, Davis  (neural voices by name)
        ///   3. Zira (cleaner female standard voice)
        ///   4. Default voice
        /// </summary>
        private void SelectBestSapiVoice()
        {
            if (_sapi == null) return;
            var voices = _sapi.GetInstalledVoices()
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
                if (match != null) { _sapi.SelectVoice(match); return; }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_sapiLock)
            {
                _sapi?.SpeakAsyncCancelAll();
                _sapi?.Dispose();
                _sapi = null;
            }
            _azure.Dispose();
        }
    }
}
