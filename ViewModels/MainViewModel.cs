using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VoiceBookStudio.Models;
using VoiceBookStudio.Services;
using VoiceBookStudio.Views;

namespace VoiceBookStudio.ViewModels
{
    // ----------------------------------------------------------------
    // Insert position enum shared with the View
    // ----------------------------------------------------------------

    public enum InsertPosition { AtCursor, AtBeginning, AtEnd }

    public sealed class InsertTextArgs : EventArgs
    {
        public string         Text     { get; }
        public InsertPosition Position { get; }
        public InsertTextArgs(string text, InsertPosition position)
        {
            Text     = text;
            Position = position;
        }
    }

    // ----------------------------------------------------------------
    // ViewModel
    // ----------------------------------------------------------------

    /// <summary>
    /// Primary ViewModel for the main window.
    /// Owns all project state, chapter management, and coordinates services.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly ProjectService            _projectService;
        private readonly AudioFeedbackService      _audio;
        private readonly SystemAnnouncementService _systemAnnouncements;
        private readonly AiService                 _aiService;
        private readonly AppSoundService           _sounds;
        private readonly ResponseCardService       _responseCardService;
        private readonly FeedbackLibraryService    _feedbackLibraryService;

        private string? _currentFilePath;

        private readonly DispatcherTimer _autoSaveTimer;

        // ----------------------------------------------------------------
        // Sub-ViewModels (Prompts tab and Cards tab)
        // ----------------------------------------------------------------

        /// <summary>Bound to the Prompts tab in the AI panel.</summary>
        public PromptLibraryViewModel PromptLibVM { get; }

        /// <summary>Bound to the Cards tab in the AI panel.</summary>
        public ResponseCardViewModel ResponseCardVM { get; }

        /// <summary>Bound to the Feedback tab in the AI panel.</summary>
        public FeedbackLibraryViewModel FeedbackLibVM { get; }

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        /// <summary>Exposed so views can pass the shared audio service to dialogs.</summary>
        public AudioFeedbackService AudioService => _audio;

        /// <summary>Exposed so MainWindow can play sounds for voice commands and app close.</summary>
        public AppSoundService SoundService => _sounds;

        public MainViewModel(ProjectService             projectService,
                             AudioFeedbackService       audio,
                             AiService                  aiService,
                             SystemAnnouncementService  systemAnnouncements,
                             AppSoundService            sounds)
        {
            _projectService      = projectService;
            _audio               = audio;
            _systemAnnouncements = systemAnnouncements;
            _aiService           = aiService;
            _sounds              = sounds;

            // Prompt library — loads from Data/PromptLibrary/prompts.json
            PromptLibVM = new PromptLibraryViewModel(
                new VoiceBookStudio.Services.PromptLibraryService(),
                _systemAnnouncements);

            // When the user picks a prompt, load it into the chat box and switch to Chat.
            PromptLibVM.PromptSelected += (_, content) =>
            {
                ChatInputText = content;
                SwitchAiTabRequested?.Invoke(this, "Chat");
                SetStatus("Prompt loaded. Edit it if needed, then click Send.");
            };

            // Response cards
            _responseCardService = new VoiceBookStudio.Services.ResponseCardService();
            ResponseCardVM = new ResponseCardViewModel(_responseCardService, _systemAnnouncements);

            // When a card is inserted, route it to the editor via InsertTextRequested.
            ResponseCardVM.InsertCardRequested += (_, card) => InsertCardAtCursor(card);

            // Feedback library — auto-receives entries from RunAiFeedbackAsync.
            _feedbackLibraryService = new VoiceBookStudio.Services.FeedbackLibraryService();
            FeedbackLibVM = new FeedbackLibraryViewModel(
                _feedbackLibraryService,
                _systemAnnouncements,
                items => StartLibraryReading(items));

            // Auto-save every 30 seconds — only fires when there are unsaved changes.
            _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _autoSaveTimer.Tick += AutoSave_Tick;
            _autoSaveTimer.Start();

            // Keep DisplayItems in sync whenever the Chapters collection changes.
            Chapters.CollectionChanged += (_, _) => RebuildDisplayItems();
            RebuildDisplayItems();
        }

        // When SelectedChapter is set directly (e.g. voice-nav), mirror it into
        // SelectedDisplayItem so the ListBox highlights the correct row.
        // Skip the sync when WholeBook is active (SelectedChapter is null there).
        partial void OnSelectedChapterChanged(ChapterViewModel? value)
        {
            if (value != null || !IsWholeBookSelected)
                SelectedDisplayItem = value;
        }

        public void SetProjectSelection(ProjectSelectionViewModel proj)
        {
            _projectSelection = proj;
        }

