using Microsoft.Win32;
using System.Windows;
using VoiceBookStudio.Services;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Views
{
    public partial class ProjectSelectionDialog : Window
    {
        public ProjectSelectionDialog()
        {
            InitializeComponent();
        }

        public ProjectSelectionViewModel ViewModel => (ProjectSelectionViewModel)DataContext;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ProjectsList.Focus();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedProject == null) return;
            DialogResult = true;
            Close();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            // Signal "create new" by setting DialogResult = false with SelectedProject null
            ViewModel.SelectedProject = null;
            DialogResult = false;
            Tag = "new";
            Close();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title      = "Open VoiceBook Project",
                Filter     = ProjectService.FileFilter,
                DefaultExt = ProjectService.FileExtension
            };
            if (dlg.ShowDialog() == true)
            {
                Tag = dlg.FileName;
                DialogResult = true;
                Close();
            }
        }
    }
}
