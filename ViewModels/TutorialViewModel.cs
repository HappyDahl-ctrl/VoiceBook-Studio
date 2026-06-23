using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VoiceBookStudio.Services;

namespace VoiceBookStudio.ViewModels
{
    // ────────────────────────────────────────────────────────────────────
    // TutorialStep
    // ────────────────────────────────────────────────────────────────────

    /// <summary>Defines one step in the interactive tutorial.</summary>
    public class TutorialStep
    {
        public string  Title          { get; init; } = string.Empty;
        public string  Content        { get; init; } = string.Empty;

        /// <summary>
        /// Null  = passive narration step; user presses Next or says "Next" to advance.
        /// Set   = tutorial blocks here until this action code is received from the app.
        /// </summary>
        public string? RequiredAction { get; init; }

        /// <summary>Short prompt shown while waiting, e.g. "Say Panel 2 or press Ctrl+2".</summary>
        public string? ActionPrompt   { get; init; }

        /// <summary>Spoken when the action is matched. Falls back to a generic confirmation.</summary>
        public string? SuccessMessage { get; init; }

        /// <summary>Whether the user can say "Skip step" if they cannot complete the action.</summary>
        public bool    IsSkippable    { get; init; }
    }

    // ────────────────────────────────────────────────────────────────────
    // TutorialViewModel
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ViewModel for the non-modal tutorial window.
    ///
    /// Interactive steps block the Next button until the main app fires the
    /// expected action via <see cref="HandleAction"/>.  The tutorial then
    /// auto-advances, playing a confirmation sound before moving on.
    ///
    /// Passive steps (no RequiredAction) let the user go at their own pace.
    /// </summary>
    public partial class TutorialViewModel : ObservableObject
    {
        private readonly SystemAnnouncementService _announcer;
        private readonly AudioFeedbackService      _audio;
        private readonly AppSoundService?          _sounds;
        private readonly bool                      _jawsDetected;
        private readonly bool                      _dragonDetected;

        private int                      _currentIndex = 0;
        private CancellationTokenSource? _timeoutCts;

        private readonly RelayCommand _nextCommand;
        private readonly RelayCommand _previousCommand;
        private readonly RelayCommand _skipStepCommand;

        private readonly TutorialStep[] _steps;

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        public TutorialViewModel(SystemAnnouncementService announcer,
                                 AudioFeedbackService      audio,
                                 AppSoundService?          sounds         = null,
                                 bool                      jawsDetected   = false,
                                 bool                      dragonDetected = false)
        {
            _announcer      = announcer;
            _audio          = audio;
            _sounds         = sounds;
            _jawsDetected   = jawsDetected;
            _dragonDetected = dragonDetected;

            _nextCommand     = new RelayCommand(Next,     CanNext);
            _previousCommand = new RelayCommand(Previous, CanPrevious);
            _skipStepCommand = new RelayCommand(SkipStep, CanSkip);

            NextCommand     = _nextCommand;
            PreviousCommand = _previousCommand;
            RepeatCommand   = new RelayCommand(Repeat);
            ExitCommand     = new RelayCommand(Exit);
            SkipStepCommand = _skipStepCommand;

            _steps = BuildSteps(jawsDetected, dragonDetected);
        }

        // ----------------------------------------------------------------
        // Observable properties
        // ----------------------------------------------------------------

        [ObservableProperty]
        private bool _isWaitingForAction;

        partial void OnIsWaitingForActionChanged(bool value)
        {
            OnPropertyChanged(nameof(ActionStatusText));
            OnPropertyChanged(nameof(ShowSkipButton));
            _nextCommand.NotifyCanExecuteChanged();
            _skipStepCommand.NotifyCanExecuteChanged();
        }

        // ----------------------------------------------------------------
        // Derived properties
        // ----------------------------------------------------------------

        public int    TotalSteps         => _steps.Length;
        public string StepCounterDisplay => $"Step {_currentIndex + 1} of {TotalSteps}";
        public string CurrentTitle       => _steps[_currentIndex].Title;
        public string CurrentContent     => _steps[_currentIndex].Content;

        public string ActionStatusText =>
            IsWaitingForAction
                ? $"Waiting: {_steps[_currentIndex].ActionPrompt ?? "complete the action above to continue"}"
                : string.Empty;

