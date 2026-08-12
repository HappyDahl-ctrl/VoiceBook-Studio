using System;
using System.Windows;
using VoiceBookStudio.Models;
using VoiceBookStudio.Services;
using VoiceBookStudio.Utils;
using WinForms = System.Windows.Forms;

namespace VoiceBookStudio.Views
{
    public partial class SettingsDialog : Window
    {
        private readonly SystemAnnouncementService _announcer;
        private readonly VoiceBookProject?          _project;

        // Holds each candidate folder path until the user confirms with Save.
        private string _pendingDefaultFolder;
        private string _pendingExportFolder;
        private string _pendingProjectFolder;

        /// <param name="project">
        /// The currently open project, if any. When null, the per-project save
        /// folder section is disabled with an explanation instead of hidden
        /// outright, so JAWS/Dragon users understand why it's unavailable.
        /// </param>
        public SettingsDialog(SystemAnnouncementService announcer, VoiceBookProject? project = null)
        {
            InitializeComponent();
            _announcer     = announcer;
            _project       = project;

            _pendingDefaultFolder = AppSettings.DefaultProjectFolder;
            _pendingExportFolder  = AppSettings.DefaultExportFolder;
            _pendingProjectFolder = project != null
                ? AppSettings.GetProjectFolderOverride(project.Id)
                : string.Empty;

            RefreshFolderDisplay();
            RefreshExportFolderDisplay();
            RefreshProjectFolderDisplay();

            if (_project == null)
            {
                ProjectFolderGroup.IsEnabled = false;
                ProjectFolderHeader.Text     = "This Project's Save Folder (no project is open)";
            }
            else
            {
                ProjectFolderHeader.Text = $"This Project's Save Folder — {_project.Title}";
            }
        }

        // Announce the current settings so JAWS / SAPI users hear them immediately.
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            AnnouncementRegion.Text = "Settings dialog is open.";

            string msg = string.IsNullOrWhiteSpace(AppSettings.DefaultProjectFolder)
                ? "Settings. Default project folder is not set. Use Browse to choose one."
                : $"Settings. Default project folder is {AppSettings.DefaultProjectFolder}";

            if (_project != null)
            {
                msg += string.IsNullOrWhiteSpace(_pendingProjectFolder)
                    ? $" This project, {_project.Title}, has no separate save folder set."
                    : $" This project's save folder is {_pendingProjectFolder}";
            }

            _announcer.Speak(msg);
        }

        // ----------------------------------------------------------------
        // Default project folder
        // ----------------------------------------------------------------

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new WinForms.FolderBrowserDialog
            {
                Description         = "Choose the default folder for new VoiceBook projects",
                ShowNewFolderButton = true
            };

            if (!string.IsNullOrWhiteSpace(_pendingDefaultFolder))
                dlg.SelectedPath = _pendingDefaultFolder;

            if (dlg.ShowDialog() == WinForms.DialogResult.OK)
            {
                _pendingDefaultFolder = dlg.SelectedPath;
                RefreshFolderDisplay();
                _announcer.Speak($"Selected: {_pendingDefaultFolder}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _pendingDefaultFolder = string.Empty;
            RefreshFolderDisplay();
            _announcer.Speak("Default project folder cleared. You will be prompted on each save.");
        }

        private void RefreshFolderDisplay()
        {
            FolderPathBox.Text = string.IsNullOrWhiteSpace(_pendingDefaultFolder)
                ? "(not set — you will be prompted on each save)"
                : _pendingDefaultFolder;
        }

        // ----------------------------------------------------------------
        // Default export folder
        // ----------------------------------------------------------------

        private void BrowseExportButton_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new WinForms.FolderBrowserDialog
            {
                Description         = "Choose the default folder for exported Word and PDF files",
                ShowNewFolderButton = true
            };

            if (!string.IsNullOrWhiteSpace(_pendingExportFolder))
                dlg.SelectedPath = _pendingExportFolder;

            if (dlg.ShowDialog() == WinForms.DialogResult.OK)
            {
                _pendingExportFolder = dlg.SelectedPath;
                RefreshExportFolderDisplay();
                _announcer.Speak($"Selected: {_pendingExportFolder}");
            }
        }

        private void ClearExportButton_Click(object sender, RoutedEventArgs e)
        {
            _pendingExportFolder = string.Empty;
            RefreshExportFolderDisplay();
            _announcer.Speak("Default export folder cleared. Export dialogs will use their own default location.");
        }

        private void RefreshExportFolderDisplay()
        {
            ExportFolderPathBox.Text = string.IsNullOrWhiteSpace(_pendingExportFolder)
                ? "(not set — export dialogs use their own default location)"
                : _pendingExportFolder;
        }

        // ----------------------------------------------------------------
        // Per-project save folder
        // ----------------------------------------------------------------

        private void BrowseProjectButton_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new WinForms.FolderBrowserDialog
            {
                Description         = _project != null
                    ? $"Choose the save folder for {_project.Title}"
                    : "Choose the save folder for this project",
                ShowNewFolderButton = true
            };

            if (!string.IsNullOrWhiteSpace(_pendingProjectFolder))
                dlg.SelectedPath = _pendingProjectFolder;

            if (dlg.ShowDialog() == WinForms.DialogResult.OK)
            {
                _pendingProjectFolder = dlg.SelectedPath;
                RefreshProjectFolderDisplay();
                _announcer.Speak($"Selected: {_pendingProjectFolder}");
            }
        }

        private void ClearProjectButton_Click(object sender, RoutedEventArgs e)
        {
            _pendingProjectFolder = string.Empty;
            RefreshProjectFolderDisplay();
            _announcer.Speak("This project's save folder override cleared. It will use the default project folder, or wherever it currently lives.");
        }

        private void RefreshProjectFolderDisplay()
        {
            ProjectFolderPathBox.Text = string.IsNullOrWhiteSpace(_pendingProjectFolder)
                ? "(not set — uses the default project folder, or wherever this project currently lives)"
                : _pendingProjectFolder;
        }

        // ----------------------------------------------------------------
        // Save / Cancel
        // ----------------------------------------------------------------

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.DefaultProjectFolder = _pendingDefaultFolder;
            AppSettings.DefaultExportFolder  = _pendingExportFolder;
            if (_project != null)
                AppSettings.SetProjectFolderOverride(_project.Id, _pendingProjectFolder);
            AppSettings.SaveJsonSettings();

            string msg = string.IsNullOrWhiteSpace(_pendingDefaultFolder)
                ? "Settings saved. No default project folder. You will be prompted on each save."
                : $"Settings saved. New projects will be saved to {_pendingDefaultFolder}";

            _announcer.Speak(msg);
            DialogResult = true;
            Close();
        }
    }
}
