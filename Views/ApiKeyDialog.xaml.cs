using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VoiceBookStudio.Services;

namespace VoiceBookStudio.Views
{
    public partial class ApiKeyDialog : Window
    {
        private bool _showing;

        public ApiKeyDialog()
        {
            InitializeComponent();

            // Pre-fill if key already stored (masked)
            string? existing = ApiKeyService.GetApiKey();
            if (!string.IsNullOrWhiteSpace(existing))
                KeyBox.Password = existing;

            Loaded += (_, _) =>
            {
                AnnouncementRegion.Text = "Configure Anthropic API key dialog is open.";
                KeyBox.Focus();
                KeyBox.SelectAll();
            };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string key = KeyBox.Password.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("Please enter an API key.", "Missing Key",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                KeyBox.Focus();
                return;
            }

            // Accept any key; Anthropic key formats can vary
            if (key.Length < 20)
            {
                MessageBox.Show(
                    "The key you entered looks too short. Anthropic API keys are typically 50 or more characters.\r\n\r\nCheck that you pasted the full key.",
                    "Key Too Short",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                KeyBox.Focus();
                return;
            }

            ApiKeyService.SaveApiKey(key);
            DialogResult = true;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Remove the saved API key? AI features will be disabled until you add a new key.",
                "Clear API Key",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            ApiKeyService.ClearApiKey();
            KeyBox.Password = string.Empty;
            DialogResult = false;
        }

        private void ShowHideButton_Click(object sender, RoutedEventArgs e)
        {
            // WPF PasswordBox doesn't support show-toggle natively.
            // Use a plain TextBox overlay instead.
            _showing = !_showing;
            ShowHideButton.Content = _showing ? "Hide" : "Show";

            if (_showing)
            {
                // Copy password into clipboard-safe TextBox shown on top
                string pwd = KeyBox.Password;
                KeyBox.Visibility = Visibility.Collapsed;

                // Dynamically build a visible TextBox if not already there
                if (FindName("VisibleKeyBox") is not TextBox tb)
                {
                    tb = new TextBox
                    {
                        Name    = "VisibleKeyBox",
                        FontSize = 13,
                        Padding  = new Thickness(4),
                        Text     = pwd
                    };
                    // Replace KeyBox in the grid
                    var parent = (Grid)KeyBox.Parent;
                    Grid.SetColumn(tb, 0);
                    parent.Children.Add(tb);
                    RegisterName("VisibleKeyBox", tb);
                }
                else
                {
                    tb.Text       = KeyBox.Password;
                    tb.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (FindName("VisibleKeyBox") is TextBox tb)
                {
                    KeyBox.Password = tb.Text;
                    tb.Visibility   = Visibility.Collapsed;
                }
                KeyBox.Visibility = Visibility.Visible;
                KeyBox.Focus();
            }
        }

        private void KeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveButton_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}