        public bool ShowSkipButton =>
            IsWaitingForAction && _steps[_currentIndex].IsSkippable;

        // ----------------------------------------------------------------
        // Commands
        // ----------------------------------------------------------------

        public ICommand NextCommand     { get; }
        public ICommand PreviousCommand { get; }
        public ICommand RepeatCommand   { get; }
        public ICommand ExitCommand     { get; }
        public ICommand SkipStepCommand { get; }

        // ----------------------------------------------------------------
        // Events
        // ----------------------------------------------------------------

        /// <summary>Fired when the tutorial session ends.</summary>
        public event Action? TutorialCompleted;

        /// <summary>
        /// Fired every time the tutorial advances to a new step (including auto-advance
        /// from HandleAction). TutorialDialog uses this to restore itself after hiding
        /// for the import-document flow.
        /// </summary>
        public event Action? StepAdvanced;

        // ----------------------------------------------------------------
        // Public navigation methods
        // ----------------------------------------------------------------

        public void Start()
        {
            _currentIndex = 0;
            OnIndexChanged();
            IsWaitingForAction = false;

            // Delay so JAWS finishes announcing the new window before the app speaks.
            _ = Task.Delay(900).ContinueWith(
                _ => System.Windows.Application.Current?.Dispatcher.InvokeAsync(EnterCurrentStep));
        }

        public void Next()
        {
            _announcer.StopSpeaking();
            if (!CanNext()) return;

            CancelTimeout();
            IsWaitingForAction = false;

            if (_currentIndex < _steps.Length - 1)
            {
                _currentIndex++;
                OnIndexChanged();
                EnterCurrentStep();
            }
            else
            {
                TutorialCompleted?.Invoke();
            }
        }

        public void Previous()
        {
            _announcer.StopSpeaking();
            if (!CanPrevious()) return;

            CancelTimeout();
            IsWaitingForAction = false;
            _currentIndex--;
            OnIndexChanged();
            EnterCurrentStep();
        }

        public void Repeat()
        {
            _announcer.StopSpeaking();
            AnnounceCurrentStep();
        }

        public void Exit()
        {
            _announcer.StopSpeaking();
            TutorialCompleted?.Invoke();
        }

        public void SkipStep()
        {
            _announcer.StopSpeaking();
            if (!CanSkip()) return;
            _announcer.Speak("Step skipped.");
            CancelTimeout();
            IsWaitingForAction = false;
            if (_currentIndex < _steps.Length - 1)
            {
                _currentIndex++;
                OnIndexChanged();
                EnterCurrentStep();
            }
            else
            {
                TutorialCompleted?.Invoke();
            }
        }

        // ----------------------------------------------------------------
        // Action notification (called by MainViewModel when user acts)
        // ----------------------------------------------------------------

        /// <summary>
        /// Called by MainViewModel whenever the user performs an app action.
        /// If the current step is waiting for this action code, auto-advances.
        /// </summary>
        public void HandleAction(string actionCode)
        {
            if (!IsWaitingForAction) return;

            var step = _steps[_currentIndex];
            if (step.RequiredAction == null) return;
            if (!string.Equals(step.RequiredAction, actionCode,
                    StringComparison.OrdinalIgnoreCase)) return;

            IsWaitingForAction = false;
            CancelTimeout();

            _sounds?.Play(AppSound.TutorialStep);
            string confirmation = step.SuccessMessage ?? "Got it. Moving to the next step.";

            // Wait for any in-flight AudioFeedbackService announcement (e.g. "Editor panel.")
            // to finish before speaking the tutorial confirmation, then advance immediately
            // after the confirmation completes rather than using a fixed timer.
            _ = HandleActionAsync(confirmation);
        }

        private async Task HandleActionAsync(string confirmation)
        {
            try
            {
                // Let the app's own focus-change announcement finish before we speak over it.
                await _audio.WaitForCurrentSpeechAsync().ConfigureAwait(false);
                // Speak the success message and wait for it to complete before advancing.
                await _announcer.SpeakAndWaitAsync(confirmation).ConfigureAwait(false);
            }
            catch
            {
                // TTS failure — fall through and still advance the tutorial.
            }
            // Marshal Next() to the UI thread regardless of TTS success.
            var app = System.Windows.Application.Current;
            if (app != null)
                await app.Dispatcher.InvokeAsync(Next);
        }

