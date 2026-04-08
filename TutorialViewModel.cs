using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using VoiceBookStudio.Services;
using VoiceBookStudio.Utils;

namespace VoiceBookStudio.ViewModels
{
    public class TutorialStep
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class TutorialViewModel
    {
        private readonly SystemAnnouncementService _announcer;
        private readonly AudioFeedbackService _audio;

        private int _currentIndex = 0;
        private readonly List<TutorialStep> _steps = new();

        public TutorialViewModel(SystemAnnouncementService announcer, AudioFeedbackService audio)
        {
            _announcer = announcer;
            _audio = audio;

            // Populate steps
            _steps.Add(new TutorialStep { Title = "Welcome & Audio Test", Content = "Can you hear me? Say 'Yes' to continue." });
            _steps.Add(new TutorialStep { Title = "Microphone Test", Content = "Say 'Testing microphone' - I'll repeat it back." });
            _steps.Add(new TutorialStep { Title = "JAWS Detection", Content = "JAWS detected: " + (AppSettings.IsJawsDetected ? "Yes" : "No") });
            _steps.Add(new TutorialStep { Title = "Voice Commands Test", Content = "Say 'Panel 1' to test navigation." });
            _steps.Add(new TutorialStep { Title = "Panel 1: Chapter Manager", Content = "This panel manages chapters. Try adding a chapter." });
            _steps.Add(new TutorialStep { Title = "Panel 2: Writing Editor", Content = "This is the editor. Dictate or type to enter text." });
            _steps.Add(new TutorialStep { Title = "Panel 3: Claude Assistant", Content = "Use the AI panel to ask Claude for feedback." });
            _steps.Add(new TutorialStep { Title = "Prompt Library", Content = "Access ready-made prompts from the prompt library." });
            _steps.Add(new TutorialStep { Title = "Response Cards", Content = "Save and reuse content using response cards." });
            _steps.Add(new TutorialStep { Title = "Wrap-up", Content = "Tutorial complete. You're ready to write!" });
        }

        public int CurrentStep => _currentIndex + 1;
        public int TotalSteps => _steps.Count;

        public string StepCounterDisplay => $"Step {CurrentStep} of {TotalSteps}";
        public string CurrentTitle   => _steps[_currentIndex].Title;
        public string CurrentContent => _steps[_currentIndex].Content;

        public ICommand NextCommand => new RelayCommand(Next, CanNext);
        public ICommand PreviousCommand => new RelayCommand(Previous, CanPrevious);
        public ICommand RepeatCommand => new RelayCommand(Repeat);
        public ICommand ExitCommand => new RelayCommand(Exit);

        public event Action? TutorialCompleted;

        public void Next()
        {
            if (_currentIndex < _steps.Count - 1)
            {
                _currentIndex++;
                AnnounceCurrent();
            }
            else
            {
                // Complete
                AppSettings.TutorialCompleted = true;
                TutorialCompleted?.Invoke();
            }
        }

        public void Previous()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                AnnounceCurrent();
            }
        }

        public void Repeat()
        {
            AnnounceCurrent();
        }

        public void Exit()
        {
            AppSettings.TutorialCompleted = false;
            TutorialCompleted?.Invoke();
        }

        private bool CanNext() => _currentIndex < _steps.Count - 1;
        private bool CanPrevious() => _currentIndex > 0;

        private void AnnounceCurrent()
        {
            string header = $"Step {CurrentStep} of {TotalSteps}: {CurrentTitle}";
            // Special handling for the Voice Commands Test step: detect Dragon (natspeak.exe)
            if (_currentIndex == 3) // zero-based index for step 4
            {
                bool dragon = Process.GetProcessesByName("natspeak").Length > 0;
                string dragonStatus = dragon ? "Dragon detected: Yes." : "Dragon detected: No. Please start Dragon NaturallySpeaking.";
                _announcer.Speak(header + ". " + dragonStatus + " " + CurrentContent);
            }
            else
            {
                _announcer.Speak(header + ". " + CurrentContent);
            }
        }

        public void Start()
        {
            _currentIndex = 0;
            AnnounceCurrent();
        }
    }
}
