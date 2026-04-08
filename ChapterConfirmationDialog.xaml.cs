using System.Windows;
using VoiceBookStudio.ViewModels;
using VoiceBookStudio.Services;
using System.Collections.Generic;
using VoiceBookStudio.Models;

namespace VoiceBookStudio.Views
{
    public partial class ChapterConfirmationDialog : Window
    {
        public ChapterConfirmationDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChaptersList.Focus();
        }

        public ChapterConfirmationViewModel ViewModel => (ChapterConfirmationViewModel)DataContext;

        public bool AcceptedAll { get; private set; }
        public bool ImportSingle { get; private set; }

        public void SetChapters(List<DetectedChapter> chapters)
        {
            ChaptersList.ItemsSource = chapters;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            AcceptedAll = true;
            DialogResult = true;
            Close();
        }

        private void SingleButton_Click(object sender, RoutedEventArgs e)
        {
            ImportSingle = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AcceptedAll = false;
            ImportSingle = false;
            DialogResult = false;
            Close();
        }
    }
}
