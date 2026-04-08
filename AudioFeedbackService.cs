using System;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
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
    ///      neural/natural voice (e.g. Microsoft Aria Natural), then falls
    ///      back to standard voices (Zira, David, Mark)
    ///
    /// When JAWS is detected at startup this service is silenced;
    /// JAWS handles all audio via UI Automation live regions.
    /// </summary>
    public class AudioFeedbackService : IDisposable
    {
        private readonly SpeechSynthesizer _sapi;
        private readonly AzureTtsService  _azure;
        private bool _disposed;

        /// <summary>Whether app TTS is active. Toggle from Settings menu.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Speech rate for SAPI fallback (-10 to 10).</summary>
        public int Rate
        {
            get => _sapi.Rate;
            set => _sapi.Rate = Math.Clamp(value, -10, 10);
        }

        /// <summary>Volume for SAPI fallback (0-100).</summary>
        public int Volume
        {
            get => _sapi.Volume;
            set => _sapi.Volume = Math.Clamp(value, 0, 100);
        }

        public AudioFeedbackService()
        {
            _sapi  = new SpeechSynthesizer();
            _azure = new AzureTtsService();

            _sapi.SetOutputToDefaultAudioDevice();
            _sapi.Rate   = 1;
            _sapi.Volume = 80;

            SelectBestSapiVoice();
            _azure.Configure();
        }

        /// <summary>
        /// Reconfigure Azure TTS after the user saves new credentials.
        /// Call this whenever AppSettings.Azure* values change.
        /// </summary>
        public void ReconfigureAzure() => _azure.Configure();

        /// <summary>True when JAWS is currently running.</summary>
        public static bool IsJawsRunning() =>
            Process.GetProcessesByName("jfw").Length > 0;

        /// <summary>Speak asynchronously. Silenced when JAWS detected at startup.</summary>
        public void Speak(string text)
        {
            if (AppSettings.IsJawsDetected) return;
            if (!IsEnabled || _disposed || string.IsNullOrWhiteSpace(text)) return;

            if (_azure.IsConfigured)
                _azure.Speak(text);
            else
                _sapi.SpeakAsync(text);
        }

        /// <summary>Cancel any queued speech.</summary>
        public void StopSpeaking()
        {
            if (_disposed) return;
            if (_azure.IsConfigured)
                _azure.StopSpeaking();
            else
                _sapi.SpeakAsyncCancelAll();
        }

        /// <summary>Speak synchronously — only for startup greetings.</summary>
        public async Task SpeakAndWaitAsync(string text)
        {
            if (AppSettings.IsJawsDetected) return;
            if (!IsEnabled || _disposed || string.IsNullOrWhiteSpace(text)) return;

            if (_azure.IsConfigured)
                await _azure.SpeakAndWaitAsync(text);
            else
                await Task.Run(() => _sapi.Speak(text));
        }

        // ----------------------------------------------------------------
        // Voice selection helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Picks the best available SAPI voice in priority order:
        ///   1. Any voice containing "Natural" (Windows 11 neural offline voices)
        ///   2. Aria, Jenny, Guy, Davis  (neural voices by name)
        ///   3. Zira (cleaner female standard voice)
        ///   4. First available voice
        /// </summary>
        private void SelectBestSapiVoice()
        {
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
                if (match != null)
                {
                    _sapi.SelectVoice(match);
                    return;
                }
            }
            // Fall through — keep default voice
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
