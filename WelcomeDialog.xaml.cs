using System.Windows;
using VoiceBookStudio.Utils;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Views
{
    public partial class WelcomeDialog : Window
    {
        public WelcomeDialog()
        {
            InitializeComponent();
        }

        public WelcomeDialogViewModel ViewModel => (WelcomeDialogViewModel)DataContext;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is WelcomeDialogViewModel vm)
                ShowOnStartupCheckbox.IsChecked = vm.ShowOnStartup;

            // Put focus on the primary action so JAWS reads it immediately
            StartButton.Focus();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Save the checkbox preference before closing
            if (DataContext is WelcomeDialogViewModel vm)
            {
                AppSettings.ShowWelcomeOnStartup = vm.ShowOnStartup;
                AppSettings.Save();
            }
            DialogResult = true;
            Close();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is WelcomeDialogViewModel vm)
            {
                AppSettings.ShowWelcomeOnStartup = vm.ShowOnStartup;
                AppSettings.Save();
            }
            DialogResult = false;
            Close();
        }
    }
}
