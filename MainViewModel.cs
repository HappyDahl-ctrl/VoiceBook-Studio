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
        private readonly ProjectService      _projectService;
        private readonly AudioFeedbackService _audio;
        private readonly SystemAnnouncementService _systemAnnouncements;
        private readonly AiService           _aiService;

        private string? _currentFilePath;

        private readonly DispatcherTimer _autoSaveTimer;

        // ----------------------------------------------------------------
        // Sub-ViewModels (Prompts tab and Cards tab)
        // ----------------------------------------------------------------

        /// <summary>Bound to the Prompts tab in the AI panel.</summary>
        public PromptLibraryViewModel PromptLibVM { get; }

        /// <summary>Bound to the Cards tab in the AI panel.</summary>
        public ResponseCardViewModel ResponseCardVM { get; }

        // ----------------------------------------------------------------
        // Constructor
        // ----------------------------------------------------------------

        public MainViewModel(ProjectService      projectService,
                             AudioFeedbackService audio,
                             AiService           aiService,
                             SystemAnnouncementService systemAnnouncements)
        {
            _projectService = projectService;
            _audio          = audio;
            _systemAnnouncements = systemAnnouncements;
            _aiService      = aiService;

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
            ResponseCardVM = new ResponseCardViewModel(
                new VoiceBookStudio.Services.ResponseCardService(),
                _systemAnnouncements);

            // When a card is inserted, route it to the editor via InsertTextRequested.
            ResponseCardVM.InsertCardRequested += (_, card) => InsertCardAtCursor(card);

            // Auto-save every 30 seconds — only fires when there are unsaved changes.
            _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _autoSaveTimer.Tick += AutoSave_Tick;
            _autoSaveTimer.Start();
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

        public string JawsStatusDisplay =>
            VoiceBookStudio.Utils.AppSettings.IsJawsDetected ? "JAWS: Running" : "JAWS: Not detected";

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

        private TutorialViewModel? _tutorial;
        private ProjectSelectionViewModel? _projectSelection;

        // ----------------------------------------------------------------
        // Panel focus — called by VoiceCommandRouter and keyboard shortcuts
        // ----------------------------------------------------------------

        /// <summary>Say "Panel 1" or "Go to panel 1" to move focus here.</summary>
        public void FocusPanel1()
        {
            FocusPanelRequested?.Invoke(this, 1);
            SetStatus("Chapter Manager panel focused.");
            _audio.Speak("Chapters panel.");
        }

        /// <summary>Say "Panel 2" or "Go to panel 2" to move focus here.</summary>
        public void FocusPanel2()
        {
            FocusPanelRequested?.Invoke(this, 2);
            SetStatus("Writing Editor panel focused.");
            _audio.Speak("Editor panel.");
        }

        /// <summary>Say "Panel 3" or "Go to panel 3" to move focus here.</summary>
        public void FocusPanel3()
        {
            FocusPanelRequested?.Invoke(this, 3);
            SetStatus("AI Assistant panel focused.");
            _audio.Speak("AI Assistant panel.");
        }

        // Methods invoked by VoiceCommandRouter for tutorial navigation.
        public void TryExecuteTutorialNext() => _tutorial?.Next();
        public void TryExecuteTutorialPrevious() => _tutorial?.Previous();
        public void TryExecuteTutorialRepeat() => _tutorial?.Repeat();
        public void TryExecuteTutorialExit() => _tutorial?.Exit();
        public void TryExecuteStartTutorial() => StartTutorial();

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

        // ----------------------------------------------------------------
        // Response cards — voice router hooks
        // ----------------------------------------------------------------

        public void TryOpenResponseCards()
        {
            SwitchAiTabRequested?.Invoke(this, "Cards");
            SetStatus("Response cards tab opened.");
            _audio.Speak("Response cards tab.");
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
            // Invoke new project flow
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

                SetStatus($"Opened: {Project.Title}");
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

        // ----------------------------------------------------------------
        // Startup
        // ----------------------------------------------------------------

        public async Task InitialiseAsync()
        {
            IsApiKeySet = ApiKeyService.HasApiKey();

            // Fire-and-forget announcement — never block on SAPI at startup
            // (SAPI can hang if JAWS or audio drivers are initialising simultaneously)
            _systemAnnouncements.Speak("VoiceBook Studio ready");

            // Yield to let the main window fully render before showing any dialog
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(
                () => { }, System.Windows.Threading.DispatcherPriority.Background);

            // Show welcome dialog if configured to do so
            if (VoiceBookStudio.Utils.AppSettings.ShowWelcomeOnStartup && !IsBusy)
            {
                var dlg = new Views.WelcomeDialog
                {
                    DataContext = new WelcomeDialogViewModel(_systemAnnouncements),
                    Owner       = System.Windows.Application.Current.MainWindow
                };

                dlg.ShowDialog();

                if (dlg.DialogResult == true)
                {
                    StartTutorial();
                }
            }

            string aiMsg = IsApiKeySet
                ? "AI assistant is active."
                : "AI not configured. Click the AI Not Set button to add your Anthropic API key.";

            SetStatus($"Ready. {aiMsg}");
        }

        private void StartTutorial()
        {
            _tutorial = new TutorialViewModel(_systemAnnouncements, _audio);

            var dlg = new Views.TutorialDialog
            {
                DataContext = _tutorial,
                Owner       = System.Windows.Application.Current.MainWindow
            };

            _tutorial.TutorialCompleted += () =>
            {
                VoiceBookStudio.Utils.AppSettings.TutorialCompleted = true;
                VoiceBookStudio.Utils.AppSettings.Save();
                _systemAnnouncements.Speak("Tutorial complete. You're ready to write!");
                System.Windows.Application.Current.Dispatcher.Invoke(() => dlg.Close());
            };

            dlg.ShowDialog();
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

            SetStatus($"New project created: {title}");
            _systemAnnouncements.Speak($"Project opened: {title}");
        }

        [RelayCommand]
        private async Task OpenProjectAsync()
        {
            if (!await ConfirmDiscardChangesAsync()) return;

            var dialog = new OpenFileDialog
            {
                Title      = "Open VoiceBook Project",
                Filter     = ProjectService.FileFilter,
                DefaultExt = ProjectService.FileExtension
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
                Title      = "Save VoiceBook Project",
                Filter     = ProjectService.FileFilter,
                DefaultExt = ProjectService.FileExtension,
                FileName   = Project.Title
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

                SetStatus($"Saved: {System.IO.Path.GetFileName(path)}");
                _systemAnnouncements.Speak("Project saved");
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

                IsBusy = false;

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

                string msg = $"Exported: {System.IO.Path.GetFileName(dlg.FileName)}";
                SetStatus(msg);
                _systemAnnouncements.Speak(msg);
            }
            catch (Exception ex)
            {
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

                string msg = $"Exported: {System.IO.Path.GetFileName(dlg.FileName)}";
                SetStatus(msg);
                _systemAnnouncements.Speak(msg);
            }
            catch (Exception ex)
            {
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
            SelectedChapter = chapter;

            if (chapter != null)
            {
                SetStatus($"Editing: {chapter.Title}  |  {chapter.WordCount:N0} words");
                _audio.Speak($"Chapter loaded: {chapter.Title}. {chapter.WordCount} words.");
            }
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
            if (SelectedChapter == null) return;

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

                var feedback = await _aiService.GetFeedbackAsync(
                    SelectedChapter.Content, feedbackType);

                AiFeedbackText = feedback.RawText;
                SetStatus("AI analysis complete. Use the Insert buttons to add it to your chapter.");
                _audio.Speak("Analysis complete. Review the feedback panel. Use Insert buttons to add it to your chapter.");
            }
            catch (Exception ex)
            {
                AiFeedbackText = $"Error: {ex.Message}";
                ShowError("AI Error", ex.Message);
                _audio.Speak("AI analysis failed. " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRunAi() => SelectedChapter != null;

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

                string response = await _aiService.ChatAsync(
                    msg, SelectedChapter?.Content);

                AiFeedbackText = response;
                SetStatus("Claude responded. Use Insert buttons to add text to your chapter.");
                _audio.Speak("Claude responded. Review the response, then use Insert buttons to add it to your chapter, or say Insert at cursor.");
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

            SetStatus("AI response inserted at cursor position.");
            _audio.Speak("Inserted at cursor position.");
        }

        [RelayCommand(CanExecute = nameof(CanInsert))]
        private void InsertAtStart()
        {
            if (string.IsNullOrWhiteSpace(AiFeedbackText)) return;
            InsertTextRequested?.Invoke(this,
                new InsertTextArgs(AiFeedbackText, InsertPosition.AtBeginning));

            SetStatus("AI response inserted at beginning of chapter.");
            _audio.Speak("Inserted at beginning of chapter.");
        }

        [RelayCommand(CanExecute = nameof(CanInsert))]
        private void InsertAtEnd()
        {
            if (string.IsNullOrWhiteSpace(AiFeedbackText)) return;
            InsertTextRequested?.Invoke(this,
                new InsertTextArgs(AiFeedbackText, InsertPosition.AtEnd));

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

            string? title = PromptText("Save as Card", "Card title:", defaultTitle);
            if (title == null) return;

            string? category = PromptText("Save as Card", "Category:", "General");
            if (category == null) return;

            var card = new VoiceBookStudio.Models.ResponseCard
            {
                Title    = title,
                Category = category,
                Content  = AiFeedbackText
            };
            ResponseCardVM.AddCard(card);

            SetStatus($"Card saved: {title}");
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

        [RelayCommand]
        private void ToggleAppTts()
        {
            AppTtsEnabled  = !AppTtsEnabled;
            _audio.IsEnabled = AppTtsEnabled;

            if (!AppTtsEnabled)
            {
                _audio.IsEnabled = true;
                _audio.Speak("App voice off. JAWS will handle all audio.");
                _audio.IsEnabled = false;
            }
            else
            {
                _audio.Speak("App voice on.");
            }

            SetStatus(AppTtsStatusDisplay);
        }

        // ----------------------------------------------------------------
        // Internal helpers
        // ----------------------------------------------------------------

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
            SelectedChapter = Chapters.FirstOrDefault();
            if (SelectedChapter != null)
                _audio.Speak($"First chapter: {SelectedChapter.Title}.");
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
