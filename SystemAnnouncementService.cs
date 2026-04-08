using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Speaks system-level announcements (startup, project events, tutorial steps).
    /// Always speaks regardless of JAWS — these are critical notifications.
    ///
    /// Uses Azure Neural TTS when configured, otherwise SAPI with best available voice.
    /// </summary>
    public class SystemAnnouncementService
    {
        private readonly SpeechSynthesizer _sapi;
        private readonly AzureTtsService   _azure;
        private bool _disposed;

        public SystemAnnouncementService()
        {
            _sapi  = new SpeechSynthesizer();
            _azure = new AzureTtsService();

            _sapi.SetOutputToDefaultAudioDevice();
            _sapi.Rate   = 1;
            _sapi.Volume = 90;

            SelectBestSapiVoice();
            _azure.Configure();
        }

        public void ReconfigureAzure() => _azure.Configure();

        public void Speak(string text)
        {
            if (_disposed || string.IsNullOrWhiteSpace(text)) return;

            if (_azure.IsConfigured)
                _azure.Speak(text);
            else
                _sapi.SpeakAsync(text);
        }

        public async Task SpeakAndWaitAsync(string text)
        {
            if (_disposed || string.IsNullOrWhiteSpace(text)) return;

            if (_azure.IsConfigured)
                await _azure.SpeakAndWaitAsync(text);
            else
                await Task.Run(() => _sapi.Speak(text));
        }

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
