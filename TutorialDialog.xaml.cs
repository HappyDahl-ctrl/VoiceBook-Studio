using System.Windows;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Views
{
    public partial class TutorialDialog : Window
    {
        public TutorialDialog()
        {
            InitializeComponent();
        }

        public TutorialViewModel ViewModel => (TutorialViewModel)DataContext;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TutorialViewModel vm)
                vm.Start();

            // Focus the Next button so JAWS announces "Next step" after reading the
            // live-region content, making it clear the app is waiting for user input.
            // Focusing StepContent would cause JAWS to re-read the instructions a second
            // time immediately after the live region already played them.
            NextButton.Focus();
        }
    }
}
