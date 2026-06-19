using System.Threading.Tasks;
using System.Windows;
using VoiceBookStudio.Helpers;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Delivers the five-step first-launch guided tour.  Each step focuses
    /// the relevant panel and announces the step text via the appropriate channel:
    /// UiaAnnouncer (JAWS present) or SystemAnnouncementService (no JAWS).
    /// All step transitions are non-blocking and async.
    /// </summary>
    public sealed class TutorialService
    {
        private readonly SystemAnnouncementService _announcer;
        private readonly ITutorialPresenter        _presenter;
        private readonly FirstLaunchService        _firstLaunch;
        private readonly bool                      _jawsDetected;

        private int  _currentStep = 0;
        private bool _isActive    = false;

        public bool IsActive => _isActive;

        // Panel index (1=chapters, 2=editor, 3=assistant, 0=no change) paired with step text.
        private static readonly (int Panel, string Text)[] Steps =
        {
            (1,
             "Step 1. This is the chapter list on the left side of the screen. " +
             "Your manuscript chapters appear here. " +
             "Click a chapter to open it, or use the chapter commands. " +
             "Say Panel 1 at any time to return focus here. " +
             "Say next step to continue."),

            (2,
             "Step 2. This is the writing editor in the centre of the screen. " +
             "When a chapter is open you dictate directly into the editor using Dragon. " +
             "All standard Dragon editing commands work here, the same as in Microsoft Word. " +
             "Say Press F2 or say Panel 2 to move focus to the editor. " +
             "Say next step to continue."),

            (3,
             "Step 3. This is the AI assistant panel on the right side of the screen. " +
             "Type a question in the chat box and press Enter to ask Claude. " +
             "You can also type any voice command in the chat box and press Enter to run it. " +
             "Say Panel 3 or Press F3 to move focus here. " +
             "Say next step to continue."),

            (0,
             "Step 4. Key commands. " +
             "Say Save or press Control S to save your work. " +
             "Say New chapter to add a chapter. " +
             "Say New project to start a new book. " +
             "Say What can I say here at any time to hear the full list of available commands. " +
             "Say next step to finish the tour."),

            (1,
             "Step 5. The tour is complete. " +
             "VoiceBook Studio is ready. " +
             "Focus is returning to the chapter list. " +
             "Say What can I say here whenever you need help."),
        };

        public TutorialService(
            SystemAnnouncementService announcer,
            ITutorialPresenter        presenter,
            FirstLaunchService        firstLaunch,
            bool                      jawsDetected)
        {
            _announcer    = announcer;
            _presenter    = presenter;
            _firstLaunch  = firstLaunch;
            _jawsDetected = jawsDetected;
        }

        /// <summary>Starts the tour from step 1. Safe to call only once.</summary>
        public void StartTour()
        {
            _isActive    = true;
            _currentStep = 0;
            _ = RunCurrentStepAsync();
        }

        /// <summary>Advances to the next step. No-op when the tour is not active.</summary>
        public void AdvanceStep()
        {
            if (!_isActive) return;
            _announcer.StopSpeaking();
            _currentStep++;
            if (_currentStep >= Steps.Length)
            {
                _ = FinalizeTourAsync();
                return;
            }
            _ = RunCurrentStepAsync();
        }

        /// <summary>Re-announces the current step. No-op when the tour is not active.</summary>
        public void RepeatStep()
        {
            if (!_isActive) return;
            _announcer.StopSpeaking();
            _ = RunCurrentStepAsync();
        }

        /// <summary>Ends the tour immediately without completing all steps.</summary>
        public void EndTour()
        {
            if (!_isActive) return;
            _announcer.StopSpeaking();
            _ = FinalizeTourAsync();
        }

        // ----------------------------------------------------------------
        // Private async core
        // ----------------------------------------------------------------

        private async Task RunCurrentStepAsync()
        {
            int myStep = _currentStep;
            if (myStep >= Steps.Length) return;

            var (panel, text) = Steps[myStep];

            FocusPanel(panel);
            await AnnounceAsync(panel, text);

            // Auto-complete when step 5 finishes and nothing has interrupted.
            if (_isActive && _currentStep == Steps.Length - 1 && myStep == _currentStep)
                _ = FinalizeTourAsync();
        }

        private async Task FinalizeTourAsync()
        {
            if (!_isActive) return; // Guard against concurrent calls from AdvanceStep and RunCurrentStepAsync
            _isActive = false;

            _firstLaunch.MarkTutorialComplete();
            _presenter.FocusChapterList();
            _presenter.NotifyTourComplete();

            await Task.CompletedTask;
        }

        private void FocusPanel(int panel)
        {
            switch (panel)
            {
                case 1: _presenter.FocusChapterList(); break;
                case 2: _presenter.FocusEditor();      break;
                case 3: _presenter.FocusAssistant();   break;
                // 0 = no focus change (voice-commands overview step)
            }
        }

        private async Task AnnounceAsync(int panel, string text)
        {
            if (_jawsDetected)
            {
                UIElement element = panel switch
                {
                    2 => _presenter.EditorElement,
                    3 => _presenter.AssistantElement,
                    _ => _presenter.ChapterListElement,
                };
                UiaAnnouncer.Announce(element, text, isUrgent: false);
                await Task.Delay(3000);
            }
            else
            {
                await _announcer.SpeakAndWaitAsync(text);
            }
        }
    }
}
