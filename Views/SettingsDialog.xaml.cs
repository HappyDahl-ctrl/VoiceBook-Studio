using System;
using System.Windows;
using VoiceBookStudio.Services;
using VoiceBookStudio.Utils;
using WinForms = System.Windows.Forms;

namespace VoiceBookStudio.Views
{
    public partial class SettingsDialog : Window
    {
        private readonly SystemAnnouncementService _announcer;

        // Holds the candidate folder path until the user confirms with Save.
        private string _pendingFolder;

        public SettingsDialog(SystemAnnouncementService announcer)
        {
            InitializeComponent();
            _announcer     = announcer;
            _pendingFolder = AppSettings.DefaultProjectFolder;
            RefreshFolderDisplay();
        }

        // Announce the current folder path so JAWS / SAPI users hear it immediately.
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            AnnouncementRegion.Text = "Settings dialog is open.";

            string msg = string.IsNullOrWhiteSpace(AppSettings.DefaultProjectFolder)
                ? "Settings. Default project folder is not set. Use Browse to choose one."
                : $"Settings. Default project folder is {AppSettings.DefaultProjectFolder}";

            _announcer.Speak(msg);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new WinForms.FolderBrowserDialog
            {
                Description         = "Choose the default folder for new VoiceBook projects",
                ShowNewFolderButton = true
            };

            if (!string.IsNullOrWhiteSpace(_pendingFolder))
                dlg.SelectedPath = _pendingFolder;

            if (dlg.ShowDialog() == WinForms.DialogResult.OK)
            {
                _pendingFolder = dlg.SelectedPath;
                RefreshFolderDisplay();
                _announcer.Speak($"Selected: {_pendingFolder}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _pendingFolder = string.Empty;
            RefreshFolderDisplay();
            _announcer.Speak("Default project folder cleared. You will be prompted on each save.");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.DefaultProjectFolder = _pendingFolder;
            AppSettings.SaveJsonSettings();

            string msg = string.IsNullOrWhiteSpace(_pendingFolder)
                ? "Settings saved. No default project folder. You will be prompted on each save."
                : $"Settings saved. New projects will be saved to {_pendingFolder}";

            _announcer.Speak(msg);
            DialogResult = true;
            Close();
        }

        private void RefreshFolderDisplay()
        {
            FolderPathBox.Text = string.IsNullOrWhiteSpace(_pendingFolder)
                ? "(not set — you will be prompted on each save)"
                : _pendingFolder;
        }
    }
}
