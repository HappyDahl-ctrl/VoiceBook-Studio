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

        /// <summary>
        /// Alternative to <see cref="RequiredAction"/> for a step that should advance on
        /// ANY one of several action codes (e.g. switching to any of a few panels).
        /// Leave both null for a passive step; set at most one of the two.
        /// </summary>
        public string[]? RequiredActionsAny { get; init; }

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

        /// <summary>
        /// Fired by Repeat() so the dialog can re-announce the current step to JAWS
        /// without a PropertyChanged event (the index does not change on Repeat).
        /// </summary>
        public event Action? RepeatRequested;

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
            RepeatRequested?.Invoke();
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
            AnnounceTutorialText("Step skipped.");
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
            bool matchesSingle = step.RequiredAction != null &&
                string.Equals(step.RequiredAction, actionCode, StringComparison.OrdinalIgnoreCase);
            bool matchesAny = step.RequiredActionsAny != null &&
                Array.Exists(step.RequiredActionsAny,
                    a => string.Equals(a, actionCode, StringComparison.OrdinalIgnoreCase));
            if (!matchesSingle && !matchesAny) return;

            IsWaitingForAction = false;
            CancelTimeout();

            // Cut off this step's own prompt narration immediately — it may still be
            // mid-utterance when the action fires (e.g. the user acts on a panel-switch
            // step before "...or press F2, F3, or F11" finishes), and left running it
            // plays concurrently with MainViewModel's LiveAnnounce for the action just taken.
            _announcer.StopSpeaking();

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
                await _announcer.SpeakOnDemandAsync(confirmation).ConfigureAwait(false);
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

        /// <summary>
        /// Routes tutorial narration through the correct speech channel.
        /// Speak() is a no-op when JAWS is detected, so JAWS users hear nothing.
        /// SpeakOnDemandAsync() bypasses that rule and is the correct channel
        /// for deliberate tutorial narration regardless of AT state — except when
        /// the user has explicitly muted app voice (IsMuted), which SpeakOnDemandAsync
        /// itself does not check since it also serves genuinely on-demand reads
        /// (AI response, chapter, paragraph) that should stay available regardless.
        /// Checking IsMuted here specifically is what makes "the user prefers JAWS
        /// only, they can disable app TTS in Settings" (see AnnounceCurrentStep)
        /// actually true.
        /// Fire-and-forget here matches the existing Speak() pattern; StopSpeaking()
        /// called from Next/Previous/Exit cancels any in-flight utterance correctly.
        /// </summary>
        private void AnnounceTutorialText(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || _announcer.IsMuted) return;
            if (_jawsDetected)
                _ = _announcer.SpeakOnDemandAsync(text);
            else
                _announcer.Speak(text);
        }

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

            if (step.RequiredAction != null || step.RequiredActionsAny != null)
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

            // Always speak tutorial steps via SystemAnnouncementService regardless of JAWS.
            // SystemAnnouncementService already adds a 500 ms pre-delay when JAWS is present
            // to avoid clashing with JAWS speech. UIA live regions on StepHeader / ActionStatusText
            // still fire in parallel; if the user prefers JAWS only, they can disable app TTS in Settings.
            var step = _steps[_currentIndex];

            if (IsWaitingForAction)
            {
                // For interactive steps, speak only the title and the short action prompt.
                // step.Content is already displayed on screen and contains the same instruction —
                // reading the full content aloud then repeating ActionPrompt caused confusing duplication.
                // The command instruction must be the last thing spoken so Dragon picks it up cleanly.
                string prompt = step.ActionPrompt ?? "Complete the action to continue.";
                AnnounceTutorialText(
                    $"Step {_currentIndex + 1} of {_steps.Length}. {step.Title}. " +
                    $"{prompt}.");
            }
            else
            {
                bool isLast = _currentIndex == _steps.Length - 1;
                string closing = isLast
                    ? "Say Exit tutorial to close this window."
                    : "Say Next to continue.";
                AnnounceTutorialText(
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
        // STRUCTURE (17 steps total — trimmed from an earlier 18-step version
        // that had a standalone "Navigating Between Panels" reference step
        // duplicating the Ctrl+1..4 / Panel 1..4 rundown already given at the
        // end of the four-panel overview; consolidated without losing coverage)
        //
        //   Section 1 — Audio and microphone test          (steps  1–2,  interactive)
        //   Section 2 — Welcome and orientation            (steps  3–5,  passive)
        //   Section 3 — Panel navigation practice          (step   6,    interactive)
        //   Section 4 — Other voice command overview       (step   7,    passive)
        //   Section 5 — Your first chapter                 (steps  8–11, interactive)
        //   Section 6 — Claude, the Prompt Library & Cards (steps 12–15, mixed)
        //   Section 7 — Practice save                      (step  16,    interactive)
        //   Section 8 — Tutorial complete                  (step  17,    passive)
        // ────────────────────────────────────────────────────────────────

        private static TutorialStep[] BuildSteps(bool jawsDetected, bool dragonDetected)
        {
            // Dynamic sentences inserted into steps where AT state matters.
            // Two scenarios:
            //   A) No Dragon, No JAWS — app reads everything, app mic is on, speak commands directly.
            //   B) Dragon + JAWS     — JAWS reads everything (no app voice), Dragon dictates,
            //                          use ScrollLock or command bar for app commands.

            string micInfo = dragonDetected
                ? "Dragon NaturallySpeaking is running and owns the microphone. " +
                  "The built-in VoiceBook microphone is off so Dragon and the app do not conflict."
                : "The built-in VoiceBook microphone is on right now and listening for spoken commands. " +
                  "You do not need to press anything to activate it.";

            string jawsInfo = jawsDetected
                ? "JAWS screen reader is running. JAWS reads all content in this application — " +
                  "buttons, panels, list items, and all announcements. " +
                  "VoiceBook's own voice is completely silent when JAWS is present so there is never any overlap."
                : "VoiceBook's built-in voice is reading these words to you right now. " +
                  "It will read every tutorial step, every status message, and every system announcement throughout your session. " +
                  "No screen reader is required.";

            string voiceCommandRoute = dragonDetected
                ? "GIVING COMMANDS WITH DRAGON\n\n" +
                  "Dragon owns the microphone for dictation. For VoiceBook app commands you have two options.\n\n" +
                  "SCROLL LOCK — fastest option\n" +
                  "Press ScrollLock once. Dragon mutes, the VoiceBook mic activates. " +
                  "Say a command. Press ScrollLock again to restore Dragon. " +
                  "The ScrollLock key works at any time, including right now during this tutorial.\n\n" +
                  "COMMAND BAR — works without any setup\n" +
                  "Type or dictate a command into the Command box at the bottom of this window and press Enter.\n\n" +
                  "BUTTON CLICKS — requires Dragon MyCommands setup\n" +
                  "WPF buttons require a one-time Dragon MyCommands configuration before voice-clicking works. " +
                  "See the Dragon Commands Setup Guide in the Docs folder for instructions."
                : "GIVING COMMANDS WITH YOUR VOICE\n\n" +
                  "Say any command out loud and the app will act on it immediately. " +
                  "You can also type commands into the Command box below and press Enter.\n\n" +
                  "Try saying: \"Panel two\", \"Add chapter\", or \"What can I say here\".";

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
                            ? "Dragon NaturallySpeaking is running.\n\n" +
                              "THREE WAYS TO CONFIRM AUDIO:\n" +
                              "  1. Say \"click Confirm Audio\" — Dragon clicks the Confirm Audio button below\n" +
                              "  2. Type the word Hello into the Command box below and press Enter\n" +
                              "  3. Press ScrollLock to activate the app mic, say Hello, then press ScrollLock again\n\n" +
                              "Any of these confirms that Dragon and command routing are working."
                            : "If the microphone hears you, the step will pass automatically.\n\n" +
                              "You can also type Hello into the Command box below and press Enter, " +
                              "or press the Confirm Audio button."),
                    RequiredAction = "continue",
                    ActionPrompt   = dragonDetected
                        ? "Say \"click Confirm Audio\", type Hello and press Enter, or press ScrollLock to use the app mic"
                        : "Say Hello, type Hello and press Enter, or press Confirm Audio",
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
                        "Here is how the rest of the tutorial works.\n\n" +
                        "HOW THE TUTORIAL WORKS\n" +
                        "Most steps ask you to listen, then press Next to continue.\n" +
                        "Some steps ask you to perform an action — the tutorial waits and " +
                        "detects when you have done it before moving on.\n\n" +
                        "KEYBOARD SHORTCUTS IN THIS WINDOW\n" +
                        "  N         — Next step\n" +
                        "  P         — Previous step\n" +
                        "  R         — Repeat current step\n" +
                        "  S         — Skip an action step\n" +
                        "  Escape    — Exit tutorial\n\n" +
                        "A GENERAL TIP\n" +
                        "Elsewhere in the app, when a dialog opens asking you to type or dictate " +
                        "something — a project title, a chapter title — press Enter afterward to " +
                        "confirm it. Speaking or typing the text does not submit it by itself."
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
                        "VoiceBook Studio has four panels.\n\n" +
                        "PANEL 1 — Chapter Manager (left side)\n" +
                        "Lists all your book sections in order: front matter, body chapters, " +
                        "back matter. Navigate with the Up and Down arrow keys.\n\n" +
                        "PANEL 2 — Writing Editor (centre)\n" +
                        "Where you write and dictate. " +
                        (dragonDetected
                            ? "Dragon NaturallySpeaking works here exactly as in Microsoft Word — " +
                              "dictate, correct with \"Correct that\", and use all Dragon navigation commands."
                            : "Speak into the microphone to dictate, or type normally. " +
                              "The app mic stays on while you write.") + "\n\n" +
                        "PANEL 3 — AI Assistant (bottom of the window)\n" +
                        "Chat with Claude for feedback and rewrites. Insert a response into your " +
                        "chapter, or say Replace and Claude finds the exact passage it rewrote and " +
                        "swaps it in for you — no selecting or copy/pasting needed.\n\n" +
                        "PANEL 4 — Library (right side)\n" +
                        "Three tabs: Prompts (81 categorized writing prompts you can send straight " +
                        "to Claude), Cards (AI responses you've saved to reuse), and Feedback " +
                        "(your saved chapter and book analyses).\n\n" +
                        "Switch panels by pressing Ctrl+1, Ctrl+2, Ctrl+3, or Ctrl+4/F11, or by saying " +
                        "Panel 1, Panel 2, Panel 3, or Panel 4. From inside the Writing Editor, Escape " +
                        "jumps straight back to the Chapter Manager."
                },

                // ════════════════════════════════════════════════════════
                // SECTION 3 — Panel navigation practice  (step 6)
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title              = "Practice Switching Panels",
                    Content            =
                        "Switch to any panel other than this one — Writing Editor, AI Assistant, " +
                        "or Library.\n\n" +
                        "Say Panel Two, Panel Three, or Panel Four — or press F2, F3, or F11.",
                    RequiredActionsAny = new[] { "panel2", "panel3", "panel4" },
                    ActionPrompt       = "Say Panel Two, Panel Three, or Panel Four, or press F2, F3, or F11",
                    SuccessMessage     = "Panel switching works. Say Panel One, or press Ctrl+1, any time to " +
                                         "return to the Chapter Manager.",
                    IsSkippable        = true
                },

                // ════════════════════════════════════════════════════════
                // SECTION 4 — Other voice commands overview  (step 7, passive)
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
                        "  Word count       — hear the word count for whatever is open now\n" +
                        "  Chapter word count / Book word count — ask for either one specifically\n\n" +
                        "DISCOVERING COMMANDS ANYTIME\n" +
                        "Say What can I say here in any panel to hear the commands available " +
                        "right there — the fastest way to explore without checking the manual.\n\n" +
                        (dragonDetected
                            ? "GIVING COMMANDS WITH DRAGON\n" +
                              "Use ScrollLock (fastest) or the Command box to send app commands while Dragon owns the mic.\n" +
                              "ScrollLock works right now — press it, say a command, press it again to restore Dragon.\n\n" +
                              "To click buttons by voice with Dragon, set up Dragon MyCommands once. " +
                              "See the Dragon Commands Setup Guide in the Docs folder."
                            : "The app microphone is on. Just say any of these commands out loud — " +
                              "no setup needed, no key to press first. " +
                              "Or type them into the Command box below and press Enter.")
                },

                // ════════════════════════════════════════════════════════
                // SECTION 5 — Your first chapter  (steps 8–11)
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
                // SECTION 6 — Claude, the Prompt Library & Cards  (steps 12–15)
                // Comes after a chapter exists so there is real content for Claude
                // to respond to, and a real response to save as a card.
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title   = "Section 6 — Getting Help From Claude",
                    Content =
                        "The AI Assistant panel at the bottom of the window is a running chat " +
                        "with Claude. Anything you type or dictate there, Claude answers with " +
                        "your open chapter as context.\n\n" +
                        "GETTING A RESPONSE INTO YOUR CHAPTER\n" +
                        "Once Claude replies, three Insert buttons add the response into your " +
                        "chapter: at cursor — meaning your last position in the editor, even " +
                        "though you're currently typing here in the chat box — at the start, " +
                        "or at the end.\n\n" +
                        "A fourth option, Replace, is different: say Replace, or click it, and " +
                        "Claude finds the exact original passage its response was meant to " +
                        "rewrite — for example if you asked \"rewrite paragraph 4\" — and swaps " +
                        "it in directly. No selecting text, no copy and paste. If Claude can't " +
                        "confidently pin down a single passage, it says so instead of guessing, " +
                        "and you can use an Insert button instead."
                },

                new TutorialStep
                {
                    Title          = "Ask Claude a Question",
                    Content        =
                        "Try it now. Go to the AI Assistant panel (Panel 3), then say or type a " +
                        "question about your chapter — for example \"How can I improve this " +
                        "opening?\" — and send it.\n\n" +
                        "Say Send, or Send message — or press Enter in the chat box.\n\n" +
                        "The tutorial will detect Claude's response and move on automatically.",
                    RequiredAction = "sendchat",
                    ActionPrompt   = "Ask Claude something in the chat box, then say Send or press Enter",
                    SuccessMessage = "Claude responded. You can Insert or Replace it in your chapter any time.",
                    IsSkippable    = true
                },

                new TutorialStep
                {
                    Title   = "The Prompt Library",
                    Content =
                        "You just wrote your own question, but you don't have to start from " +
                        "blank every time. The Library panel's Prompts tab holds 81 pre-written " +
                        "prompts organised by category, each labelled like A1, A2, A3.\n\n" +
                        "Say Open prompt library any time to jump there — it tells you exactly " +
                        "what to say as soon as you arrive, so there's nothing to memorise now."
                },

                new TutorialStep
                {
                    Title          = "Save a Response as a Card",
                    Content        =
                        "Claude's response from a moment ago is still showing in the AI panel. " +
                        "When a response is worth reusing — a note on your character's voice, a " +
                        "phrasing you like — save it as a card instead of losing it.\n\n" +
                        "Say Save card, or Save response card — or click Save as Card.\n\n" +
                        "A small dialog asks for a title and a category, then saves it to the " +
                        "Library panel's Cards tab. From there, say Card categories to browse " +
                        "them, or Insert card A1 to drop any saved card straight into your " +
                        "chapter later — in any project, any time.",
                    RequiredAction = "savecard",
                    ActionPrompt   = "Say Save card, or click Save as Card, and complete the dialog",
                    SuccessMessage = "Card saved. You can reuse it from the Cards tab any time.",
                    IsSkippable    = true
                },

                // ════════════════════════════════════════════════════════
                // SECTION 7 — Practice save  (step 16, interactive)
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
                // SECTION 8 — Completion  (step 17, passive)
                // ════════════════════════════════════════════════════════

                new TutorialStep
                {
                    Title   = "Tutorial Complete — You Are Ready to Write",
                    Content =
                        "Congratulations. You have completed the VoiceBook Studio tutorial.\n\n" +
                        "QUICK REFERENCE\n" +
                        "  Ctrl+1            — Chapter Manager\n" +
                        "  F2 / Ctrl+2      — Writing Editor\n" +
                        "  F3 / Ctrl+3      — AI Assistant\n" +
                        "  F11 / Ctrl+4     — Library (Prompts, Cards, Feedback)\n" +
                        "  Ctrl+S           — Save\n" +
                        "  Ctrl+A           — Add chapter\n" +
                        "  Ctrl+N           — New project\n" +
                        "  Ctrl+I           — Import Word document\n" +
                        "  Ctrl+F           — Run comprehensive AI feedback\n\n" +
                        "  Send             — send your chat message to Claude\n" +
                        "  Insert at cursor / Insert at start / Insert at end — add a response to your chapter\n" +
                        "  Replace          — swap a response straight into the passage it rewrote\n" +
                        "  Save card        — keep a response for later, in the Cards tab\n" +
                        "  Word count / Chapter word count / Book word count\n\n" +
                        "Say What can I say here at any time to hear context-sensitive commands.\n\n" +
                        "Say Start tutorial at any time to run this tutorial again."
                }
            };
        }
    }
}
