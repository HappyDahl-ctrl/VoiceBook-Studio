using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VoiceBookStudio.Services;
using VoiceBookStudio.ViewModels;
using WinForms = System.Windows.Forms;

namespace VoiceBookStudio.Views
{
    public partial class MainWindow : Window, Services.ITutorialPresenter
    {
        // ----------------------------------------------------------------
        // Dragon-compatible editor (WinForms RichTextBox via WindowsFormsHost)
        // WinForms RichTextBox implements Win32 Text Services Framework (TSF),
        // giving Dragon NaturallySpeaking full dictation, correction, navigation,
        // and "Correct that / Scratch that / Select that" — the same layer Word uses.
        // ----------------------------------------------------------------

        private WinForms.RichTextBox _editorRtb = null!;

        // Last caret position before user switched away from the editor.
        // Used by the "Insert at Cursor" AI command.
        private int  _lastEditorCaretIndex;
        private bool _suppressEditorSync;

        private MainViewModel          ViewModel    => (MainViewModel)DataContext;
        private VoiceCommandRouter?    _voiceRouter;
        private SpeechListenerService? _speechListener;
        private readonly AppSoundService _sounds;

        public MainWindow(AppSoundService sounds)
        {
            _sounds = sounds;
            InitializeComponent();

            Loaded += Window_Loaded;
            Closing += MainWindow_Closing;
            Closed  += (_, _) => _speechListener?.Dispose();

            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        // ----------------------------------------------------------------
        // Startup wiring
        // ----------------------------------------------------------------

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ── Build the Dragon-compatible editor ──────────────────────────
            _editorRtb = new WinForms.RichTextBox
            {
                Font            = new System.Drawing.Font("Segoe UI", 10.5f),
                Multiline       = true,
                WordWrap        = true,
                ScrollBars      = WinForms.RichTextBoxScrollBars.Vertical,
                DetectUrls      = false,
                ShortcutsEnabled = true,  // lets Dragon use standard edit shortcuts
                AcceptsTab      = true,
                BorderStyle     = WinForms.BorderStyle.None,
                ReadOnly        = true,   // read-only until a chapter is selected
                BackColor       = System.Drawing.Color.White,
                ForeColor       = System.Drawing.Color.Black,
            };

            _editorRtb.TextChanged += EditorRtb_TextChanged;
            _editorRtb.Leave       += EditorRtb_Leave;

            // Forward app-level keyboard shortcuts from the WinForms message loop
            // to WPF commands. WPF PreviewKeyDown does not fire when WinForms has focus.
            _editorRtb.KeyDown += EditorRtb_KeyDown;

            EditorHost.Child = _editorRtb;

            // ── Wire ViewModel events ───────────────────────────────────────
            ViewModel.InsertTextRequested     += OnInsertTextRequested;
            ViewModel.FocusPanelRequested     += OnFocusPanelRequested;
            ViewModel.SwitchAiTabRequested    += OnSwitchAiTabRequested;
            ViewModel.FocusChatInputRequested        += (_, _) => ChatInputBox.Focus();
            ViewModel.FocusAndClearChatInputRequested += (_, _) =>
            {
                // Switch to the Chat tab so the input box is visible, clear any
                // previous text, then focus — ready for Dragon to type a command.
                AiTabControl.SelectedIndex = 0;
                ChatInputBox.Clear();
                ChatInputBox.Focus();
            };

            // Register / deregister TutorialService with the voice router when
            // the guided tour starts and ends.
            ViewModel.TourStarted += (_, svc) => _voiceRouter?.SetTourService(svc);
            ViewModel.TourEnded   += (_, _)   => _voiceRouter?.SetTourService(null);

            // "Read paragraph" — extract the paragraph under the WinForms caret and speak it.
            ViewModel.ReadParagraphAtCursorRequested += (_, _) =>
            {
                string para = ExtractParagraphAtCursor();
                ViewModel.SpeakViaAnnouncer(
                    string.IsNullOrWhiteSpace(para) ? "No text at cursor position." : para);
            };

            // "Close application" voice command — handled by closing the window.
            ViewModel.CloseApplicationRequested += (_, _) => Close();

            // ── Voice recognition ────────────────────────────────────────────
            _voiceRouter    = new VoiceCommandRouter(ViewModel);
            _speechListener = new SpeechListenerService();

            _speechListener.CommandRecognized += (_, cmd) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_voiceRouter.TryRoute(cmd))
                        _sounds.Play(AppSound.CommandRecognized);
                    else
                    {
                        _sounds.Play(AppSound.CommandNotRecognized);
                        ViewModel.OnCommandNotRecognized(cmd);
                    }
                });
            };

            ViewModel.MicToggleRequested += (_, desired) =>
            {
                bool actual = desired && _speechListener.StartListening();
                if (!desired) { _speechListener.StopListening(); actual = false; }

                string? error = (desired && !actual)
                    ? "Could not start microphone. Check that a microphone is connected and Windows Speech Recognition is set up."
                    : null;
                ViewModel.SetMicListening(actual, error);
            };
        }

        // ----------------------------------------------------------------
        // WinForms editor keyboard shortcuts
        // WPF PreviewKeyDown does not fire when a WinForms control owns the
        // Win32 focus. Handle all app shortcuts here.
        // ----------------------------------------------------------------

        private void EditorRtb_KeyDown(object? sender, WinForms.KeyEventArgs e)
        {
            // Any key press in the editor also stops active library reading.
            if (ViewModel.IsReadingActive)
                ViewModel.StopLibraryReading();

            bool ctrl  = e.Control;
            bool shift = e.Shift;
            bool alt   = e.Alt;
            bool none  = !ctrl && !shift && !alt;

            // Panel navigation (same keys as the WPF PreviewKeyDown handler)
            // NOTE: F1 is deliberately NOT captured here — JAWS uses F1 for contextual help.
            // Use Ctrl+1 / Ctrl+2 / Ctrl+3 for panel focus (matches user manual section 14).
            if (none)
            {
                switch (e.KeyCode)
                {
                    case WinForms.Keys.F2:
                        ViewModel.FocusPanel2();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.F3:
                        ViewModel.FocusPanel3();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.F4:
                        ViewModel.TryReadParagraph();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.F5:
                        ViewModel.TryAnnounceChapterTitle();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.F6:
                        ViewModel.TrySelectNextChapter();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.F7:
                        ViewModel.TrySelectPreviousChapter();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.F8:
                        ViewModel.TryReadChapter();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.F9:
                        ViewModel.TryAnnounceApplicationStatus();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.Escape:
                        ViewModel.FocusPanel1();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                }
            }

            // App shortcuts — forward to ViewModel commands
            if (ctrl && !shift && !alt)
            {
                switch (e.KeyCode)
                {
                    case WinForms.Keys.F4:
                        ViewModel.TryStopReading();
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.S:
                        // Signal tutorial action before the CanExecute guard (step 1.4 detection).
                        ViewModel.TrySignalTutorialAction("save");
                        if (ViewModel.SaveProjectCommand.CanExecute(null))
                            ViewModel.SaveProjectCommand.Execute(null);
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.F:
                        if (ViewModel.RunAiFeedbackCommand.CanExecute("comprehensive"))
                            ViewModel.RunAiFeedbackCommand.Execute("comprehensive");
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                }
            }

            if (ctrl && shift && !alt)
            {
                if (e.KeyCode == WinForms.Keys.S)
                {
                    if (ViewModel.SaveProjectAsCommand.CanExecute(null))
                        ViewModel.SaveProjectAsCommand.Execute(null);
                    e.Handled = e.SuppressKeyPress = true;
                    return;
                }
            }

            if (alt && !ctrl && !shift)
            {
                switch (e.KeyCode)
                {
                    case WinForms.Keys.Up:
                        if (ViewModel.MoveChapterUpCommand.CanExecute(null))
                            ViewModel.MoveChapterUpCommand.Execute(null);
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                    case WinForms.Keys.Down:
                        if (ViewModel.MoveChapterDownCommand.CanExecute(null))
                            ViewModel.MoveChapterDownCommand.Execute(null);
                        e.Handled = e.SuppressKeyPress = true;
                        return;
                }
            }
        }

        // ----------------------------------------------------------------
        // Chapter list
        // ----------------------------------------------------------------

        private void ChapterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            _suppressEditorSync = true;

            if (e.AddedItems[0] is WholeBookViewModel)
            {
                ViewModel.SelectWholeBook();
                _editorRtb.ReadOnly = true;
                _editorRtb.Text     = ViewModel.WholeBook.Content;
            }
            else
            {
                var selected = e.AddedItems[0] as ChapterViewModel;
                ViewModel.OnChapterSelected(selected);
                _editorRtb.ReadOnly = selected == null;
                _editorRtb.Text     = selected?.Content ?? string.Empty;
            }

            _suppressEditorSync   = false;
            _lastEditorCaretIndex = 0;
        }

        // ----------------------------------------------------------------
        // Editor events (WinForms)
        // ----------------------------------------------------------------

        private void EditorRtb_TextChanged(object? sender, EventArgs e)
        {
            if (_suppressEditorSync) return;
            ViewModel.OnEditorTextChanged(_editorRtb.Text);
        }

        // Capture caret before user leaves the editor so "Insert at Cursor" works.
        private void EditorRtb_Leave(object? sender, EventArgs e)
        {
            _lastEditorCaretIndex = _editorRtb.SelectionStart;
        }

        // ----------------------------------------------------------------
        // Insert AI response into the editor
        // ----------------------------------------------------------------

        private void OnInsertTextRequested(object? sender, InsertTextArgs e)
        {
            string block = Environment.NewLine + Environment.NewLine +
                           e.Text.Trim()       +
                           Environment.NewLine + Environment.NewLine;

            string current = _editorRtb.Text;
            int pos = e.Position switch
            {
                InsertPosition.AtCursor    => Math.Clamp(_lastEditorCaretIndex, 0, current.Length),
                InsertPosition.AtBeginning => 0,
                InsertPosition.AtEnd       => current.Length,
                _                          => current.Length
            };

            _suppressEditorSync        = true;
            _editorRtb.Text            = current.Insert(pos, block);
            _editorRtb.SelectionStart  = pos + block.Length;
            _editorRtb.SelectionLength = 0;
            _suppressEditorSync        = false;

            ViewModel.OnEditorTextChanged(_editorRtb.Text);
            _editorRtb.Focus();
        }

        // ----------------------------------------------------------------
        // WPF-level panel shortcuts
        // These handle the WPF controls (chat, chapter list).
        // Editor shortcuts are handled in EditorRtb_KeyDown above.
        // ----------------------------------------------------------------

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Any key press stops active library reading (spec: stop on any key).
            if (ViewModel.IsReadingActive)
                ViewModel.StopLibraryReading();

            // Skip if WinForms editor has Win32 focus — EditorRtb_KeyDown handles it.
            if (_editorRtb?.Focused == true) return;

            bool ctrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool none = Keyboard.Modifiers == ModifierKeys.None;

            if (none)
            {
                // NOTE: F1 intentionally NOT captured — JAWS uses F1 for contextual help on the focused control.
                // Use Ctrl+1 / Ctrl+2 / Ctrl+3 for panel focus (user manual section 14).
                if (e.Key == Key.F2) { ViewModel.FocusPanel2();                  e.Handled = true; return; }
                if (e.Key == Key.F3) { ViewModel.FocusPanel3();                  e.Handled = true; return; }
                if (e.Key == Key.F4) { ViewModel.TryReadParagraph();             e.Handled = true; return; }
                if (e.Key == Key.F5) { ViewModel.TryAnnounceChapterTitle();      e.Handled = true; return; }
                if (e.Key == Key.F6) { ViewModel.TrySelectNextChapter();         e.Handled = true; return; }
                if (e.Key == Key.F7) { ViewModel.TrySelectPreviousChapter();     e.Handled = true; return; }
                if (e.Key == Key.F8) { ViewModel.TryReadChapter();               e.Handled = true; return; }
                if (e.Key == Key.F9) { ViewModel.TryAnnounceApplicationStatus(); e.Handled = true; return; }
            }

            if (ctrl)
            {
                if (e.Key == Key.D1 || e.Key == Key.NumPad1) { ViewModel.FocusPanel1();     e.Handled = true; return; }
                if (e.Key == Key.D2 || e.Key == Key.NumPad2) { ViewModel.FocusPanel2();     e.Handled = true; return; }
                if (e.Key == Key.D3 || e.Key == Key.NumPad3) { ViewModel.FocusPanel3();     e.Handled = true; return; }
                if (e.Key == Key.F4)                          { ViewModel.TryStopReading(); e.Handled = true; return; }

                // Signal tutorial "save" action on every Ctrl+S regardless of project state.
                // e.Handled stays false so the XAML KeyBinding also fires and saves if possible.
                if (e.Key == Key.S) { ViewModel.TrySignalTutorialAction("save"); return; }
            }
        }

        private void OnFocusPanelRequested(object? sender, int panelNumber)
        {
            switch (panelNumber)
            {
                case 1: ChapterListBox.Focus(); break;
                case 2: _editorRtb?.Focus();   break;
                case 3: ChatInputBox.Focus();   break;
            }
        }

        private void OnSwitchAiTabRequested(object? sender, string tabName)
        {
            AiTabControl.SelectedIndex = tabName switch
            {
                "Prompts"  => 1,
                "Cards"    => 2,
                "Feedback" => 3,
                _          => 0
            };
        }

        // ----------------------------------------------------------------
        // Chat input — command routing and Claude chat
        // ----------------------------------------------------------------

        private void ChatInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                string text = ChatInputBox.Text.Trim();
                if (string.IsNullOrEmpty(text)) { e.Handled = true; return; }

                if (_voiceRouter != null && _voiceRouter.TryRoute(text))
                {
                    _sounds.Play(AppSound.CommandRecognized);
                    ChatInputBox.Clear();
                    e.Handled = true;
                    return;
                }

                if (ViewModel.SendChatCommand.CanExecute(null))
                    ViewModel.SendChatCommand.Execute(null);

                e.Handled = true;
            }
        }

        // ----------------------------------------------------------------
        // Menu handlers
        // ----------------------------------------------------------------

        private void AddPromptButton_Click(object sender, RoutedEventArgs e) =>
            ViewModel.TryAddPrompt();

        /// <summary>
        /// Sets keyboard focus to the chapter list. Called by the startup sequence
        /// in App.xaml.cs after the window is shown so JAWS announces the first panel.
        /// Safe to call before Window_Loaded — ChapterListBox is created by InitializeComponent.
        /// </summary>
        public void FocusChapterList() => ChapterListBox?.Focus();

        /// <summary>
        /// Routes a voice command text string — used by TutorialDialog's command input
        /// so Dragon users can dictate commands directly into the tutorial.
        /// </summary>
        public bool TryRouteVoiceCommand(string text)
        {
            if (_voiceRouter == null || string.IsNullOrWhiteSpace(text)) return false;
            if (_voiceRouter.TryRoute(text))
            {
                _sounds.Play(AppSound.CommandRecognized);
                return true;
            }
            _sounds.Play(AppSound.CommandNotRecognized);
            return false;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e) => Close();

        // ----------------------------------------------------------------
        // ITutorialPresenter — implemented explicitly to keep the interface
        // contract off the public surface of MainWindow.
        // ----------------------------------------------------------------

        System.Windows.UIElement Services.ITutorialPresenter.ChapterListElement => ChaptersPanel;
        System.Windows.UIElement Services.ITutorialPresenter.EditorElement      => EditorPanel;
        System.Windows.UIElement Services.ITutorialPresenter.AssistantElement   => AiPanel;

        void Services.ITutorialPresenter.FocusChapterList() => ChapterListBox?.Focus();
        void Services.ITutorialPresenter.FocusEditor()      => _editorRtb?.Focus();
        void Services.ITutorialPresenter.FocusAssistant()   => ChatInputBox?.Focus();

        void Services.ITutorialPresenter.NotifyTourComplete()
        {
            // ViewModel clears the tour reference and fires TourEnded, which causes
            // Window_Loaded's TourEnded handler to deregister the tour from the router.
            ViewModel.OnTourComplete();
        }

        private void ShowShortcuts_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new HelpDialog(ViewModel.AudioService) { Owner = this };
            dlg.Show();
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

        // ----------------------------------------------------------------
        // Editor reading helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Returns the paragraph that contains the current caret position in
        /// the WinForms editor.  Paragraphs are blocks of text separated by
        /// one or more blank lines (consecutive newline pairs).
        /// Returns an empty string when the editor has no content.
        /// </summary>
        private string ExtractParagraphAtCursor()
        {
            string text   = _editorRtb.Text;
            int    cursor = _editorRtb.SelectionStart;

            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            cursor = Math.Clamp(cursor, 0, text.Length);

            // Walk backward from cursor to find the start of this paragraph
            // (defined as two consecutive newline characters).
            int start = cursor;
            while (start > 1)
            {
                if ((text[start - 1] == '\n' || text[start - 1] == '\r') &&
                    (text[start - 2] == '\n' || text[start - 2] == '\r'))
                    break;
                start--;
            }

            // Walk forward from cursor to find the end of this paragraph.
            int end = cursor;
            while (end < text.Length - 1)
            {
                if ((text[end] == '\n' || text[end] == '\r') &&
                    (text[end + 1] == '\n' || text[end + 1] == '\r'))
                    break;
                end++;
            }

            return text[start..end].Trim();
        }

        // ----------------------------------------------------------------
        // App close — play chime then speak a synchronous goodbye so the
        // message completes before the process exits. Synchronous SAPI Speak()
        // is used deliberately: async speech would be cut off on process exit.
        // ----------------------------------------------------------------

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _sounds.Play(AppSound.AppClosing);
            ViewModel.SpeakGoodbye();
        }
    }
}
