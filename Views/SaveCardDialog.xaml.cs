using System.Windows;

namespace VoiceBookStudio.Views
{
    public partial class SaveCardDialog : Window
    {
        public string CardTitle      { get; private set; } = string.Empty;
        public string CategoryName   { get; private set; } = string.Empty;

        public SaveCardDialog(System.Collections.Generic.IEnumerable<string> existingCategories,
                               string defaultTitle)
        {
            InitializeComponent();

            foreach (string cat in existingCategories)
                CategoryCombo.Items.Add(cat);

            if (CategoryCombo.Items.Count > 0)
                CategoryCombo.SelectedIndex = 0;

            TitleBox.Text = defaultTitle;

            Loaded += (_, _) =>
            {
                AnnouncementRegion.Text = "Save response as card dialog is open.";
                TitleBox.Focus();
                TitleBox.SelectAll();
            };
        }

        private void NewCategoryCheck_Changed(object sender, RoutedEventArgs e)
        {
            bool newCat = NewCategoryCheck.IsChecked == true;
            NewCategoryPanel.Visibility = newCat ? Visibility.Visible : Visibility.Collapsed;
            CategoryCombo.IsEnabled     = !newCat;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Please enter a card title.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleBox.Focus();
                return;
            }

            bool newCat = NewCategoryCheck.IsChecked == true;

            if (newCat)
            {
                if (string.IsNullOrWhiteSpace(NewCategoryNameBox.Text))
                {
                    MessageBox.Show("Please enter a category name.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    NewCategoryNameBox.Focus();
                    return;
                }
                CategoryName = NewCategoryNameBox.Text.Trim();
            }
            else
            {
                CategoryName = CategoryCombo.SelectedItem?.ToString() ?? "General";
            }

            CardTitle    = TitleBox.Text.Trim();
            DialogResult = true;
        }
    }
}
