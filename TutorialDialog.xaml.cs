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

            // Focus the window rather than any child control. This lets JAWS read
            // the live regions (title assertive → content polite) without following
            // up with button text, so the voice cue ("Say 'Yes' to continue" etc.)
            // is the last thing the user hears — giving a clear silence signal to speak.
            Focus();
        }
    }
}
