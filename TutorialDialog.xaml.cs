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

            // Focus the content area so JAWS reads the first step immediately
            StepContent.Focus();
        }
    }
}
