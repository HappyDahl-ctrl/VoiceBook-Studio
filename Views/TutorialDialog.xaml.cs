using System.Windows;
using System.Windows.Input;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Views
{
    public partial class TutorialDialog : Window
    {
        // Set to true when we hide the dialog so the import dialogs can take
        // focus without a WPF modal ownership conflict. Cleared when we re-show.
        private bool _hiddenForImport = false;

        public TutorialDialog()
        {
            InitializeComponent();
        }

        public TutorialViewModel ViewModel => (TutorialViewModel)DataContext;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Position in the bottom-right corner of the owner window so it
            // does not cover Panels 1 or 2 while the user practises commands.
            if (Owner != null)
            {
                Left = Owner.Left + Owner.Width  - Width  - 20;
                Top  = Owner.Top  + Owner.Height - Height - 60;
            }

            if (DataContext is TutorialViewModel vm)
            {
                vm.Start();

                // Re-show the tutorial dialog after the import flow completes.
                // The import path hides us to avoid a WPF modal conflict with the
                // file picker and chapter-confirmation dialogs; StepAdvanced fires
                // once projectopened is detected and HandleAction auto-advances.
                vm.StepAdvanced += () =>
                {
                    if (!_hiddenForImport) return;
                    _hiddenForImport = false;
                    Dispatcher.BeginInvoke(() =>
                    {
                        Show();
                        Activate();
                        StepContent.Focus();
                    });
                };
            }

            // Keep focus on the step content area — JAWS reads it via live region.
            StepContent.Focus();
        }

        // ----------------------------------------------------------------
        // Keyboard shortcuts
        //
        // No modifier:
        //   N = Next        P = Previous    R = Repeat
        //   S = Skip step   Escape = Exit   Enter = "continue" action
        //   F1/F2/F3 = focus main-window panel and fire panel action
        //
        // Ctrl modifier:
        //   S = fire "save" action (step 4.2 detection)
        //   N = fire New Project + "newproject_or_import" action
        //   A = forward Add Chapter to main window
        //   I = fire Import Document + "newproject_or_import" action
        //   1/2/3 = focus main-window panel + fire panel action
        // ----------------------------------------------------------------

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not TutorialViewModel vm) return;

            bool ctrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool none = Keyboard.Modifiers == ModifierKeys.None;

            if (ctrl)
            {
                switch (e.Key)
                {
                    case Key.S:
                        // Fire save — works regardless of whether a project is open.
                        vm.HandleAction("save");
                        ForwardToMain(mvm => mvm.TrySaveProject());
                        e.Handled = true;
                        return;

                    case Key.N:
                        vm.HandleAction("newproject_or_import");
                        ForwardToMain(mvm => mvm.TryCreateNewProject());
                        e.Handled = true;
                        return;

                    case Key.A:
                        ForwardToMain(mvm => mvm.TryAddChapter());
                        e.Handled = true;
                        return;

                    case Key.I:
                        vm.HandleAction("newproject_or_import");
                        // Hide this dialog before opening the file browser.
                        // The file picker and chapter-confirmation dialogs are modal and
                        // conflict with a non-modal WPF window sharing the same owner.
                        // We restore in the StepAdvanced handler once projectopened fires.
                        _hiddenForImport = true;
                        this.Hide();
                        ForwardToMain(mvm => mvm.TryImportDocument());
                        e.Handled = true;
                        return;

                    case Key.D1:
                    case Key.NumPad1:
                        FocusMainPanel(1);
                        e.Handled = true;
                        return;

                    case Key.D2:
                    case Key.NumPad2:
                        FocusMainPanel(2);
                        e.Handled = true;
                        return;

                    case Key.D3:
                    case Key.NumPad3:
                        FocusMainPanel(3);
                        e.Handled = true;
                        return;
                }
            }

            if (!none) return;

            switch (e.Key)
            {
                case Key.N:      vm.Next();                    e.Handled = true; break;
                case Key.P:      vm.Previous();                e.Handled = true; break;
                case Key.R:      vm.Repeat();                  e.Handled = true; break;
                case Key.S:      vm.SkipStep();                e.Handled = true; break;
                case Key.Escape: vm.Exit();                    e.Handled = true; break;
                case Key.Enter:  vm.HandleAction("continue");  e.Handled = true; break;

                case Key.F1: FocusMainPanel(1); e.Handled = true; break;
                case Key.F2: FocusMainPanel(2); e.Handled = true; break;
                case Key.F3: FocusMainPanel(3); e.Handled = true; break;
            }
        }

        /// <summary>
        /// Focuses the requested main-window panel and fires the matching tutorial
        /// action so interactive panel-navigation steps auto-advance.
        /// </summary>
        private void FocusMainPanel(int panel)
        {
            if (Owner is MainWindow main && main.DataContext is MainViewModel vm)
            {
                switch (panel)
                {
                    case 1: vm.FocusPanel1(); break;
                    case 2: vm.FocusPanel2(); break;
                    case 3: vm.FocusPanel3(); break;
                }
                main.Activate();
            }
        }

        private void ForwardToMain(System.Action<MainViewModel> action)
        {
            if (Owner is MainWindow main && main.DataContext is MainViewModel vm)
                action(vm);
        }

        // ----------------------------------------------------------------
        // Command input box — Dragon dictation / typed commands
        //
        // The user dictates or types a command phrase and presses Enter.
        // We attempt to route it through the main window's voice router.
        // Additionally we check for a few tutorial-specific phrases here
        // so the user does not have to know exact router command names.
        // ----------------------------------------------------------------

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;

            string raw = CommandInput.Text.Trim();
            if (string.IsNullOrEmpty(raw)) return;

            if (DataContext is not TutorialViewModel vm) return;

            string lower = raw.ToLowerInvariant();

            // Tutorial-level commands that the voice router does not know about
            bool handled = true;
            switch (lower)
            {
                case "next":
                case "next step":
                case "go":
                    vm.Next();
                    break;

                case "previous":
                case "back":
                case "previous step":
                    vm.Previous();
                    break;

                case "repeat":
                case "repeat step":
                case "say again":
                    vm.Repeat();
                    break;

                case "skip":
                case "skip step":
                    vm.SkipStep();
                    break;

                case "exit":
                case "exit tutorial":
                case "close tutorial":
                    vm.Exit();
                    break;

                // Mic / audio confirmation
                case "hello":
                case "continue":
                case "yes":
                case "ok":
                case "okay":
                case "confirm":
                case "audio ok":
                case "audio confirmed":
                    vm.HandleAction("continue");
                    break;

                // New project shortcut
                case "new project":
                case "create project":
                case "start new project":
                    vm.HandleAction("newproject_or_import");
                    ForwardToMain(mvm => mvm.TryCreateNewProject());
                    break;

                // Import shortcut — hide dialog before opening file browser to avoid
                // WPF modal ownership conflict with the file picker and confirmation dialogs.
                case "import":
                case "import document":
                case "import word":
                    vm.HandleAction("newproject_or_import");
                    _hiddenForImport = true;
                    this.Hide();
                    ForwardToMain(mvm => mvm.TryImportDocument());
                    break;

                default:
                    handled = false;
                    break;
            }

            if (handled)
            {
                CommandInput.Clear();
                return;
            }

            // Fall through to the main window's voice command router for all
            // other commands (panel navigation, save, add chapter, etc.).
            if (Owner is MainWindow main && main.TryRouteVoiceCommand(raw))
            {
                CommandInput.Clear();
            }
            // If the router did not recognise it, leave the text so the user
            // can see what was misunderstood and correct it.
        }
    }
}
