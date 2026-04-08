using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VoiceBookStudio.Services;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Views
{
    public partial class MainWindow : Window
    {
        // Last known caret position in EditorBox before the user switched
        // to the AI panel. Used by the "Insert at Cursor" command.
        private int  _lastEditorCaretIndex;
        private bool _suppressEditorSync;

        private MainViewModel    ViewModel => (MainViewModel)DataContext;
        private VoiceCommandRouter? _voiceRouter;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                ViewModel.InsertTextRequested    += OnInsertTextRequested;
                ViewModel.FocusPanelRequested    += OnFocusPanelRequested;
                ViewModel.SwitchAiTabRequested   += OnSwitchAiTabRequested;
                ViewModel.FocusChatInputRequested += (_, _) => ChatInputBox.Focus();

                // Bind the Response Cards panel to its ViewModel.
                ResponseCardPanelInstance.DataContext = ViewModel.ResponseCardVM;

                _voiceRouter = new VoiceCommandRouter(ViewModel);
            };

            KeyDown += MainWindow_KeyDown;
        }

        // ----------------------------------------------------------------
        // Chapter list
        // ----------------------------------------------------------------

        private void ChapterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var selected = e.AddedItems[0] as ChapterViewModel;

            _suppressEditorSync = true;
            ViewModel.OnChapterSelected(selected);
            EditorBox.Text      = selected?.Content ?? string.Empty;
            _suppressEditorSync = false;

            _lastEditorCaretIndex = 0;
        }

        // ----------------------------------------------------------------
        // Editor
        // ----------------------------------------------------------------

        private void EditorBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressEditorSync) return;
            ViewModel.OnEditorTextChanged(EditorBox.Text);
        }

        // Save cursor position whenever the editor loses focus so the
        // "Insert at Cursor" command knows where to place the text.
        private void EditorBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _lastEditorCaretIndex = EditorBox.CaretIndex;
        }

        // ----------------------------------------------------------------
        // Insert AI response into the editor
        // ----------------------------------------------------------------

        private void OnInsertTextRequested(object? sender, InsertTextArgs e)
        {
            // Wrap the inserted block in paragraph breaks
            string block = Environment.NewLine + Environment.NewLine +
                           e.Text.Trim()       +
                           Environment.NewLine + Environment.NewLine;

            int pos = e.Position switch
            {
                InsertPosition.AtCursor    => Math.Clamp(_lastEditorCaretIndex, 0, EditorBox.Text.Length),
                InsertPosition.AtBeginning => 0,
                InsertPosition.AtEnd       => EditorBox.Text.Length,
                _                          => EditorBox.Text.Length
            };

            _suppressEditorSync  = true;
            EditorBox.Text       = EditorBox.Text.Insert(pos, block);
            EditorBox.CaretIndex = pos + block.Length;
            _suppressEditorSync  = false;

            ViewModel.OnEditorTextChanged(EditorBox.Text);

            // Return focus to editor so JAWS announces the updated position
            EditorBox.Focus();
        }

        // ----------------------------------------------------------------
        // Panel focus — Ctrl+1 / Ctrl+2 / Ctrl+3
        // Dragon NaturallySpeaking: configure "Panel 1" → press Ctrl+1 etc.
        // JSay: map the same phrases to these shortcuts.
        // ----------------------------------------------------------------

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            if (e.Key == Key.D1 || e.Key == Key.NumPad1) { ViewModel.FocusPanel1(); e.Handled = true; }
            else if (e.Key == Key.D2 || e.Key == Key.NumPad2) { ViewModel.FocusPanel2(); e.Handled = true; }
            else if (e.Key == Key.D3 || e.Key == Key.NumPad3) { ViewModel.FocusPanel3(); e.Handled = true; }
        }

        private void OnFocusPanelRequested(object? sender, int panelNumber)
        {
            switch (panelNumber)
            {
                case 1: ChapterListBox.Focus(); break;
                case 2: EditorBox.Focus();      break;
                case 3: ChatInputBox.Focus();   break;
            }
        }

        private void OnSwitchAiTabRequested(object? sender, string tabName)
        {
            AiTabControl.SelectedIndex = tabName switch
            {
                "Prompts" => 1,
                "Cards"   => 2,
                _         => 0   // "Chat"
            };
        }

        // ----------------------------------------------------------------
        // Chat input
        // ----------------------------------------------------------------

        private void ChatInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Enter (without Shift) — try voice command first, then send to Claude
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                string text = ChatInputBox.Text.Trim();

                // If the text starts with '!' treat it explicitly as a voice command
                bool isExplicitCommand = text.StartsWith("!");
                string commandText = isExplicitCommand ? text[1..].Trim() : text;

                if (_voiceRouter != null && isExplicitCommand)
                {
                    if (_voiceRouter.TryRoute(commandText))
                        ChatInputBox.Clear();
                    else
                        ViewModel.OnCommandNotRecognized(commandText);
                    e.Handled = true;
                    return;
                }

                // Otherwise send as a chat message to Claude
                if (ViewModel.SendChatCommand.CanExecute(null))
                    ViewModel.SendChatCommand.Execute(null);

                e.Handled = true;
            }
        }

        // ----------------------------------------------------------------
        // Menu / misc handlers
        // ----------------------------------------------------------------

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowShortcuts_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "KEYBOARD SHORTCUTS\r\n\r\n" +
                "File\r\n" +
                "  Ctrl+N        New Project\r\n" +
                "  Ctrl+O        Open Project\r\n" +
                "  Ctrl+S        Save\r\n" +
                "  Ctrl+Shift+S  Save As\r\n" +
                "  Ctrl+I        Import .docx Document\r\n\r\n" +
                "Chapters\r\n" +
                "  Ctrl+A        Add Chapter / Section\r\n" +
                "  Ctrl+D        Rename Chapter\r\n" +
                "  Ctrl+Delete   Delete Chapter\r\n" +
                "  Alt+Up        Move Chapter Up\r\n" +
                "  Alt+Down      Move Chapter Down\r\n\r\n" +
                "AI\r\n" +
                "  Ctrl+F        Comprehensive Feedback\r\n\r\n" +
                "Panel Focus\r\n" +
                "  Ctrl+1        Chapter List (left panel)\r\n" +
                "  Ctrl+2        Writing Editor (centre panel)\r\n" +
                "  Ctrl+3        AI Assistant (right panel)\r\n\r\n" +
                "VOICE COMMANDS (Dragon / JSay / any STT)\r\n\r\n" +
                "  Panel navigation\r\n" +
                "    Panel 1  /  Go to panel 1  /  Panel one\r\n" +
                "    Panel 2  /  Go to panel 2  /  Panel two\r\n" +
                "    Panel 3  /  Go to panel 3  /  Panel three\r\n" +
                "    Go to chat  /  Chat tab  /  Chat\r\n" +
                "    Open prompts  /  Show prompts\r\n" +
                "    Open response cards  /  Cards\r\n\r\n" +
                "  Project\r\n" +
                "    Save  /  Save project  /  Save now\r\n" +
                "    New project\r\n\r\n" +
                "  Chapters\r\n" +
                "    Add chapter  /  New chapter  /  New section\r\n" +
                "    Delete chapter  /  Remove chapter\r\n" +
                "    Rename chapter  /  Rename\r\n" +
                "    Move up  /  Move chapter up\r\n" +
                "    Move down  /  Move chapter down\r\n" +
                "    Change type  /  Change section type\r\n\r\n" +
                "  AI Feedback\r\n" +
                "    Feedback  /  Run feedback  /  Comprehensive\r\n" +
                "    Pacing  /  Check pacing\r\n" +
                "    Dialogue  /  Check dialogue\r\n" +
                "    Style  /  Check style\r\n" +
                "    Structure  /  Check structure\r\n" +
                "    Send  /  Send message  /  Ask Claude\r\n\r\n" +
                "  Insert AI response\r\n" +
                "    Insert at cursor  /  Insert here\r\n" +
                "    Insert at start  /  Insert at beginning\r\n" +
                "    Insert at end  /  Append response\r\n\r\n" +
                "  Prompts\r\n" +
                "    Open prompt library  /  Show prompts\r\n" +
                "    Use prompt [ID]  — e.g. 'Use prompt F3'\r\n\r\n" +
                "  Response Cards\r\n" +
                "    Save card  /  Save response card\r\n" +
                "    Insert card [N]  — e.g. 'Insert card 2'\r\n" +
                "    Delete card [N]  — e.g. 'Delete card 1'\r\n" +
                "    Show [category] cards — e.g. 'Show Fiction cards'\r\n\r\n" +
                "  Export\r\n" +
                "    Export Word  /  Export manuscript  /  Export docx\r\n" +
                "    Export PDF  /  Export as PDF\r\n\r\n" +
                "  Settings\r\n" +
                "    Toggle voice  /  Voice on  /  Voice off\r\n" +
                "    Set API key  /  API key\r\n\r\n" +
                "JAWS / Screen Reader Tips\r\n" +
                "  F1            Read Help Text for the focused control\r\n" +
                "  Insert+F7     Links list (all buttons and controls)\r\n" +
                "  Status bar    Live region — JAWS reads all status updates\r\n" +
                "  JAWS voice    Runs independently. Turn off App Voice in Settings\r\n" +
                "                to let JAWS handle all audio.",
                "Keyboard Shortcuts & Voice Commands",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "VoiceBook Studio\r\n\r\n" +
                "Accessibility-first book writing application.\r\n\r\n" +
                "Designed for:\r\n" +
                "  JAWS screen reader\r\n" +
                "  Dragon NaturallySpeaking\r\n" +
                "  JSay voice control\r\n\r\n" +
                "AI powered by Anthropic Claude.",
                "About VoiceBook Studio",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
