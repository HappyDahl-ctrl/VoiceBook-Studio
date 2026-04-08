using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VoiceBookStudio.Services;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.Views
{
    public partial class AzureTtsDialog : Window
    {
        private readonly AudioFeedbackService     _audio;
        private readonly SystemAnnouncementService _announce;

        public AzureTtsDialog(AudioFeedbackService audio, SystemAnnouncementService announce)
        {
            InitializeComponent();
            _audio    = audio;
            _announce = announce;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Pre-fill from saved settings
            KeyBox.Text    = AppSettings.AzureSpeechKey;
            RegionBox.Text = AppSettings.AzureSpeechRegion;

            // Select the saved voice
            foreach (ComboBoxItem item in VoiceCombo.Items)
            {
                if (item.Tag?.ToString() == AppSettings.AzureVoiceName)
                {
                    VoiceCombo.SelectedItem = item;
                    break;
                }
            }

            UpdateStatus();
            KeyBox.Focus();
        }

        private void UpdateStatus()
        {
            if (AppSettings.IsAzureTtsConfigured)
                StatusLabel.Text = $"Azure TTS active — voice: {AppSettings.AzureVoiceName}";
            else
                StatusLabel.Text = "Using Windows SAPI voice (Azure not configured)";
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            // Apply current dialog values temporarily for test
            string key    = KeyBox.Text.Trim();
            string region = RegionBox.Text.Trim();
            string voice  = SelectedVoiceTag();

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
            {
                // Test SAPI fallback
                StatusLabel.Text = "Testing Windows SAPI voice…";
                _audio.Speak("Hello! This is the Windows voice for VoiceBook Studio.");
                return;
            }

            StatusLabel.Text = "Testing Azure voice — please wait…";
            TestButton.IsEnabled = false;

            // Temporarily configure a test synthesizer
            var prev = (AppSettings.AzureSpeechKey, AppSettings.AzureSpeechRegion, AppSettings.AzureVoiceName);
            AppSettings.AzureSpeechKey    = key;
            AppSettings.AzureSpeechRegion = region;
            AppSettings.AzureVoiceName    = voice;

            using var tester = new AzureTtsService();
            tester.Configure();

            if (tester.IsConfigured)
            {
                await tester.SpeakAndWaitAsync(
                    $"Hello! This is {voice.Replace("en-US-", "").Replace("Neural", "")} speaking. " +
                    "VoiceBook Studio is ready to help you write your book.");
                StatusLabel.Text = "Test complete. Save to keep this voice.";
            }
            else
            {
                StatusLabel.Text = "Could not connect. Check your key and region.";
            }

            // Restore previous settings (user hasn't saved yet)
            (AppSettings.AzureSpeechKey, AppSettings.AzureSpeechRegion, AppSettings.AzureVoiceName) = prev;
            TestButton.IsEnabled = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            KeyBox.Text    = string.Empty;
            RegionBox.Text = string.Empty;

            AppSettings.AzureSpeechKey    = string.Empty;
            AppSettings.AzureSpeechRegion = string.Empty;
            AppSettings.AzureVoiceName    = "en-US-AriaNeural";
            AppSettings.Save();

            _audio.ReconfigureAzure();
            _announce.ReconfigureAzure();

            StatusLabel.Text = "Azure cleared. Using Windows SAPI voice.";
            _audio.Speak("Azure voice cleared. Using Windows voice.");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.AzureSpeechKey    = KeyBox.Text.Trim();
            AppSettings.AzureSpeechRegion = RegionBox.Text.Trim();
            AppSettings.AzureVoiceName    = SelectedVoiceTag();
            AppSettings.Save();

            _audio.ReconfigureAzure();
            _announce.ReconfigureAzure();

            if (AppSettings.IsAzureTtsConfigured)
                _audio.Speak("Azure neural voice saved and active.");
            else
                _audio.Speak("Settings saved. Using Windows voice.");

            DialogResult = true;
            Close();
        }

        private string SelectedVoiceTag() =>
            (VoiceCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString()
            ?? "en-US-AriaNeural";
    }
}