        // ----------------------------------------------------------------
        // Observable properties
        // ----------------------------------------------------------------

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WindowTitle))]
        [NotifyPropertyChangedFor(nameof(HasProject))]
        [NotifyCanExecuteChangedFor(nameof(AddChapterCommand))]
        [NotifyCanExecuteChangedFor(nameof(RenameChapterCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteChapterCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveChapterUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveChapterDownCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveProjectCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveProjectAsCommand))]
        [NotifyCanExecuteChangedFor(nameof(RunAiFeedbackCommand))]
        [NotifyCanExecuteChangedFor(nameof(SendChatCommand))]
        [NotifyCanExecuteChangedFor(nameof(ExportDocxCommand))]
        [NotifyCanExecuteChangedFor(nameof(ExportPdfCommand))]
        private VoiceBookProject? _project;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WindowTitle))]
        private bool _isModified;

        [ObservableProperty]
        private ObservableCollection<ChapterViewModel> _chapters = new();

        /// <summary>
        /// The list displayed in ChapterListBox — always starts with the pinned WholeBook entry
        /// followed by real chapter view-models in sorted order.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<object> _displayItems = new();

        /// <summary>The pinned "Whole Book" sentinel, always at DisplayItems[0].</summary>
        public WholeBookViewModel WholeBook { get; } = new WholeBookViewModel();

        /// <summary>True while the "Whole Book" entry is the active selection.</summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RunAiFeedbackCommand))]
        [NotifyCanExecuteChangedFor(nameof(SendChatCommand))]
        private bool _isWholeBookSelected;

        /// <summary>
        /// Drives ChapterListBox.SelectedItem (TwoWay). Mirrors SelectedChapter for real
        /// chapters; points to WholeBook when the whole-book entry is active.
        /// </summary>
        [ObservableProperty]
        private object? _selectedDisplayItem;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelectedChapter))]
        [NotifyPropertyChangedFor(nameof(EditorTitle))]
        [NotifyPropertyChangedFor(nameof(WordCountDisplay))]
        [NotifyCanExecuteChangedFor(nameof(RenameChapterCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteChapterCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveChapterUpCommand))]
        [NotifyCanExecuteChangedFor(nameof(MoveChapterDownCommand))]
        [NotifyCanExecuteChangedFor(nameof(RunAiFeedbackCommand))]
        [NotifyCanExecuteChangedFor(nameof(SendChatCommand))]
        [NotifyCanExecuteChangedFor(nameof(InsertAtCursorCommand))]
        [NotifyCanExecuteChangedFor(nameof(InsertAtStartCommand))]
        [NotifyCanExecuteChangedFor(nameof(InsertAtEndCommand))]
        [NotifyCanExecuteChangedFor(nameof(ChangeChapterTypeCommand))]
        private ChapterViewModel? _selectedChapter;

        [ObservableProperty]
        private string _statusMessage = "Welcome to VoiceBook Studio.";

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InsertAtCursorCommand))]
        [NotifyCanExecuteChangedFor(nameof(InsertAtStartCommand))]
        [NotifyCanExecuteChangedFor(nameof(InsertAtEndCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveAsCardCommand))]
        [NotifyPropertyChangedFor(nameof(HasAiResponse))]
        private string _aiFeedbackText = "Select a chapter and run an AI analysis to see feedback here.";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AppTtsStatusDisplay))]
        private bool _appTtsEnabled = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MicStatusDisplay))]
        private bool _isMicListening;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ApiKeyStatusDisplay))]
        [NotifyPropertyChangedFor(nameof(AiStatusDisplay))]
        [NotifyCanExecuteChangedFor(nameof(RunAiFeedbackCommand))]
        [NotifyCanExecuteChangedFor(nameof(SendChatCommand))]
        private bool _isApiKeySet;

        [ObservableProperty]
        private string _chatInputText = string.Empty;

        // ----------------------------------------------------------------
        // Derived properties
        // ----------------------------------------------------------------

        public string WindowTitle =>
            Project == null
                ? "VoiceBook Studio"
                : $"VoiceBook Studio — {Project.Title}{(IsModified ? " *" : string.Empty)}";

        public bool HasProject         => Project != null;
        public bool HasSelectedChapter => SelectedChapter != null;

        public bool HasAiResponse =>
            !string.IsNullOrWhiteSpace(AiFeedbackText) &&
            AiFeedbackText != "Select a chapter and run an AI analysis to see feedback here.";

        public string EditorTitle =>
            SelectedChapter == null ? "No chapter selected" : SelectedChapter.Title;

        public string WordCountDisplay =>
            SelectedChapter == null
                ? "Words: 0"
                : $"Words: {SelectedChapter.WordCount:N0}";

        public string AiStatusDisplay =>
            IsApiKeySet ? "AI: Ready" : "AI: Not configured";

        public string ApiKeyStatusDisplay =>
            IsApiKeySet ? "✓ AI Active" : "⚠ AI Not Set";

        public string AppTtsStatusDisplay =>
            AppTtsEnabled ? "App voice: On" : "App voice: Off";

        public string MicStatusDisplay =>
            IsMicListening ? "Mic: On" : "Mic: Off";

        public string JawsStatusDisplay =>
            VoiceBookStudio.Utils.AppSettings.IsJawsDetected ? "JAWS: Running" : "JAWS: Not detected";

        public string DragonStatusDisplay =>
            VoiceBookStudio.Utils.AppSettings.IsDragonRunning ? "Dragon: Running" : string.Empty;

        public bool IsDragonRunning => VoiceBookStudio.Utils.AppSettings.IsDragonRunning;

        /// <summary>
        /// False when Dragon is running — prevents the built-in mic listener from
        /// competing with Dragon for the audio device. Dragon handles all speech input.
        /// </summary>
        public bool IsMicButtonEnabled => !VoiceBookStudio.Utils.AppSettings.IsDragonRunning;

        public string VoiceStatusDisplay =>
            VoiceBookStudio.Utils.AppSettings.IsAzureTtsConfigured
                ? $"Voice: Azure ({VoiceBookStudio.Utils.AppSettings.AzureVoiceName.Replace("en-US-", "").Replace("Neural", "")})"
                : "Voice: Windows SAPI";

        // ----------------------------------------------------------------
        // Events raised when the View should perform UI-level actions
        // ----------------------------------------------------------------

        public event EventHandler<InsertTextArgs>? InsertTextRequested;

        /// <summary>
        /// Raised by FocusPanel1/2/3 so the View can move keyboard focus to the
        /// correct panel without the ViewModel touching UI objects directly.
        /// Payload is the panel number (1, 2, or 3).
        /// </summary>
        public event EventHandler<int>? FocusPanelRequested;

        /// <summary>
        /// Raised when the AI panel should switch to a named tab.
        /// Payload: "Chat", "Prompts", or "Cards".
        /// </summary>
        public event EventHandler<string>? SwitchAiTabRequested;

        /// <summary>Raised after a chat response so the view refocuses the chat input field.</summary>
        public event EventHandler? FocusChatInputRequested;

        /// <summary>
        /// Raised when the five-step guided tour starts.
        /// The payload is the new TutorialService so MainWindow can register it
        /// with the voice router.
        /// </summary>
        public event EventHandler<TutorialService>? TourStarted;

        /// <summary>Raised when the guided tour ends so the voice router can be unhooked.</summary>
        public event EventHandler? TourEnded;

        private TutorialViewModel? _tutorial;
        private TutorialService?  _guidedTour;
        private ProjectSelectionViewModel? _projectSelection;

        // Receives app action codes and forwards them to the active tutorial.
        // Null when no tutorial is running, so the invocation is always safe.
        private Action<string>? _tutorialActionSink;

        // ----------------------------------------------------------------
        // Library reading controller
        // ----------------------------------------------------------------

        private List<string>               _readingQueue   = new();
        private bool                       _isReadingActive;
        private System.Threading.CancellationTokenSource? _readingCts;

        // Tracks which library was last navigated so bare "Read A1" commands are routed correctly.
        private string _lastLibraryContext = "feedback";

        public bool IsReadingActive => _isReadingActive;

        public void StartLibraryReading(System.Collections.Generic.IEnumerable<string> items)
        {
            StopLibraryReading();
            _readingQueue = new List<string>(items);
            _ = ContinueReadingAsync();
        }

        public void StopLibraryReading()
        {
            _isReadingActive = false;
            _readingCts?.Cancel();
            _systemAnnouncements.StopSpeaking();
        }

        public void ResumeLibraryReading()
        {
            if (_readingQueue.Count == 0)
            {
                _systemAnnouncements.Speak("Nothing to resume.");
                return;
            }
            _readingCts?.Cancel();
            _ = ContinueReadingAsync();
        }

        private async System.Threading.Tasks.Task ContinueReadingAsync()
        {
            _isReadingActive = true;
            _readingCts?.Dispose();
            _readingCts = new System.Threading.CancellationTokenSource();
            var token = _readingCts.Token;

            try
            {
                while (_readingQueue.Count > 0 && !token.IsCancellationRequested)
                {
                    string item = _readingQueue[0];
                    await _systemAnnouncements.SpeakAndWaitAsync(item);
                    if (!token.IsCancellationRequested)
                    {
                        _readingQueue.RemoveAt(0);
                        if (_readingQueue.Count > 0)
                            await System.Threading.Tasks.Task.Delay(300, token);
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally { _isReadingActive = false; }
        }

        // Current panel — 1=Chapters, 2=Editor, 3=AI. Updated on every panel switch
        // so "what can I say here" always gives context-relevant help.
        private int _currentPanel = 1;

        // ----------------------------------------------------------------
        // Panel focus — called by VoiceCommandRouter and keyboard shortcuts
        // ----------------------------------------------------------------

        /// <summary>Say "Panel 1" or "Go to panel 1" to move focus here.</summary>
        public void FocusPanel1()
        {
            _currentPanel = 1;
            FocusPanelRequested?.Invoke(this, 1);
            SetStatus("Chapter Manager panel focused.");
            _audio.Speak("Chapters panel.");
            _tutorialActionSink?.Invoke("panel1");
        }

        /// <summary>Say "Panel 2" or "Go to panel 2" to move focus here.</summary>
        public void FocusPanel2()
        {
            _currentPanel = 2;
            FocusPanelRequested?.Invoke(this, 2);
            SetStatus("Writing Editor panel focused.");
            _audio.Speak("Editor panel.");
            _tutorialActionSink?.Invoke("panel2");
        }

        /// <summary>Say "Panel 3" or "Go to panel 3" to move focus here.</summary>
        public void FocusPanel3()
        {
            _currentPanel = 3;
            FocusPanelRequested?.Invoke(this, 3);
            SetStatus("AI Assistant panel focused.");
            _audio.Speak("AI Assistant panel.");
            _tutorialActionSink?.Invoke("panel3");
        }

        // Methods invoked by VoiceCommandRouter for tutorial navigation.
        public void TryExecuteTutorialNext()     => _tutorial?.Next();
        public void TryExecuteTutorialPrevious() => _tutorial?.Previous();
        public void TryExecuteTutorialRepeat()   => _tutorial?.Repeat();
        public void TryExecuteTutorialExit()     => _tutorial?.Exit();
        public void TryExecuteTutorialSkip()     => _tutorial?.SkipStep();
        public void TryExecuteStartTutorial()    => StartTutorial();
        public void TryExecuteTutorialContinue() => _tutorialActionSink?.Invoke("continue");

        /// <summary>
        /// Signals an action code to the running tutorial (e.g. from a key intercept).
        /// No-op when no tutorial is active.
        /// </summary>
        public void TrySignalTutorialAction(string action) => _tutorialActionSink?.Invoke(action);

        // ----------------------------------------------------------------
        // Prompt library — voice router hooks
        // ----------------------------------------------------------------

        public void TryOpenPromptLibrary()
        {
            SwitchAiTabRequested?.Invoke(this, "Prompts");
            SetStatus("Prompts tab opened.");
            _audio.Speak("Prompts tab.");
        }

        public void TryUsePromptById(string promptId)
        {
            PromptLibVM.SelectById(promptId);
        }

        public void TryReadPromptCategories()
        {
            _lastLibraryContext = "prompts";
            SwitchAiTabRequested?.Invoke(this, "Prompts");
            StartLibraryReading(PromptLibVM.GetCategoryReadingList());
        }

        public void TryReadPromptCategory(string letter)
        {
            _lastLibraryContext = "prompts";
            SwitchAiTabRequested?.Invoke(this, "Prompts");
            StartLibraryReading(PromptLibVM.GetCategoryEntryList(letter));
        }

        public void TryAddPrompt()
        {
            var categories  = PromptLibVM.GetExistingCategories();
            string nextLetter = PromptLibVM.GetNextAvailableLetter();

            var dlg = new Views.AddPromptDialog(categories, nextLetter)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            if (dlg.ShowDialog() != true) return;

            string id = PromptLibVM.AddPrompt(dlg.CategoryLetter, dlg.CategoryName,
                                               dlg.PromptTitle, dlg.PromptContent);
            SwitchAiTabRequested?.Invoke(this, "Prompts");
            string msg = $"Prompt {id} added: {dlg.PromptTitle}";
            SetStatus(msg);
            _systemAnnouncements.Speak(msg);
        }

        // ----------------------------------------------------------------
        // Response cards — voice router hooks
        // ----------------------------------------------------------------

        public void TryOpenResponseCards()
        {
            SwitchAiTabRequested?.Invoke(this, "Cards");
            SetStatus("Response cards tab opened.");
            _audio.Speak("Response cards tab.");
        }

        public void TryReadCardCategories()
        {
            _lastLibraryContext = "cards";
            SwitchAiTabRequested?.Invoke(this, "Cards");
            StartLibraryReading(ResponseCardVM.GetCategoryReadingList());
        }

        public void TryReadCardCategory(string letter)
        {
            _lastLibraryContext = "cards";
            SwitchAiTabRequested?.Invoke(this, "Cards");
            StartLibraryReading(ResponseCardVM.GetCategoryEntryList(letter));
        }

        public void TryInsertCardByLetterNumber(string id)
        {
            ResponseCardVM.InsertCardByLetterNumber(id);
        }

        // ----------------------------------------------------------------
        // Feedback library — voice router hooks
        // ----------------------------------------------------------------

        public void TryOpenFeedbackLibrary()
        {
            SwitchAiTabRequested?.Invoke(this, "Feedback");
            SetStatus("Feedback library tab opened.");
            _audio.Speak("Feedback library.");
        }

        public void TryReadFeedbackCategories()
        {
            _lastLibraryContext = "feedback";
            SwitchAiTabRequested?.Invoke(this, "Feedback");
            StartLibraryReading(FeedbackLibVM.GetCategoryReadingList());
        }

        public void TryReadFeedbackCategory(string letter)
        {
            _lastLibraryContext = "feedback";
            SwitchAiTabRequested?.Invoke(this, "Feedback");
            StartLibraryReading(FeedbackLibVM.GetCategoryEntryList(letter));
        }

        public void TryReadEntry(string id)
        {
            switch (_lastLibraryContext)
            {
                case "prompts":
                    PromptLibVM.SelectById(id);
                    break;
                case "cards":
                    ResponseCardVM.InsertCardByLetterNumber(id);
                    break;
                default: // "feedback" and fallback
                    _lastLibraryContext = "feedback";
                    SwitchAiTabRequested?.Invoke(this, "Feedback");
                    StartLibraryReading(FeedbackLibVM.GetEntryText(id));
                    break;
            }
        }

        public void TrySaveResponseCard()
        {
            if (SaveAsCardCommand.CanExecute(null))
                SaveAsCardCommand.Execute(null);
        }

        public void TryInsertCard(int oneBased)
        {
            ResponseCardVM.InsertCardByNumber(oneBased);
        }

        public void TryFilterCards(string category)
        {
            ResponseCardVM.FilterByCategory(category);
            SwitchAiTabRequested?.Invoke(this, "Cards");
        }

        public void TryDeleteCard(int oneBased)
        {
            ResponseCardVM.DeleteCardByNumber(oneBased);
        }

        // Project selection actions invoked by voice router
        public void TryOpenProjectByName(string name)
        {
            // If a project selection VM is active, attempt to match by name
            var vm = _projectSelection;
            if (vm == null) return;

            var match = vm.RecentProjects.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
            if (match != null)
            {
                // Load project
                TryLoadProject(match.Path);
            }
        }

        public void TryCreateNewProject()
        {
            // Signal tutorial that the user chose the new-project path (step 13 detection).
            // TryImportDocument does the same for the import path; voice "new project"
            // routes here and must fire the same code so step 13 can advance.
            _tutorialActionSink?.Invoke("newproject_or_import");
            _ = NewProjectAsync();
        }

        public void TryBrowseForProject()
        {
            // Open file dialog to browse for project
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open VoiceBook Project",
                Filter = ProjectService.FileFilter,
                DefaultExt = ProjectService.FileExtension
            };

            if (dlg.ShowDialog() == true)
            {
                TryLoadProject(dlg.FileName);
            }
        }

        private async void TryLoadProject(string path)
        {
            try
            {
                IsBusy = true;
                Project = await _projectService.LoadAsync(path);
                _currentFilePath = path;
                IsModified = false;
                LoadChapters();
                SelectFirstChapter();
                string? loadRootDir = System.IO.Path.GetDirectoryName(path);
                if (loadRootDir != null) ProjectService.EnsureProjectFolderStructure(loadRootDir);
                OnProjectFolderChanged(loadRootDir ?? string.Empty);

                _sounds.Play(AppSound.ProjectOpened);
                SetStatus($"Opened: {Project.Title}");
                _systemAnnouncements.Speak($"Project opened: {Project.Title}");
                _tutorialActionSink?.Invoke("projectopened");
            }
            catch (Exception ex)
            {
                _sounds.Play(AppSound.Error);
                ShowError("Open Failed", ex.Message);
                _systemAnnouncements.Speak("Could not open project. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ----------------------------------------------------------------
        // Startup
        // ----------------------------------------------------------------

        public async Task InitialiseAsync()
        {
            IsApiKeySet = ApiKeyService.HasApiKey();

            // The full AT-status and ready announcements have already been spoken by
            // App.xaml.cs before the window was shown. This method handles post-show
            // initialisation: sound confirmation, status bar, mic, and welcome dialog.

            _sounds.Play(AppSound.AppReady);

            // Display the AT detection summary in the status bar so it is visible
            // to any sighted helper and available to JAWS via the live region.
            SetStatus(AssistiveTechnologyDetector.BuildStartupStatusMessage());

            // Start the built-in microphone if Dragon is not handling voice input.
            bool dragonOn = VoiceBookStudio.Utils.AppSettings.IsDragonRunning;
            if (!dragonOn)
            {
                _startupMicToggle = true;
                MicToggleRequested?.Invoke(this, true);
                _startupMicToggle = false;
                await Task.Delay(300);
            }

            // First-launch welcome and tutorial flow.
            if (!VoiceBookStudio.Utils.AppSettings.FirstLaunchComplete)
            {
                await Task.Delay(1000);

                try
                {
                    var welcomeVm = new WelcomeDialogViewModel(_systemAnnouncements);
                    var dlg = new Views.WelcomeDialog
                    {
                        DataContext = welcomeVm,
                        Owner       = System.Windows.Application.Current.MainWindow
                    };
                    var dlgClosed = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);
                    dlg.Closed += (_, _) => dlgClosed.TrySetResult(true);
                    dlg.Show();
                    await dlgClosed.Task;

                    if (welcomeVm.StartRequested)
                    {
                        // Launch the full interactive tutorial (17-step TutorialDialog).
                        // FirstLaunchComplete is set only when the user finishes or
                        // explicitly skips within the tutorial (TutorialViewModel.TutorialCompleted).
                        StartTutorial();
                    }
                    else
                    {
                        // User dismissed the WelcomeDialog without starting the tutorial
                        // (Skip Tour button, X button, Alt+F4, or owner-window-close cascade).
                        // Leave FirstLaunchComplete = false so the dialog auto-starts again
                        // on the next launch, giving them another chance to engage.
                        FocusPanelRequested?.Invoke(this, 1);
                        _currentPanel = 1;
                        string skipMsg = "VoiceBook Studio ready. Panel 1 focused.";
                        SetStatus(skipMsg);
                        _systemAnnouncements.Speak(skipMsg);
                    }
                }
                catch (Exception ex)
                {
                    SetStatus($"Welcome dialog could not open: {ex.Message}");
                }
            }
        }

        private void StartTutorial()
        {
            _tutorial = new TutorialViewModel(
                _systemAnnouncements, _audio, _sounds,
                jawsDetected:   VoiceBookStudio.Utils.AppSettings.IsJawsDetected,
                dragonDetected: VoiceBookStudio.Utils.AppSettings.IsDragonRunning);

            _tutorialActionSink = _tutorial.HandleAction;

            var dlg = new Views.TutorialDialog
            {
                DataContext = _tutorial,
                Owner       = System.Windows.Application.Current.MainWindow
            };

            _tutorial.TutorialCompleted += () =>
            {
                _tutorialActionSink = null;
                _tutorial           = null;

                // Mark both FirstLaunchComplete (JSON) and TutorialCompleted (Registry)
                // through the shared service so the auto-start gate is only cleared
                // when the user explicitly finishes or skips within the tutorial.
                new FirstLaunchService().MarkTutorialComplete();
                _sounds.Play(AppSound.TutorialComplete);

                // Announce completion and focus Panel 1.
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    dlg.Close();
                    FocusPanelRequested?.Invoke(this, 1);
                    _currentPanel = 1;
                    string doneMsg =
                        "Tutorial complete. VoiceBook Studio is ready. " +
                        "Panel 1 focused. Say What can I say here at any time for available commands.";
                    SetStatus(doneMsg);
                    _systemAnnouncements.Speak(doneMsg);
                });
            };

            dlg.Show();
        }

        /// <summary>
        /// Starts the five-step guided tour on the main window.
        /// Non-blocking: the tour runs asynchronously after this method returns.
        /// </summary>
        public void StartGuidedTour(ITutorialPresenter presenter)
        {
            var firstLaunch = new FirstLaunchService();
            _guidedTour = new TutorialService(
                _systemAnnouncements,
                presenter,
                firstLaunch,
                VoiceBookStudio.Utils.AppSettings.IsJawsDetected);

            TourStarted?.Invoke(this, _guidedTour);
            _guidedTour.StartTour();
        }

        /// <summary>
        /// Called by MainWindow's ITutorialPresenter.NotifyTourComplete when the tour ends.
        /// Clears the tour reference and returns focus to panel 1.
        /// </summary>
        public void OnTourComplete()
        {
            _guidedTour = null;
            TourEnded?.Invoke(this, System.EventArgs.Empty);
            FocusPanelRequested?.Invoke(this, 1);
            _currentPanel = 1;
        }

        // Exposed command to explicitly open the welcome dialog / tutorial
        [RelayCommand]
        private void ShowWelcome()
        {
            var dlg = new Views.WelcomeDialog
            {
                DataContext = new WelcomeDialogViewModel(_systemAnnouncements),
                Owner = System.Windows.Application.Current.MainWindow
            };

            dlg.ShowDialog();

            if ((dlg.DataContext as WelcomeDialogViewModel)?.StartRequested == true)
                StartTutorial();
        }

        // ----------------------------------------------------------------
        // Project commands
        // ----------------------------------------------------------------

        [RelayCommand]
        private async Task NewProjectAsync()
        {
            if (!await ConfirmDiscardChangesAsync()) return;

            string? title = PromptText("New Project", "Enter project title:", "Untitled Project");
            if (title == null) return;

            Project          = _projectService.CreateNew(title);
            _currentFilePath = null;
            IsModified       = false;
            LoadChapters();
            SelectFirstChapter();

            // If a default project folder is configured, auto-save without prompting.
            string defaultFolder = VoiceBookStudio.Utils.AppSettings.DefaultProjectFolder;
            if (!string.IsNullOrWhiteSpace(defaultFolder))
            {
                try
                {
                    string safeName   = SanitizeFileName(title);
                    string projectDir = System.IO.Path.Combine(defaultFolder, safeName);
                    System.IO.Directory.CreateDirectory(projectDir);
                    ProjectService.EnsureProjectFolderStructure(projectDir);
                    _currentFilePath = System.IO.Path.Combine(projectDir, safeName + ProjectService.FileExtension);
                    await SaveToPathAsync(_currentFilePath);
                    SetStatus($"New project created: {title}. Project folder is ready.");
                    _systemAnnouncements.Speak($"Project {title} created. Project folder is ready.");
                }
                catch (Exception ex)
                {
                    _currentFilePath = null;
                    _sounds.Play(AppSound.ProjectOpened);
                    SetStatus($"New project created: {title}. Auto-save failed — {ex.Message}");
                    _systemAnnouncements.Speak($"Project created: {title}. Could not auto-save.");
                }
            }
            else
            {
                _sounds.Play(AppSound.ProjectOpened);
                SetStatus($"New project created: {title}");
                _systemAnnouncements.Speak($"Project opened: {title}");
            }

            _tutorialActionSink?.Invoke("projectopened");
            OnProjectFolderChanged(_currentFilePath != null
                ? System.IO.Path.GetDirectoryName(_currentFilePath) ?? string.Empty
                : string.Empty);
        }

        [RelayCommand]
        private async Task OpenProjectAsync()
        {
            if (!await ConfirmDiscardChangesAsync()) return;

            var dialog = new OpenFileDialog
            {
                Title            = "Open VoiceBook Project",
                Filter           = ProjectService.FileFilter,
                DefaultExt       = ProjectService.FileExtension,
                InitialDirectory = VoiceBookStudio.Utils.AppSettings.DefaultProjectFolder
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                IsBusy  = true;
                Project = await _projectService.LoadAsync(dialog.FileName);
                _currentFilePath = dialog.FileName;
                IsModified       = false;
                LoadChapters();
                SelectFirstChapter();
                string? openRootDir = System.IO.Path.GetDirectoryName(dialog.FileName);
                if (openRootDir != null) ProjectService.EnsureProjectFolderStructure(openRootDir);
                OnProjectFolderChanged(openRootDir ?? string.Empty);

                _sounds.Play(AppSound.ProjectOpened);
                string msg = $"Opened: {Project.Title}. {Project.Chapters.Count} chapters.";
                SetStatus(msg);
                _systemAnnouncements.Speak($"Project opened: {Project.Title}");
            }
            catch (Exception ex)
            {
                ShowError("Open Failed", ex.Message);
                _systemAnnouncements.Speak("Could not open project. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        private async Task SaveProjectAsync()
        {
            if (Project == null) return;
            FlushEditorToChapter();

            if (string.IsNullOrEmpty(_currentFilePath))
            {
                await SaveProjectAsAsync();
                return;
            }

            await SaveToPathAsync(_currentFilePath);
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        private async Task SaveProjectAsAsync()
        {
            if (Project == null) return;
            FlushEditorToChapter();

            var dialog = new SaveFileDialog
            {
                Title            = "Save VoiceBook Project",
                Filter           = ProjectService.FileFilter,
                DefaultExt       = ProjectService.FileExtension,
                FileName         = Project.Title,
                InitialDirectory = VoiceBookStudio.Utils.AppSettings.DefaultProjectFolder
            };

            if (dialog.ShowDialog() != true) return;
            await SaveToPathAsync(dialog.FileName);
        }

        private async Task SaveToPathAsync(string path)
        {
            try
            {
                IsBusy = true;
                foreach (var cvm in Chapters) cvm.FlushToModel();

                await _projectService.SaveAsync(Project!, path);
                _currentFilePath = path;
                IsModified       = false;
                OnProjectFolderChanged(System.IO.Path.GetDirectoryName(path) ?? string.Empty);

                _sounds.Play(AppSound.ProjectSaved);
                SetStatus($"Saved: {System.IO.Path.GetFileName(path)}");
                _systemAnnouncements.Speak("Project saved");

                // Notify the tutorial that a save occurred (only user-initiated saves
                // reach here; auto-save calls _projectService.SaveAsync directly).
                _tutorialActionSink?.Invoke("save");
            }
            catch (Exception ex)
            {
                ShowError("Save Failed", ex.Message);
                _audio.Speak("Save failed. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ----------------------------------------------------------------
        // Import document command
        // ----------------------------------------------------------------

        [RelayCommand]
        private async Task ImportDocumentAsync()
        {
            // If no project is open, offer to create one from the document name
            if (Project == null)
            {
                var create = MessageBox.Show(
                    "No project is open. A new project will be created from your document.\n\n" +
                    "Continue?",
                    "Import Document",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (create != MessageBoxResult.Yes) return;
            }

            var picker = new OpenFileDialog
            {
                Title      = "Import Word Document",
                Filter     = "Word Documents (*.docx)|*.docx|All Files (*.*)|*.*",
                DefaultExt = ".docx"
            };
            if (picker.ShowDialog() != true) return;

            string filePath      = picker.FileName;
            string suggestedName = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                IsBusy = true;
                SetStatus("Reading document…");
                _systemAnnouncements.Speak("Importing document...");

                var docxService = new DocxImportService();
                // Extract paragraphs with style metadata for better detection
                var paragraphs = docxService.ExtractParagraphs(filePath);
                string fullText = string.Join("\n\n", paragraphs.Select(p => p.Text));

                if (string.IsNullOrWhiteSpace(fullText))
                {
                IsBusy = false;
                MessageBox.Show(
                        "The document appears to be empty or could not be read.",
                        "Import Document", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _systemAnnouncements.Speak("The document is empty.");
                    return;
                }

                // Auto-create project if needed, now that we know the doc is valid
                if (Project == null)
                {
                    string? projectTitle = PromptText("New Project", "Enter project title:", suggestedName);
                    if (projectTitle == null) { IsBusy = false; return; }

                    Project          = _projectService.CreateNew(projectTitle);
                    // Remove the default blank chapter — we will add imported chapters
                    Project.Chapters.Clear();
                    _currentFilePath = null;
                    IsModified       = false;
                    LoadChapters();
                }

                List<DetectedChapter>? detected = null;

                // Try heuristic detection first (fast, free, works offline).
                var detector       = new VoiceBookStudio.Services.ChapterDetectionService();
                var heuristicHits  = detector.DetectByPatterns(paragraphs);
                if (heuristicHits.Count >= 2)
                {
                    SetStatus($"Detected {heuristicHits.Count} chapters from headings.");
                    detected = SplitByHeuristicBreaks(paragraphs, heuristicHits);
                }
                else if (_aiService.IsAvailable)
                {
                    SetStatus("Asking Claude to detect chapter breaks…");
                    _audio.Speak("Asking Claude to detect chapter breaks. This may take a moment.");

                    try   { detected = await _aiService.DetectChaptersAsync(fullText); }
                    catch { detected = null; }
                }

                if (detected != null && detected.Count > 1)
                {
                    // Show confirmation dialog with detected chapters
                    var confirm = new Views.ChapterConfirmationDialog();
                    confirm.Owner = Application.Current.MainWindow;
                    confirm.SetChapters(detected);
                    confirm.ShowDialog();

                    if (confirm.DialogResult == true && confirm.AcceptedAll)
                    {
                        ImportAsMultipleChapters(detected);
                    }
                    else if (confirm.DialogResult == true && confirm.ImportSingle)
                    {
                        ImportAsSingleChapter(fullText, suggestedName);
                    }
                    else
                    {
                        // User cancelled — treat as single import
                        ImportAsSingleChapter(fullText, suggestedName);
                    }
                }
                else
                {
                    ImportAsSingleChapter(fullText, suggestedName);
                }

                // Notify the tutorial that a project is now open so step 14
                // ("Complete the Dialog", RequiredAction = "projectopened") can advance.
                _tutorialActionSink?.Invoke("projectopened");
            }
            catch (Exception ex)
            {
                IsBusy = false;
                ShowError("Import Failed", ex.Message);
                _audio.Speak("Import failed. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Converts heuristic break points (paragraph indices) into DetectedChapters
        /// with the full inter-break text as each chapter's content.
        /// </summary>
        private static List<DetectedChapter> SplitByHeuristicBreaks(
            List<VoiceBookStudio.Models.ParagraphData> paragraphs,
            List<(string Title, int StartIndex)> breaks)
        {
            var result = new List<DetectedChapter>();
            for (int i = 0; i < breaks.Count; i++)
            {
                int start = breaks[i].StartIndex;
                int end   = i + 1 < breaks.Count ? breaks[i + 1].StartIndex : paragraphs.Count;
                // Skip the heading paragraph itself; join the body paragraphs as content.
                var bodyParas = paragraphs
                    .Skip(start + 1)
                    .Take(end - start - 1)
                    .Select(p => p.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t));
                result.Add(new DetectedChapter
                {
                    Title   = breaks[i].Title,
                    Content = string.Join("\n\n", bodyParas)
                });
            }
            return result;
        }

        private void ImportAsMultipleChapters(List<DetectedChapter> detected)
        {
            int created = 0;
            int total   = detected.Count;

            for (int i = 0; i < total; i++)
            {
                var ch = detected[i];

                string? title = PromptText(
                    $"Import Chapter {i + 1} of {total}",
                    "Claude suggests this title. Edit it or click OK to accept:",
                    ch.Title);

                if (title == null)
                {
                    // User pressed Cancel — offer to skip this chapter or stop entirely
                    var skip = MessageBox.Show(
                        $"Skip chapter {i + 1} of {total} and continue with the remaining chapters?",
                        "Import Document",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (skip == MessageBoxResult.Yes) continue;
                    break;
                }

                AddImportedChapter(title, ch.Content);
                created++;
            }

            string msg = created == 1
                ? "1 chapter imported from document."
                : $"{created} of {total} chapters imported from document.";
            SetStatus(msg);
            _systemAnnouncements.Speak(msg);
            if (created > 0) MarkDirty();
        }

        private void ImportAsSingleChapter(string content, string suggestedTitle)
        {
            string? title = PromptText(
                "Import Document",
                "Enter a title for the imported chapter:",
                suggestedTitle);
            if (title == null) return;

            AddImportedChapter(title, content);

            string msg = $"Document imported as chapter: {title}";
            SetStatus(msg);
            _systemAnnouncements.Speak(msg);
            MarkDirty();
        }

        private void AddImportedChapter(string title, string content)
        {
            var model = new BookChapter
            {
                Title     = title,
                Content   = content,
                SortOrder = Project!.Chapters.Count
            };
            Project.Chapters.Add(model);

            var cvm = new ChapterViewModel(model);
            cvm.LoadFromModel();
            Chapters.Add(cvm);
            SelectedChapter = cvm;
        }

        // ----------------------------------------------------------------
        // Chapter commands  (CanExecute wired to HasProject / HasSelectedChapter)
        // ----------------------------------------------------------------

        [RelayCommand(CanExecute = nameof(HasProject))]
        private void AddChapter()
        {
            // Notify tutorial that add-chapter flow is starting (step 4.3 detection).
            _tutorialActionSink?.Invoke("addchapter_started");

            // Step 1: pick section type
            var typeDlg = new SectionTypeDialog { Owner = Application.Current.MainWindow };
            if (typeDlg.ShowDialog() != true || typeDlg.SelectedType == null) return;

            SectionType sectionType = typeDlg.SelectedType.Value;
            string defaultTitle     = SectionTypeHelper.GetDefaultTitle(sectionType);

            // Step 2: confirm / edit title
            string? title = PromptText("Add Section", "Enter title:", defaultTitle);
            if (title == null) return;

            var chapter = new BookChapter
            {
                Title       = title,
                SectionType = sectionType,
                SortOrder   = Project!.Chapters.Count
            };
            Project.Chapters.Add(chapter);

            var cvm = new ChapterViewModel(chapter);
            cvm.LoadFromModel();

            // Insert at the correct display position (front → body → back)
            InsertSorted(cvm);
            SelectedChapter = cvm;
            Project.NormaliseSortOrder();
            MarkDirty();

            string label = SectionTypeHelper.GetDisplayName(sectionType);
            SetStatus($"Added: {title}  [{label}]");
            _systemAnnouncements.Speak($"Added {label}: {title}");
            _tutorialActionSink?.Invoke("addchapter");
        }

        [RelayCommand(CanExecute = nameof(CanModifyChapter))]
        private void ChangeChapterType()
        {
            if (SelectedChapter == null) return;

            var dlg = new SectionTypeDialog(SelectedChapter.SectionType)
            {
                Owner = Application.Current.MainWindow
            };
            if (dlg.ShowDialog() != true || dlg.SelectedType == null) return;

            SelectedChapter.SectionType = dlg.SelectedType.Value;
            SelectedChapter.FlushToModel();

            // Re-sort so the chapter moves to its new group position
            Chapters.Remove(SelectedChapter);
            InsertSorted(SelectedChapter);
            Project!.NormaliseSortOrder();
            MarkDirty();

            string label = SectionTypeHelper.GetDisplayName(dlg.SelectedType.Value);
            SetStatus($"{SelectedChapter.Title} → [{label}]");
            _systemAnnouncements.Speak($"Section type changed to {label}");
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        private async Task ExportDocxAsync()
        {
            if (Project == null) return;
            FlushEditorToChapter();
            foreach (var cvm in Chapters) cvm.FlushToModel();

            var dlg = new SaveFileDialog
            {
                Title      = "Export Manuscript as Word Document",
                Filter     = "Word Document (*.docx)|*.docx",
                DefaultExt = ".docx",
                FileName   = Project.Title
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                IsBusy = true;
                SetStatus("Exporting manuscript…");
                _audio.Speak("Exporting manuscript. Please wait.");

                var exporter  = new DocxExportService();
                var chapters  = Chapters
                    .Select(c => c.Model)
                    .OrderBy(c => SectionTypeHelper.GetGroupOrder(c.SectionType))
                    .ThenBy(c => (int)c.SectionType)
                    .ThenBy(c => c.SortOrder)
                    .ToList();

                await Task.Run(() => exporter.Export(Project, chapters, dlg.FileName));

                _sounds.Play(AppSound.ExportSuccess);
                string msg = $"Exported: {System.IO.Path.GetFileName(dlg.FileName)}";
                SetStatus(msg);
                _systemAnnouncements.Speak(msg);
            }
            catch (Exception ex)
            {
                _sounds.Play(AppSound.ExportError);
                ShowError("Export Failed", ex.Message);
                _audio.Speak("Export failed. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(HasProject))]
        private async Task ExportPdfAsync()
        {
            if (Project == null) return;
            FlushEditorToChapter();
            foreach (var cvm in Chapters) cvm.FlushToModel();

            var dlg = new SaveFileDialog
            {
                Title      = "Export Manuscript as PDF",
                Filter     = "PDF Document (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName   = Project.Title
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                IsBusy = true;
                SetStatus("Exporting PDF…");
                _audio.Speak("Exporting PDF. Please wait.");

                var exporter = new PdfExportService();
                var chapters = Chapters
                    .Select(c => c.Model)
                    .OrderBy(c => SectionTypeHelper.GetGroupOrder(c.SectionType))
                    .ThenBy(c => (int)c.SectionType)
                    .ThenBy(c => c.SortOrder)
                    .ToList();

                await Task.Run(() => exporter.Export(Project, chapters, dlg.FileName));

                _sounds.Play(AppSound.ExportSuccess);
                string msg = $"Exported: {System.IO.Path.GetFileName(dlg.FileName)}";
                SetStatus(msg);
                _systemAnnouncements.Speak(msg);
            }
            catch (Exception ex)
            {
                _sounds.Play(AppSound.ExportError);
                ShowError("Export Failed", ex.Message);
                _audio.Speak("Export failed. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifyChapter))]
        private void RenameChapter()
        {
            if (SelectedChapter == null) return;

            string? title = PromptText("Rename Chapter", "Enter new title:", SelectedChapter.Title);
            if (title == null || title == SelectedChapter.Title) return;

            string old = SelectedChapter.Title;
            SelectedChapter.Title = title;
            SelectedChapter.FlushToModel();
            MarkDirty();
            OnPropertyChanged(nameof(EditorTitle));

            _sounds.Play(AppSound.ChapterAdded);
            SetStatus($"Renamed: {old} → {title}");
            _systemAnnouncements.Speak("Chapter renamed");
        }

        [RelayCommand(CanExecute = nameof(CanModifyChapter))]
        private void DeleteChapter()
        {
            if (SelectedChapter == null || Project == null) return;

            string title = SelectedChapter.Title;
            var result   = MessageBox.Show(
                $"Delete chapter \"{title}\"? This cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            Project.Chapters.Remove(SelectedChapter.Model);
            Chapters.Remove(SelectedChapter);
            SelectedChapter = Chapters.FirstOrDefault();
            Project.NormaliseSortOrder();
            MarkDirty();

            _sounds.Play(AppSound.ChapterDeleted);
            SetStatus($"Chapter deleted: {title}");
            _systemAnnouncements.Speak("Chapter deleted");
        }

        [RelayCommand(CanExecute = nameof(CanModifyChapter))]
        private void MoveChapterUp()
        {
            if (SelectedChapter == null) return;
            int idx = Chapters.IndexOf(SelectedChapter);
            if (idx <= 0) return;

            Chapters.Move(idx, idx - 1);
            Project!.Chapters.RemoveAt(idx);
            Project.Chapters.Insert(idx - 1, SelectedChapter.Model);
            Project.NormaliseSortOrder();
            MarkDirty();

            _sounds.Play(AppSound.ChapterMoved);
            SetStatus($"Moved up: {SelectedChapter.Title}");
            _audio.Speak($"{SelectedChapter.Title} moved up.");
        }

        [RelayCommand(CanExecute = nameof(CanModifyChapter))]
        private void MoveChapterDown()
        {
            if (SelectedChapter == null) return;
            int idx = Chapters.IndexOf(SelectedChapter);
            if (idx < 0 || idx >= Chapters.Count - 1) return;

            Chapters.Move(idx, idx + 1);
            Project!.Chapters.RemoveAt(idx);
            Project.Chapters.Insert(idx + 1, SelectedChapter.Model);
            Project.NormaliseSortOrder();
            MarkDirty();

            _sounds.Play(AppSound.ChapterMoved);
            SetStatus($"Moved down: {SelectedChapter.Title}");
            _audio.Speak($"{SelectedChapter.Title} moved down.");
        }

        private bool CanModifyChapter() => SelectedChapter != null;

        // ----------------------------------------------------------------
        // Editor sync
        // ----------------------------------------------------------------

        public void OnChapterSelected(ChapterViewModel? chapter)
        {
            FlushEditorToChapter();
            IsWholeBookSelected = false;
            SelectedChapter = chapter;

            if (chapter != null)
            {
                SetStatus($"Editing: {chapter.Title}  |  {chapter.WordCount:N0} words");
                _audio.Speak($"Chapter loaded: {chapter.Title}. {chapter.WordCount} words.");
            }
        }

        /// <summary>
        /// Called by the view when the user selects the "Whole Book" list entry.
        /// Refreshes the concatenated manuscript and puts the editor in read-only mode.
        /// </summary>
        public void SelectWholeBook()
        {
            FlushEditorToChapter();
            IsWholeBookSelected = true;
            WholeBook.Refresh(Chapters);
            SelectedChapter = null;
            SetStatus("Whole Book — read-only view of all chapters in order.");
            _audio.Speak("Whole Book. Read only.");
        }

        public void OnEditorTextChanged(string newText)
        {
            if (SelectedChapter == null) return;

            bool contentChanged    = SelectedChapter.Content != newText;
            SelectedChapter.Content = newText;

            SelectedChapter.WordCount = string.IsNullOrWhiteSpace(newText)
                ? 0
                : newText.Split(new[] { ' ', '\t', '\n', '\r' },
                      System.StringSplitOptions.RemoveEmptyEntries).Length;

            OnPropertyChanged(nameof(WordCountDisplay));
            if (contentChanged) MarkDirty();
        }

        // ----------------------------------------------------------------
        // AI feedback commands
        // ----------------------------------------------------------------

        [RelayCommand(CanExecute = nameof(CanRunAi))]
        private async Task RunAiFeedbackAsync(string feedbackType)
        {
            if (SelectedChapter == null && !IsWholeBookSelected) return;

            if (!_aiService.IsAvailable)
            {
                string msg = "AI is not configured. Click the ⚠ AI Not Set button to add your API key.";
                AiFeedbackText = msg;
                SetStatus(msg);
                _audio.Speak(msg);
                return;
            }

            try
            {
                IsBusy = true;
                SetStatus($"Running {feedbackType} analysis…");
                _audio.Speak($"Running {feedbackType} analysis. Please wait.");

                string chapterForFeedback;
                AiFeedback feedback;

                if (IsWholeBookSelected)
                {
                    // Whole Book selected — use the full concatenated manuscript as context.
                    // Refresh first in case chapters were edited since the selection was made.
                    WholeBook.Refresh(Chapters);
                    feedback = await _aiService.GetFeedbackAsync(
                        WholeBook.Content, feedbackType, bookContext: null);
                    chapterForFeedback = "Whole Book";
                }
                else
                {
                    feedback = await _aiService.GetFeedbackAsync(
                        SelectedChapter!.Content, feedbackType, BuildBookContext());
                    chapterForFeedback = SelectedChapter.Title;
                }

                _sounds.Play(AppSound.AiResponded);
                AiFeedbackText = feedback.RawText;

                // Auto-save to feedback library (silent, no user action required)
                FeedbackLibVM.AddEntry(feedbackType, chapterForFeedback, feedback.RawText);

                SetStatus("AI analysis complete. Feedback saved to library. Use the Insert buttons to add it to your chapter.");
                _audio.Speak("Analysis complete. Feedback saved to library. Review the feedback panel.");
                _tutorialActionSink?.Invoke("feedback");
            }
            catch (Exception ex)
            {
                string detail = $"{ex.GetType().Name}: {ex.Message}";
                AiFeedbackText = $"Error: {detail}";
                ShowError("AI Error", detail);
                _audio.Speak("AI analysis failed. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRunAi() => SelectedChapter != null || IsWholeBookSelected;

        // ----------------------------------------------------------------
        // Book-wide feedback (all chapters as primary content)
        // ----------------------------------------------------------------

        [RelayCommand(CanExecute = nameof(CanRunBookFeedback))]
        private async Task RunBookFeedbackAsync()
        {
            if (Project == null) return;

            if (!_aiService.IsAvailable)
            {
                string msg = "AI is not configured. Click the ⚠ AI Not Set button to add your API key.";
                AiFeedbackText = msg;
                SetStatus(msg);
                _audio.Speak(msg);
                return;
            }

            string fullContent = BuildFullBookContent(out int wordCount);
            if (string.IsNullOrWhiteSpace(fullContent))
            {
                SetStatus("No content to analyse — add some writing to your chapters first.");
                return;
            }

            try
            {
                IsBusy = true;
                string msg = $"Analysing full manuscript ({wordCount:N0} words)…";
                SetStatus(msg);
                _audio.Speak("Analysing your full manuscript. This may take a moment.");

                var feedback = await _aiService.GetBookFeedbackAsync(fullContent, Project.Title);

                _sounds.Play(AppSound.AiResponded);
                AiFeedbackText = feedback.RawText;

                // Auto-save book-wide analysis to feedback library as category A (Comprehensive)
                FeedbackLibVM.AddEntry("book", Project.Title + " (full book)", feedback.RawText);

                SetStatus("Book analysis complete. Feedback saved to library.");
                _audio.Speak("Book analysis complete. Feedback saved to library.");
            }
            catch (Exception ex)
            {
                _sounds.Play(AppSound.AiError);
                string detail = $"{ex.GetType().Name}: {ex.Message}";
                AiFeedbackText = $"Error: {detail}";
                ShowError("AI Error", detail);
                _audio.Speak("Book analysis failed. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRunBookFeedback() => Project != null && Chapters.Any(c => !string.IsNullOrWhiteSpace(c.Content));

        /// <summary>
        /// Builds the full manuscript text (all sections in document order)
        /// for book-wide AI analysis. All chapters are primary content — none
        /// are treated as background context only.
        /// </summary>
        private string BuildFullBookContent(out int totalWordCount)
        {
            var sb = new System.Text.StringBuilder();
            totalWordCount = 0;

            foreach (var cvm in Chapters)
            {
                string typeName = SectionTypeHelper.GetDisplayName(cvm.SectionType);
                sb.AppendLine($"=== {typeName}: {cvm.Title} ===");
                sb.AppendLine();

                if (!string.IsNullOrWhiteSpace(cvm.Content))
                {
                    sb.AppendLine(cvm.Content.Trim());
                    totalWordCount += cvm.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                }
                else
                {
                    sb.AppendLine("[No content yet]");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // Chat command
        // ----------------------------------------------------------------

        [RelayCommand(CanExecute = nameof(CanRunAi))]
        private async Task SendChatAsync()
        {
            string msg = ChatInputText.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            if (!_aiService.IsAvailable)
            {
                string notice = "AI is not configured. Click the ⚠ AI Not Set button to add your API key.";
                AiFeedbackText = notice;
                _audio.Speak(notice);
                return;
            }

            try
            {
                IsBusy        = true;
                ChatInputText = string.Empty;
                SetStatus("Asking Claude…");
                _audio.Speak("Sending your question to Claude. Please wait.");

                if (IsWholeBookSelected) WholeBook.Refresh(Chapters);
                string? chatContext = IsWholeBookSelected ? WholeBook.Content : SelectedChapter?.Content;
                string response = await _aiService.ChatAsync(
                    msg, chatContext, IsWholeBookSelected ? null : BuildBookContext());

                _sounds.Play(AppSound.AiResponded);
                AiFeedbackText = response;
                SetStatus("Claude responded. Use Insert buttons to add text to your chapter.");
                _audio.Speak("Claude responded. Review the response, then use Insert buttons to add it to your chapter, or say Insert at cursor.");
                _tutorialActionSink?.Invoke("sendchat");
                FocusChatInputRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                AiFeedbackText = $"Error: {ex.Message}";
                ShowError("Chat Error", ex.Message);
                _audio.Speak("Chat failed. " + ex.Message);
                FocusChatInputRequested?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ----------------------------------------------------------------
        // Insert AI response into chapter (3 positions)
        // ----------------------------------------------------------------

        [RelayCommand(CanExecute = nameof(CanInsert))]
        private void InsertAtCursor()
        {
            if (string.IsNullOrWhiteSpace(AiFeedbackText)) return;
            InsertTextRequested?.Invoke(this,
                new InsertTextArgs(AiFeedbackText, InsertPosition.AtCursor));

            _sounds.Play(AppSound.TextInserted);
            SetStatus("AI response inserted at cursor position.");
            _audio.Speak("Inserted at cursor position.");
        }

        [RelayCommand(CanExecute = nameof(CanInsert))]
        private void InsertAtStart()
        {
            if (string.IsNullOrWhiteSpace(AiFeedbackText)) return;
            InsertTextRequested?.Invoke(this,
                new InsertTextArgs(AiFeedbackText, InsertPosition.AtBeginning));

            _sounds.Play(AppSound.TextInserted);
            SetStatus("AI response inserted at beginning of chapter.");
            _audio.Speak("Inserted at beginning of chapter.");
        }

        [RelayCommand(CanExecute = nameof(CanInsert))]
        private void InsertAtEnd()
        {
            if (string.IsNullOrWhiteSpace(AiFeedbackText)) return;
            InsertTextRequested?.Invoke(this,
                new InsertTextArgs(AiFeedbackText, InsertPosition.AtEnd));

            _sounds.Play(AppSound.TextInserted);
            SetStatus("AI response inserted at end of chapter.");
            _audio.Speak("Inserted at end of chapter.");
        }

        private bool CanInsert() =>
            SelectedChapter != null &&
            !string.IsNullOrWhiteSpace(AiFeedbackText) &&
            AiFeedbackText != "Select a chapter and run an AI analysis to see feedback here.";

        // ----------------------------------------------------------------
        // Save as Card command
        // ----------------------------------------------------------------

        [RelayCommand(CanExecute = nameof(HasAiResponse))]
        private void SaveAsCard()
        {
            string defaultTitle = AiFeedbackText.Length > 40
                ? AiFeedbackText[..40].TrimEnd() + "…"
                : AiFeedbackText.Trim();

            var existingCategories = ResponseCardVM.GetExistingCategoryNames();
            var dlg = new Views.SaveCardDialog(existingCategories, defaultTitle)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            if (dlg.ShowDialog() != true) return;

            var card = new VoiceBookStudio.Models.ResponseCard
            {
                Title    = dlg.CardTitle,
                Category = dlg.CategoryName,
                Content  = AiFeedbackText
            };
            ResponseCardVM.AddCard(card);
            SwitchAiTabRequested?.Invoke(this, "Cards");
            SetStatus($"Card saved: {dlg.CardTitle}");
        }

        // ----------------------------------------------------------------
        // Insert card at cursor (called by ResponseCardVM.InsertCardRequested)
        // ----------------------------------------------------------------

        public void InsertCardAtCursor(VoiceBookStudio.Models.ResponseCard card)
        {
            InsertTextRequested?.Invoke(this,
                new InsertTextArgs(card.Content, InsertPosition.AtCursor));

            string msg = $"Card inserted: {card.Title}";
            SetStatus(msg);
            _systemAnnouncements.Speak(msg);
        }

        // ----------------------------------------------------------------
        // API key command
        // ----------------------------------------------------------------

        [RelayCommand]
        private void SetApiKey()
        {
            var dialog = new ApiKeyDialog { Owner = Application.Current.MainWindow };
            dialog.ShowDialog();
            IsApiKeySet = ApiKeyService.HasApiKey();

            string status = IsApiKeySet
                ? "AI key saved. AI features are now active."
                : "API key removed. AI features disabled.";

            SetStatus(status);
            _audio.Speak(status);
        }

        // ----------------------------------------------------------------
        // Settings commands
        // ----------------------------------------------------------------

        [RelayCommand]
        private void OpenSettings()
        {
            var dlg = new Views.SettingsDialog(_systemAnnouncements)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            dlg.ShowDialog();

            string msg = string.IsNullOrWhiteSpace(VoiceBookStudio.Utils.AppSettings.DefaultProjectFolder)
                ? "Settings saved. No default project folder set."
                : $"Settings saved. Default folder: {VoiceBookStudio.Utils.AppSettings.DefaultProjectFolder}";
            SetStatus(msg);
        }

        /// <summary>Opens the folder picker directly (voice command: "set project folder").</summary>
        public void TryOpenDefaultFolderPicker()
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description         = "Choose the default folder where new VoiceBook projects will be saved",
                ShowNewFolderButton = true,
                SelectedPath        = VoiceBookStudio.Utils.AppSettings.DefaultProjectFolder
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                VoiceBookStudio.Utils.AppSettings.DefaultProjectFolder = dlg.SelectedPath;
                VoiceBookStudio.Utils.AppSettings.SaveJsonSettings();
                string msg = $"Default project folder set to: {dlg.SelectedPath}";
                SetStatus(msg);
                _systemAnnouncements.Speak(msg);
            }
        }

        public void TryOpenSettings()
        {
            OpenSettingsCommand.Execute(null);
        }

        [RelayCommand]
        private void ConfigureVoice()
        {
            var dialog = new Views.AzureTtsDialog(_audio, _systemAnnouncements)
            {
                Owner = Application.Current.MainWindow
            };
            dialog.ShowDialog();

            string status = VoiceBookStudio.Utils.AppSettings.IsAzureTtsConfigured
                ? $"Azure Neural voice active: {VoiceBookStudio.Utils.AppSettings.AzureVoiceName}"
                : "Using Windows SAPI voice.";
            SetStatus(status);
        }

        /// <summary>
        /// Fired when the user requests the microphone to be toggled on or off.
        /// The bool argument is the <em>desired</em> new state (true = start listening).
        /// MainWindow handles this event, attempts to start/stop the recogniser,
        /// and calls <see cref="SetMicListening"/> with the actual result.
        /// </summary>
        public event EventHandler<bool>? MicToggleRequested;

        [RelayCommand]
        private void ToggleMic()
        {
            bool desired = !IsMicListening;
            MicToggleRequested?.Invoke(this, desired);
        }

        // True while the startup sequence is enabling the mic — step 4 already
        // announced the mic state so SetMicListening should not speak again.
        private bool _startupMicToggle;

        /// <summary>Called by MainWindow after the mic service reports its actual state.</summary>
        public void SetMicListening(bool isListening, string? errorMessage = null)
        {
            IsMicListening = isListening;
            // During startup, suppress the redundant "Microphone on" announcement —
            // step 4 already said "VoiceBook microphone on". Always report errors.
            if (_startupMicToggle && isListening && errorMessage == null) return;
            string msg = errorMessage
                ?? (isListening
                    ? "Microphone on. Say a command to control the app."
                    : "Microphone off.");
            SetStatus(msg);
            _audio.Speak(msg);
        }

        [RelayCommand]
        private void ToggleAppTts()
        {
            AppTtsEnabled = !AppTtsEnabled;

            if (!AppTtsEnabled)
            {
                // Temporarily re-enable to speak the "off" announcement, then silence.
                _audio.IsEnabled = true;
                _audio.Speak("App voice off. JAWS will handle all audio.");
                _audio.IsEnabled = false;
            }
            else
            {
                _audio.IsEnabled = true;
                _audio.Speak("App voice on.");
            }

            SetStatus(AppTtsStatusDisplay);
        }

        // ----------------------------------------------------------------
        // Internal helpers
        // ----------------------------------------------------------------

        private void OnProjectFolderChanged(string projectFolderPath)
        {
            _responseCardService.SetProjectFolder(projectFolderPath);
            ResponseCardVM.Reload();
            _feedbackLibraryService.SetProjectFolder(projectFolderPath);
            FeedbackLibVM.Reload();
        }

        private void LoadChapters()
        {
            Chapters.Clear();
            if (Project == null) return;

            var sorted = Project.Chapters
                .OrderBy(c => SectionTypeHelper.GetGroupOrder(c.SectionType))
                .ThenBy(c => (int)c.SectionType)
                .ThenBy(c => c.SortOrder);

            foreach (var ch in sorted)
            {
                var cvm = new ChapterViewModel(ch);
                cvm.LoadFromModel();
                Chapters.Add(cvm);
            }
        }

        /// <summary>
        /// Inserts <paramref name="cvm"/> into the Chapters list at the correct
        /// position for its section group (front matter → body → back matter).
        /// Within the same group, new items are appended after existing ones.
        /// </summary>
        private void InsertSorted(ChapterViewModel cvm)
        {
            int group = SectionTypeHelper.GetGroupOrder(cvm.SectionType);

            // Find the first item whose group order is greater than ours.
            for (int i = 0; i < Chapters.Count; i++)
            {
                int existingGroup = SectionTypeHelper.GetGroupOrder(Chapters[i].SectionType);
                if (existingGroup > group)
                {
                    Chapters.Insert(i, cvm);
                    return;
                }
            }

            // No later group found — append at end.
            Chapters.Add(cvm);
        }

        private void SelectFirstChapter()
        {
            IsWholeBookSelected = false;
            SelectedChapter = Chapters.FirstOrDefault();
            if (SelectedChapter != null)
                _audio.Speak($"First chapter: {SelectedChapter.Title}.");
        }

        /// <summary>
        /// Rebuilds DisplayItems from scratch: WholeBook sentinel first, then all
        /// real chapters in their current sorted order.
        /// </summary>
        private void RebuildDisplayItems()
        {
            DisplayItems.Clear();
            DisplayItems.Add(WholeBook);
            foreach (var c in Chapters)
                DisplayItems.Add(c);
        }

        private void FlushEditorToChapter() => SelectedChapter?.FlushToModel();

        private void MarkDirty() => IsModified = true;

        private void SetStatus(string message) => StatusMessage = message;

        // ----------------------------------------------------------------
        // Auto-save
        // ----------------------------------------------------------------

        private async void AutoSave_Tick(object? sender, EventArgs e)
        {
            if (!IsModified || Project == null || string.IsNullOrEmpty(_currentFilePath))
                return;

            try
            {
                FlushEditorToChapter();
                foreach (var cvm in Chapters) cvm.FlushToModel();
                await _projectService.SaveAsync(Project, _currentFilePath);
                IsModified = false;
                _sounds.Play(AppSound.AutoSaved);
                SetStatus($"Auto-saved: {System.IO.Path.GetFileName(_currentFilePath)}");
            }
            catch
            {
                // Auto-save is best-effort — silently skip on failure so the
                // user isn't interrupted mid-dictation by an error dialog.
            }
        }

        private static void ShowError(string title, string message) =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        /// <summary>Converts a project title to a safe directory / file name.</summary>
        private static string SanitizeFileName(string name)
        {
            char[] invalid = System.IO.Path.GetInvalidFileNameChars();
            var sb = new System.Text.StringBuilder();
            foreach (char c in name)
                sb.Append(invalid.Contains(c) || c == ' ' ? '_' : c);
            return sb.ToString().Trim('_');
        }

        /// <summary>
        /// Builds a formatted book overview sent to Claude on every AI call.
        /// Includes all section titles and a ~300-word preview of each section's
        /// content so Claude understands characters, themes, and continuity.
        /// The current chapter is excluded — it is sent separately as the focus.
        /// </summary>
        private string? BuildBookContext()
        {
            if (Project == null || Chapters.Count <= 1) return null;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Book title: {Project.Title}");
            sb.AppendLine($"Total sections: {Chapters.Count}");
            sb.AppendLine();

            foreach (var cvm in Chapters)
            {
                // Skip the chapter currently open in the editor — it is sent as the focus
                if (cvm == SelectedChapter) continue;

                string group   = SectionTypeHelper.GetGroup(cvm.SectionType);
                string typeName = SectionTypeHelper.GetDisplayName(cvm.SectionType);
                sb.AppendLine($"[{group}] {typeName}: {cvm.Title}");

                if (!string.IsNullOrWhiteSpace(cvm.Content))
                {
                    // Send up to ~300 words per section to stay well within context limits
                    string[] words   = cvm.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string   preview = words.Length > 300
                        ? string.Join(' ', words[..300]) + " [...]"
                        : cvm.Content.Trim();
                    sb.AppendLine(preview);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private async Task<bool> ConfirmDiscardChangesAsync()
        {
            if (!IsModified) return true;

            var result = MessageBox.Show(
                "You have unsaved changes. Discard them?",
                "Unsaved Changes",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return await Task.FromResult(result == MessageBoxResult.Yes);
        }

        private static string? PromptText(string title, string prompt, string defaultValue = "")
        {
            var dialog = new InputDialog(title, prompt, defaultValue);
            return dialog.ShowDialog() == true ? dialog.InputValue : null;
        }

        // ----------------------------------------------------------------
        // Voice router hooks — thin delegates to existing RelayCommands.
        // Called by VoiceCommandRouter so Dragon / JSay / custom speech
        // front-ends can trigger any action without touching UI directly.
        // ----------------------------------------------------------------

        public void TrySaveProject()
        {
            if (SaveProjectCommand.CanExecute(null))
                SaveProjectCommand.Execute(null);
        }

        public void TrySaveProjectAs()
        {
            if (SaveProjectAsCommand.CanExecute(null))
                SaveProjectAsCommand.Execute(null);
        }

        public void TryAddChapter()
        {
            if (AddChapterCommand.CanExecute(null))
                AddChapterCommand.Execute(null);
        }

        public void TryDeleteChapter()
        {
            if (DeleteChapterCommand.CanExecute(null))
                DeleteChapterCommand.Execute(null);
        }

        public void TryRenameChapter()
        {
            if (RenameChapterCommand.CanExecute(null))
                RenameChapterCommand.Execute(null);
        }

        public void TryMoveChapterUp()
        {
            if (MoveChapterUpCommand.CanExecute(null))
                MoveChapterUpCommand.Execute(null);
        }

        public void TryMoveChapterDown()
        {
            if (MoveChapterDownCommand.CanExecute(null))
                MoveChapterDownCommand.Execute(null);
        }

        public void TryChangeChapterType()
        {
            if (ChangeChapterTypeCommand.CanExecute(null))
                ChangeChapterTypeCommand.Execute(null);
        }

        public void TryRunAiFeedback(string feedbackType)
        {
            if (RunAiFeedbackCommand.CanExecute(feedbackType))
                RunAiFeedbackCommand.Execute(feedbackType);
        }

        public void TryRunBookFeedback()
        {
            if (RunBookFeedbackCommand.CanExecute(null))
                RunBookFeedbackCommand.Execute(null);
        }

        public void TryImportDocument()
        {
            // Signal tutorial that the user chose the import path (step 5.2 detection).
            _tutorialActionSink?.Invoke("newproject_or_import");
            if (ImportDocumentCommand.CanExecute(null))
                ImportDocumentCommand.Execute(null);
        }

        /// <summary>
        /// Announces the commands available in the current panel context.
        /// Called by "what can I say here" / "list commands" voice commands.
        /// </summary>
        public void AnnounceContextualHelp()
        {
            string help = _currentPanel switch
            {
                1 => "Chapter Manager commands. " +
                     "Add chapter. Rename chapter. Delete chapter. Change type. " +
                     "To navigate the list: Next chapter. Previous chapter. " +
                     "To reorder: Move up. Move down. " +
                     "Keyboard shortcuts: Control A to add. Control D to rename. Control Delete to delete. " +
                     "Alt Up or Alt Down to reorder. F6 or F7 to move selection. " +
                     "Global commands: Save. New project. Export Word. Export PDF. " +
                     "Panel one, two, or three to switch panels.",

                2 => "Writing Editor. Dictate or type your chapter here. " +
                     "Dragon NaturallySpeaking works the same as in Word — dictate, correct, and navigate naturally. " +
                     "Say Press Escape or F1 to leave the editor. " +
                     "Say Press F3 to go to the AI assistant. " +
                     "To give voice commands: go to the AI panel, type in the chat box, and press Enter. " +
                     "Keyboard shortcuts: Control S to save. Control F for AI feedback. " +
                     "Global commands: Save. Feedback. Pacing. Dialogue. Style. Structure. Book analysis.",

                3 => "AI Assistant panel commands. " +
                     "Analyse full book. Comprehensive feedback. Pacing feedback. Dialogue feedback. " +
                     "Style feedback. Structure feedback. " +
                     "Send, or Send message, to send the chat. " +
                     "Insert at cursor. Insert at start. Insert at end. " +
                     "Save response card, or Save card. " +
                     "Open prompt library. Open response cards. " +
                     "Type any command in the chat box and press Enter. Examples: " +
                     "Add chapter. Save project. Rename chapter. Export PDF. Panel one. " +
                     "Or type any question for Claude and press Enter.",

                _ => "Global commands: Panel one. Panel two. Panel three. " +
                     "Save. New project. Open project. Import document. Export Word. Export PDF. " +
                     "Feedback. Book analysis. Toggle voice. Set API key. Start tutorial."
            };

            string globalExtra = " Say What can I say here at any time to hear this help again.";
            string full = help + globalExtra;

            SetStatus(full);
            if (!VoiceBookStudio.Utils.AppSettings.IsJawsDetected)
                _systemAnnouncements.Speak(full);
        }

        public void TrySendChat()
        {
            if (SendChatCommand.CanExecute(null))
                SendChatCommand.Execute(null);
        }

        public void TryInsertAtCursor()
        {
            if (InsertAtCursorCommand.CanExecute(null))
                InsertAtCursorCommand.Execute(null);
        }

        public void TryInsertAtStart()
        {
            if (InsertAtStartCommand.CanExecute(null))
                InsertAtStartCommand.Execute(null);
        }

        public void TryInsertAtEnd()
        {
            if (InsertAtEndCommand.CanExecute(null))
                InsertAtEndCommand.Execute(null);
        }

        public void TryExportDocx()
        {
            if (ExportDocxCommand.CanExecute(null))
                ExportDocxCommand.Execute(null);
        }

        public void TryExportPdf()
        {
            if (ExportPdfCommand.CanExecute(null))
                ExportPdfCommand.Execute(null);
        }

        public void TryOpenChat()
        {
            SwitchAiTabRequested?.Invoke(this, "Chat");
            FocusPanel3();
            SetStatus("Chat tab opened.");
            _audio.Speak("Chat tab.");
        }


        public void TryToggleAppTts()
        {
            ToggleAppTtsCommand.Execute(null);
        }

        public void TrySetApiKey()
        {
            SetApiKeyCommand.Execute(null);
        }

        // ----------------------------------------------------------------
        // Chapter navigation (voice commands: next chapter / previous chapter)
        // ----------------------------------------------------------------

        /// <summary>The panel number currently in focus (1=Chapters, 2=Editor, 3=AI).</summary>
        public int CurrentPanel => _currentPanel;

        public void TrySelectNextChapter()
        {
            if (Chapters.Count == 0) { AnnounceNotAvailable("No chapters in this project."); return; }
            int idx  = SelectedChapter == null ? -1 : Chapters.IndexOf(SelectedChapter);
            int next = (idx + 1) % Chapters.Count;
            SelectedChapter = Chapters[next];
            FocusPanelRequested?.Invoke(this, 1);
            string msg = $"Chapter: {SelectedChapter.Title}";
            SetStatus(msg);
            _audio.Speak(msg);
        }

        public void TrySelectPreviousChapter()
        {
            if (Chapters.Count == 0) { AnnounceNotAvailable("No chapters in this project."); return; }
            int idx  = SelectedChapter == null ? 0 : Chapters.IndexOf(SelectedChapter);
            int prev = (idx - 1 + Chapters.Count) % Chapters.Count;
            SelectedChapter = Chapters[prev];
            FocusPanelRequested?.Invoke(this, 1);
            string msg = $"Chapter: {SelectedChapter.Title}";
            SetStatus(msg);
            _audio.Speak(msg);
        }

        /// <summary>
        /// Focuses the chapter list and announces available chapter names so the user
        /// can say "click [chapter name]" via Dragon to select one.
        /// </summary>
        public void TryOpenChapter()
        {
            FocusPanelRequested?.Invoke(this, 1);
            _currentPanel = 1;
            if (Chapters.Count == 0)
            {
                string empty = "No chapters in this project. Say new chapter to create one.";
                SetStatus(empty);
                _systemAnnouncements.Speak(empty);
                return;
            }
            string list = string.Join(", ", Chapters.Select(c => c.Title));
            string msg  = $"Chapter list is focused. Available chapters: {list}. " +
                          "Say click followed by the chapter name to open it.";
            SetStatus(msg);
            _systemAnnouncements.Speak(msg);
        }

        // ----------------------------------------------------------------
        // Editor reading
        // ----------------------------------------------------------------

        public void TryAnnounceChapterTitle()
        {
            if (!HasSelectedChapter) { AnnounceNotAvailable("No chapter is open."); return; }
            string msg = $"Current chapter: {SelectedChapter!.Title}. {SelectedChapter.WordCount} words.";
            SetStatus(msg);
            _systemAnnouncements.Speak(msg);
        }

        /// <summary>
        /// Raised when VoiceCommandRouter receives "read paragraph".
        /// MainWindow handles this event by extracting the paragraph at the
        /// current editor cursor position and speaking it via SpeakViaAnnouncer.
        /// </summary>
        public event EventHandler? ReadParagraphAtCursorRequested;

        public void TryReadParagraph()
        {
            if (!HasSelectedChapter) { AnnounceNotAvailable("No chapter is open."); return; }
            ReadParagraphAtCursorRequested?.Invoke(this, EventArgs.Empty);
        }

        public void TryReadChapter()
        {
            if (!HasSelectedChapter) { AnnounceNotAvailable("No chapter is open."); return; }

            string content = SelectedChapter!.Content ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content))
            {
                _systemAnnouncements.Speak("The current chapter has no content.");
                return;
            }

            var paras = content
                .Split(new[] { "\r\n\r\n", "\n\n", "\r\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            if (paras.Count == 0) paras = new List<string> { content.Trim() };

            paras.Insert(0, $"Reading chapter: {SelectedChapter.Title}.");
            StartLibraryReading(paras);

            string status = $"Reading chapter: {SelectedChapter.Title}.";
            SetStatus(status);
        }

        public void TryStopReading()
        {
            StopLibraryReading();
            SetStatus("Reading stopped.");
            _audio.Speak("Stopped.");
        }

        // ----------------------------------------------------------------
        // AI assistant helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Sets the chat input to <paramref name="text"/>, focuses the AI panel,
        /// and submits the message to Claude.
        /// </summary>
        public void TryAskAssistant(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            ChatInputText = text;
            SwitchAiTabRequested?.Invoke(this, "Chat");
            FocusPanelRequested?.Invoke(this, 3);
            _currentPanel = 3;
            if (SendChatCommand.CanExecute(null))
                SendChatCommand.Execute(null);
        }

        public void TryReadAiResponse()
        {
            if (!HasAiResponse)
            {
                AnnounceNotAvailable("No AI response to read.");
                return;
            }

            var parts = AiFeedbackText
                .Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            if (parts.Count == 0) parts = new List<string> { AiFeedbackText };

            SwitchAiTabRequested?.Invoke(this, "Chat");
            StartLibraryReading(parts);
            SetStatus("Reading AI response.");
        }

        public void TryDiscardResponse()
        {
            AiFeedbackText = "Select a chapter and run an AI analysis to see feedback here.";
            string msg = "AI response discarded.";
            SetStatus(msg);
            _audio.Speak(msg);
        }

        // ----------------------------------------------------------------
        // Application status and lifecycle
        // ----------------------------------------------------------------

        public void TryAnnounceApplicationStatus()
        {
            var parts = new System.Collections.Generic.List<string>();

            parts.Add(Project == null
                ? "No project is open."
                : $"Project: {Project.Title}. {Chapters.Count} chapters.");

            if (SelectedChapter != null)
                parts.Add($"Current chapter: {SelectedChapter.Title}. {SelectedChapter.WordCount} words.");

            parts.Add(IsModified ? "There are unsaved changes." : "All changes saved.");

            if (VoiceBookStudio.Utils.AppSettings.IsJawsDetected)   parts.Add("JAWS is running.");
            if (VoiceBookStudio.Utils.AppSettings.IsDragonRunning)  parts.Add("Dragon is running.");
            if (VoiceBookStudio.Utils.AppSettings.IsJSayDetected)   parts.Add("J-Say is running.");

            parts.Add(IsApiKeySet ? "AI is active." : "AI key is not set.");

            string status = string.Join(" ", parts);
            SetStatus(status);
            _systemAnnouncements.Speak(status);
        }

        /// <summary>
        /// Raised when VoiceCommandRouter receives "close application".
        /// MainWindow handles this by calling Close().
        /// </summary>
        public event EventHandler? CloseApplicationRequested;

        public void TryCloseApplication()
        {
            CloseApplicationRequested?.Invoke(this, EventArgs.Empty);
        }

        // ----------------------------------------------------------------
        // Shared helpers for voice command feedback
        // ----------------------------------------------------------------

        /// <summary>
        /// Speaks "not available" feedback.  Called when a voice command is received
        /// but its preconditions are not met (no project open, wrong panel, etc.).
        /// Sets the status bar (JAWS live region) and speaks via AudioFeedbackService.
        /// </summary>
        public void AnnounceNotAvailable(string reason = "")
        {
            string detail = string.IsNullOrEmpty(reason) ? string.Empty : " " + reason;
            string msg    = "That command is not available right now." + detail;
            SetStatus(msg);
            _audio.Speak("That command is not available right now.");
        }

        /// <summary>Speaks text through SystemAnnouncementService (for MainWindow event handlers).</summary>
        public void SpeakViaAnnouncer(string text) => _systemAnnouncements.Speak(text);

        /// <summary>
        /// Speaks a synchronous goodbye message when the app is closing.
        /// Called from MainWindow.Closing — blocks until the utterance is done
        /// so the user hears the full message before the process exits.
        /// </summary>
        public void SpeakGoodbye() =>
            _systemAnnouncements.SpeakSync("VoiceBook Studio closing. Goodbye.");

        /// <summary>
        /// Called by VoiceCommandRouter when a spoken phrase doesn't match any command.
        /// Provides audio feedback so the user knows the command wasn't understood.
        /// </summary>
        public void OnCommandNotRecognized(string rawCommand)
        {
            SetStatus($"Command not recognized: \"{rawCommand}\". Say a voice command or type a message.");
            _audio.Speak("Command not recognized. Try saying Panel 1, Panel 2, or Panel 3 to navigate, or ask Claude a question.");
        }
    }
}