        // ----------------------------------------------------------------
        // Private helpers
        // ----------------------------------------------------------------

        private bool CanNext()     => !IsWaitingForAction && _currentIndex < _steps.Length - 1;
        private bool CanPrevious() => _currentIndex > 0;
        private bool CanSkip()     => IsWaitingForAction && _steps[_currentIndex].IsSkippable;

        private void OnIndexChanged()
        {
            OnPropertyChanged(nameof(StepCounterDisplay));
            OnPropertyChanged(nameof(CurrentTitle));
            OnPropertyChanged(nameof(CurrentContent));
            OnPropertyChanged(nameof(ActionStatusText));
            OnPropertyChanged(nameof(ShowSkipButton));
            _nextCommand.NotifyCanExecuteChanged();
            _previousCommand.NotifyCanExecuteChanged();
            _skipStepCommand.NotifyCanExecuteChanged();
            StepAdvanced?.Invoke();
        }

        private void EnterCurrentStep()
        {
            var step = _steps[_currentIndex];

            if (step.RequiredAction != null)
            {
                IsWaitingForAction = true;
                StartTimeout(step);
            }
            else
            {
                IsWaitingForAction = false;
            }

            AnnounceCurrentStep();
        }

        private void AnnounceCurrentStep()
        {
            _sounds?.Play(AppSound.TutorialStep);

            // When JAWS is running, live regions on StepHeader and ActionStatusText
            // (both Assertive) already announce content immediately on change.
            // Speaking via SAPI at the same time causes double-reading.
            // Skip SAPI for JAWS users — live regions handle it.
            if (_jawsDetected) return;

            var step = _steps[_currentIndex];

            if (IsWaitingForAction)
            {
                // For interactive steps, speak only the title and the short action prompt.
                // step.Content is already displayed on screen and contains the same instruction —
                // reading the full content aloud then repeating ActionPrompt caused confusing duplication.
                // The command instruction must be the last thing spoken so Dragon picks it up cleanly.
                string prompt = step.ActionPrompt ?? "Complete the action to continue.";
                _announcer.Speak(
                    $"Step {_currentIndex + 1} of {_steps.Length}. {step.Title}. " +
                    $"{prompt}.");
            }
            else
            {
                bool isLast = _currentIndex == _steps.Length - 1;
                string closing = isLast
                    ? "Say Exit tutorial to close this window."
                    : "Say Next to continue.";
                _announcer.Speak(
                    $"Step {_currentIndex + 1} of {_steps.Length}. " +
                    $"{step.Title}. {step.Content} " +
                    $"{closing}");
            }
        }

