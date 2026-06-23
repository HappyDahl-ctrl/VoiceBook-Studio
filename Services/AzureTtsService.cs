using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Text-to-speech using Azure Cognitive Services neural voices.
    /// Far more natural-sounding than SAPI. Optional — falls back to SAPI when not configured.
    ///
    /// Supported neural voices (recommended):
    ///   en-US-AriaNeural   — warm, conversational female (default)
    ///   en-US-JennyNeural  — friendly, clear female
    ///   en-US-GuyNeural    — casual, expressive male
    ///   en-US-DavisNeural  — professional male
    ///   en-US-JaneNeural   — warm female
    ///   en-US-JasonNeural  — energetic male
    /// </summary>
    public sealed class AzureTtsService : IDisposable
    {
        private SpeechSynthesizer? _synth;
        private bool _disposed;

        // Tracks the Task for the most-recently-started fire-and-forget Speak() call so
        // WaitForCurrentSpeechAsync() can await it without starting a new utterance.
        private volatile Task<SynthesisResult>? _currentSpeechTask;

        public bool IsConfigured => AppSettings.IsAzureTtsConfigured && _synth != null;

        /// <summary>
        /// Call after AppSettings are loaded (or whenever the user saves new credentials).
        /// Safe to call multiple times — recreates the synthesizer with new settings.
        /// </summary>
        public void Configure()
        {
            DisposeSynth();
            if (!AppSettings.IsAzureTtsConfigured) return;

            try
            {
                var config = SpeechConfig.FromSubscription(
                    AppSettings.AzureSpeechKey,
                    AppSettings.AzureSpeechRegion);

                config.SpeechSynthesisVoiceName = AppSettings.AzureVoiceName;

                // Suppress Azure SDK console output
                config.SetProperty(PropertyId.Speech_LogFilename, string.Empty);

                _synth = new SpeechSynthesizer(config);
            }
            catch
            {
                _synth = null; // Bad credentials — fall back to SAPI gracefully
            }
        }

        /// <summary>Speak text asynchronously. Returns immediately; speech queues internally.</summary>
        public void Speak(string text)
        {
            if (_synth == null || _disposed || string.IsNullOrWhiteSpace(text)) return;
            // Store the task so WaitForCurrentSpeechAsync() can await it.
            _currentSpeechTask = _synth.SpeakTextAsync(text);
        }

        /// <summary>Speak and wait for completion — used for critical startup/tutorial announcements.</summary>
        public async Task SpeakAndWaitAsync(string text)
        {
            if (_synth == null || _disposed || string.IsNullOrWhiteSpace(text)) return;
            await _synth.SpeakTextAsync(text);
        }

        /// <summary>
        /// Awaits the completion of the most-recently-started fire-and-forget Speak() call.
        /// Returns immediately if nothing is speaking. Any Azure SDK exception is swallowed
        /// so a TTS failure never blocks tutorial progression.
        /// </summary>
        public async Task WaitForCurrentSpeechAsync()
        {
            // Capture local reference — a concurrent Speak() call may overwrite the field,
            // but we keep awaiting the task we started with, which is correct: if new speech
            // started, the previous utterance was either cancelled or completed, so this
            // Task will settle promptly either way.
            var task = _currentSpeechTask;
            if (task == null) return;
            try { await task.ConfigureAwait(false); }
            catch { /* Azure SDK exception or cancellation — treat as "speech ended" */ }
        }

        /// <summary>Cancel any in-progress or queued speech.</summary>
        public void StopSpeaking()
        {
            if (_synth != null && !_disposed)
                _ = _synth.StopSpeakingAsync();
        }

        private void DisposeSynth()
        {
            if (_synth != null)
            {
                _synth.Dispose();
                _synth = null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;  // prevent new Speak() calls before tearing down
                DisposeSynth();
            }
        }
    }
}
