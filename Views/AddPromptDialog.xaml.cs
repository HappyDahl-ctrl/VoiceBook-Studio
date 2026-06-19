using System.Windows;

namespace VoiceBookStudio.Views
{
    public partial class AddPromptDialog : Window
    {
        // Results set when the user clicks OK
        public string PromptTitle    { get; private set; } = string.Empty;
        public string PromptContent  { get; private set; } = string.Empty;
        public string CategoryLetter { get; private set; } = string.Empty;
        public string CategoryName   { get; private set; } = string.Empty;

        public AddPromptDialog(System.Collections.Generic.IEnumerable<(string Letter, string Name)> existingCategories,
                                string suggestedNextLetter)
        {
            InitializeComponent();

            foreach (var (letter, name) in existingCategories)
                CategoryCombo.Items.Add(new CategoryItem(letter, name));

            NewCategoryLetterBox.Text = suggestedNextLetter;

            if (CategoryCombo.Items.Count > 0)
                CategoryCombo.SelectedIndex = 0;

            Loaded += (_, _) =>
            {
                AnnouncementRegion.Text = "Add new prompt dialog is open.";
                TitleBox.Focus();
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
                MessageBox.Show("Please enter a prompt title.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ContentBox.Text))
            {
                MessageBox.Show("Please enter the prompt text.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ContentBox.Focus();
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
                if (string.IsNullOrWhiteSpace(NewCategoryLetterBox.Text))
                {
                    MessageBox.Show("Please enter a category letter.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    NewCategoryLetterBox.Focus();
                    return;
                }
                CategoryLetter = NewCategoryLetterBox.Text.ToUpper().Trim();
                CategoryName   = NewCategoryNameBox.Text.Trim();
            }
            else
            {
                if (CategoryCombo.SelectedItem is CategoryItem item)
                {
                    CategoryLetter = item.Letter;
                    CategoryName   = item.Name;
                }
                else
                {
                    MessageBox.Show("Please select a category.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            PromptTitle   = TitleBox.Text.Trim();
            PromptContent = ContentBox.Text.Trim();
            DialogResult  = true;
        }

        private sealed class CategoryItem
        {
            public string Letter { get; }
            public string Name   { get; }
            public CategoryItem(string letter, string name) { Letter = letter; Name = name; }
            public override string ToString() => $"{Letter}: {Name}";
        }
    }
}