        private void StartTimeout(TutorialStep step)
        {
            CancelTimeout();
            _timeoutCts = new CancellationTokenSource();
            var token   = _timeoutCts.Token;
            int stepIdx = _currentIndex;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(60_000, token);
                    if (token.IsCancellationRequested || _currentIndex != stepIdx) return;

                    string reminder = step.IsSkippable
                        ? $"Still waiting. {step.ActionPrompt}. Or say Skip step to move on."
                        : $"Still waiting. {step.ActionPrompt}.";

                    System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
                        _announcer.Speak(reminder));
                }
                catch (OperationCanceledException) { }
            }, token);
        }

        private void CancelTimeout()
        {
            _timeoutCts?.Cancel();
            _timeoutCts?.Dispose();
            _timeoutCts = null;
        }

        // ────────────────────────────────────────────────────────────────
        // Step definitions
        //
        // STRUCTURE
        //   Section 1 — Audio and microphone test        (steps  1–2,  interactive)
        //   Section 2 — Welcome and orientation          (steps  3–5,  passive)
        //   Section 3 — Understanding the three panels   (steps  6–9,  mixed)
        //   Section 4 — Voice command practice           (steps 10–11, passive)
        //   Section 5 — Your first chapter               (steps 12–15, interactive)
        //   Section 6 — Practice save                    (step  16,    interactive)
        //   Section 7 — Tutorial complete                (step  17,    passive)
        // ────────────────────────────────────────────────────────────────

        private static TutorialStep[] BuildSteps(bool jawsDetected, bool dragonDetected)
        {
            // Dynamic sentences inserted into steps where AT state matters
            string micInfo = dragonDetected
                ? "Dragon NaturallySpeaking is running. Dragon handles all your voice input. " +
                  "The built-in VoiceBook microphone is disabled so they do not conflict."
                : "The built-in VoiceBook microphone is on and listening for spoken commands.";

            string jawsInfo = jawsDetected
                ? "JAWS is running. JAWS will read all controls, live regions, and status bar " +
                  "updates automatically. VoiceBook still speaks critical tutorial messages."
                : "JAWS is not running. VoiceBook's built-in voice will read all instructions " +
                  "and confirmations throughout your session.";

            string voiceCommandRoute = dragonDetected
                ? "When Dragon is running, give VoiceBook commands by typing or dictating into " +
                  "the Command box at the bottom of this tutorial window, then press Enter."
                : "You can give VoiceBook commands by speaking them. The built-in microphone is " +
                  "listening. You can also type commands into the Command box below and press Enter.";

            return new[]
            {
                // ════════════════════════════════════════════════════════
                // SECTION 1 — Audio and microphone test  (steps 1–2)
                // These come first so audio/mic issues are caught before anything else.
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title   = "Step 1 — Audio Check",
                    Content =
                        "Welcome to VoiceBook Studio. Before anything else, let us confirm " +
                        "your audio is working.\n\n" +
                        "You should be hearing these words spoken aloud right now.\n\n" +
                        "If you cannot hear anything:\n" +
                        "  - Check that your speakers or headphones are connected and turned up\n" +
                        "  - Check the Windows volume mixer is not muted for VoiceBook Studio\n" +
                        "  - Press R to repeat this step's announcement"
                },

                new TutorialStep
                {
                    Title          = "Step 2 — Microphone Check",
                    Content =
                        "Now let us confirm the app can hear you.\n\n" +
                        (dragonDetected
                            ? "Dragon NaturallySpeaking is running. Type the word Hello into the " +
                              "Command box below and press Enter. This confirms Dragon is connected " +
                              "and command routing is working."
                            : "If the microphone hears you, the step will pass automatically.\n\n" +
                              "You can type Hello into the Command box below and press Enter, " +
                              "or say hello out loud right now."),
                    RequiredAction = "continue",
                    ActionPrompt   = dragonDetected
                        ? "Type Hello in the Command box and press Enter"
                        : "If the microphone hears you, the step will pass automatically. " +
                          "You can type Hello into the Command box below and press Enter, " +
                          "or say hello out loud right now",
                    SuccessMessage = "Audio and microphone confirmed. Everything is working.",
                    IsSkippable    = true
                },

                // ════════════════════════════════════════════════════════
                // SECTION 2 — Welcome and orientation  (passive, steps 3–5)
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title   = "Welcome to VoiceBook Studio",
                    Content =
                        "Great — audio is confirmed. Here is how the rest of the tutorial works.\n\n" +
                        "HOW THE TUTORIAL WORKS\n" +
                        "Most steps ask you to listen, then press Next to continue.\n" +
                        "Some steps ask you to perform an action — the tutorial waits and " +
                        "detects when you have done it before moving on.\n\n" +
                        "KEYBOARD SHORTCUTS IN THIS WINDOW\n" +
                        "  N         — Next step\n" +
                        "  P         — Previous step\n" +
                        "  R         — Repeat current step\n" +
                        "  S         — Skip an action step\n" +
                        "  Escape    — Exit tutorial"
                },

                new TutorialStep
                {
                    Title   = "Your Setup",
                    Content =
                        jawsInfo + "\n\n" + micInfo + "\n\n" +
                        voiceCommandRoute
                },

                new TutorialStep
                {
                    Title   = "About VoiceBook Studio",
                    Content =
                        "VoiceBook Studio has three panels.\n\n" +
                        "PANEL 1 — Chapter Manager (left side)\n" +
                        "Lists all your book sections in order: front matter, body chapters, " +
                        "back matter. Navigate with the Up and Down arrow keys.\n\n" +
                        "PANEL 2 — Writing Editor (centre)\n" +
                        "Where you write. Dragon NaturallySpeaking dictation works here the " +
                        "same as in Microsoft Word — all correction and navigation commands work.\n\n" +
                        "PANEL 3 — AI Assistant (right side)\n" +
                        "Chat with Claude for feedback, browse 75 writing prompts, and save " +
                        "useful responses as cards.\n\n" +
                        "Switch panels by pressing F1, F2, or F3. Or say Panel 1, Panel 2, or Panel 3."
                },

                // ════════════════════════════════════════════════════════
                // SECTION 3 — Panel navigation practice  (steps 6–9)
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title   = "Section 3 — Navigating Between Panels",
                    Content =
                        "You can switch panels in two ways.\n\n" +
                        "KEYBOARD\n" +
                        "  F1 or Ctrl+1  — Chapter Manager\n" +
                        "  F2 or Ctrl+2  — Writing Editor\n" +
                        "  F3 or Ctrl+3  — AI Assistant\n\n" +
                        "VOICE (say any of these)\n" +
                        "  Panel 1    /    Go to panel 1    /    Panel one\n" +
                        "  Panel 2    /    Go to panel 2    /    Panel two\n" +
                        "  Panel 3    /    Go to panel 3    /    Panel three\n\n" +
                        "The next three steps ask you to switch to each panel so you can " +
                        "confirm the commands work on your machine."
                },

                new TutorialStep
                {
                    Title          = "Switch to the Writing Editor — Panel 2",
                    Content        =
                        "Switch to the Writing Editor now.\n\n" +
                        "Say Panel Two — or press F2 or Ctrl+2.\n\n" +
                        "The tutorial will detect when you have done it and move on automatically.",
                    RequiredAction = "panel2",
                    ActionPrompt   = "Say Panel Two, or press F2 or Ctrl+2",
                    SuccessMessage = "Panel 2 focused. Navigation commands are working.",
                    IsSkippable    = true
                },

                new TutorialStep
                {
                    Title          = "Switch to the AI Assistant — Panel 3",
                    Content        =
                        "Now switch to the AI Assistant panel.\n\n" +
                        "Say Panel Three — or press F3 or Ctrl+3.",
                    RequiredAction = "panel3",
                    ActionPrompt   = "Say Panel Three, or press F3 or Ctrl+3",
                    SuccessMessage = "Panel 3 focused.",
                    IsSkippable    = true
                },

                new TutorialStep
                {
                    Title          = "Return to the Chapter Manager — Panel 1",
                    Content        =
                        "Now return to the Chapter Manager.\n\n" +
                        "Say Panel One — or press F1 or Ctrl+1.",
                    RequiredAction = "panel1",
                    ActionPrompt   = "Say Panel One, or press F1 or Ctrl+1",
                    SuccessMessage = "Panel 1 focused. Navigation practice complete.",
                    IsSkippable    = true
                },

                // ════════════════════════════════════════════════════════
                // SECTION 4 — Other voice commands overview  (steps 10–11, passive)
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title   = "Section 4 — Other Voice Commands",
                    Content =
                        "VoiceBook Studio understands many spoken commands beyond panel navigation.\n\n" +
                        "COMMON COMMANDS\n" +
                        "  Save             — save your project\n" +
                        "  New project      — start a new book\n" +
                        "  Add chapter      — add a new chapter or section\n" +
                        "  Rename chapter   — rename the selected chapter\n" +
                        "  Delete chapter   — delete the selected chapter\n" +
                        "  Move up / Move down — reorder chapters\n" +
                        "  Export Word      — export as a Word document\n" +
                        "  Export PDF       — export as a PDF\n" +
                        "  Comprehensive feedback — AI analysis of the current chapter\n" +
                        "  What can I say here — hear commands for the current panel\n\n" +
                        "In the Writing Editor, give commands by going to Panel 3 and " +
                        "typing or dictating in the Chat box, then pressing Enter."
                },

                new TutorialStep
                {
                    Title   = "Context-Sensitive Help",
                    Content =
                        "At any time while using VoiceBook Studio, you can say:\n\n" +
                        "  What can I say here\n\n" +
                        "The app will speak a list of the commands available in whichever " +
                        "panel you are currently using.\n\n" +
                        "This is the fastest way to discover commands without looking " +
                        "anything up in the manual."
                },

                // ════════════════════════════════════════════════════════
                // SECTION 5 — Your first chapter  (steps 12–15)
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title   = "Section 5 — Creating Your First Chapter",
                    Content =
                        "Now you will create a project and add your first chapter.\n\n" +
                        "VoiceBook Studio has two ways to start writing.\n\n" +
                        "OPTION A — START FRESH\n" +
                        "Create a new project, then add a blank chapter and start dictating.\n\n" +
                        "OPTION B — IMPORT AN EXISTING DOCUMENT\n" +
                        "If you already have writing in a Word document, import it. " +
                        "VoiceBook will detect your chapter breaks and bring your text in.\n\n" +
                        "The next step will ask which option you want."
                },

                new TutorialStep
                {
                    Title          = "New Project or Import Document?",
                    Content        =
                        "To create a new blank project:\n" +
                        "  Say New Project  —  or press Ctrl+N\n\n" +
                        "To import an existing Word document:\n" +
                        "  Say Import Document  —  or press Ctrl+I\n\n" +
                        "Choose whichever applies to you. " +
                        "The tutorial will detect your choice and guide you from there.\n\n" +
                        "You can also press Skip Step if you want to explore this on your own later.",
                    RequiredAction = "newproject_or_import",
                    ActionPrompt   = "Say New Project (Ctrl+N) or say Import Document (Ctrl+I)",
                    SuccessMessage = "Got it. Follow the dialog that just opened.",
                    IsSkippable    = true
                },

                new TutorialStep
                {
                    Title          = "Complete the Dialog",
                    Content        =
                        "A dialog has opened.\n\n" +
                        "If you are creating a new project: type your book title and press Enter.\n\n" +
                        "If you are importing a document: choose your Word file in the file browser, " +
                        "then follow the prompts to confirm chapter titles.\n\n" +
                        "Take your time. The tutorial will wait until your project is open.",
                    RequiredAction = "projectopened",
                    ActionPrompt   = "Complete the dialog — the tutorial will detect when done",
                    SuccessMessage = "Project opened. Well done.",
                    IsSkippable    = true
                },

                new TutorialStep
                {
                    Title          = "Add Your First Chapter",
                    Content        =
                        "Your project is open. Now add a chapter.\n\n" +
                        "Say Add Chapter  —  or press Ctrl+A.\n\n" +
                        "A dialog will ask you to choose a section type. " +
                        "Chapter is the default for body content. Press Enter to accept it, " +
                        "then type your chapter title and press Enter.\n\n" +
                        "If you imported a document that already contains chapters, " +
                        "you can skip this step.",
                    RequiredAction = "addchapter",
                    ActionPrompt   = "Say Add Chapter, or press Ctrl+A",
                    SuccessMessage = "Chapter added. Your book structure is ready.",
                    IsSkippable    = true
                },

                // ════════════════════════════════════════════════════════
                // SECTION 6 — Practice save  (step 16, interactive)
                // Save comes after the project is created so there is something to save.
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title          = "Practice: Save Your Project",
                    Content        =
                        "Now that your project is set up, let's save it.\n\n" +
                        "Say Save — or press Ctrl+S.\n\n" +
                        "Get into the habit of saving regularly. VoiceBook will confirm " +
                        "every save with a sound and a status message.",
                    RequiredAction = "save",
                    ActionPrompt   = "Say Save, or press Ctrl+S",
                    SuccessMessage = "Saved. Good habit.",
                    IsSkippable    = true
                },

                // ════════════════════════════════════════════════════════
                // SECTION 7 — Completion  (step 17, passive)
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title   = "Tutorial Complete — You Are Ready to Write",
                    Content =
                        "Congratulations. You have completed the VoiceBook Studio tutorial.\n\n" +
                        "QUICK REFERENCE\n" +
                        "  F1 / Ctrl+1      — Chapter Manager\n" +
                        "  F2 / Ctrl+2      — Writing Editor\n" +
                        "  F3 / Ctrl+3      — AI Assistant\n" +
                        "  Ctrl+S           — Save\n" +
                        "  Ctrl+A           — Add chapter\n" +
                        "  Ctrl+N           — New project\n" +
                        "  Ctrl+I           — Import Word document\n" +
                        "  Ctrl+F           — Run comprehensive AI feedback\n\n" +
                        "Say What can I say here at any time to hear context-sensitive commands.\n\n" +
                        "Say Start tutorial at any time to run this tutorial again."
                }
            };
        }
    }
}
