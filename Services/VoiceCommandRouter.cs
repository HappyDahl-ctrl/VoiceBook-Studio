using System;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Routes voice command text (from the built-in microphone listener, Dragon,
    /// JSay, or the chat input box) to the appropriate ViewModel action.
    /// </summary>
    public sealed class VoiceCommandRouter
    {
        private readonly MainViewModel _vm;

        // Set by MainViewModel when the five-step guided tour starts; cleared when it ends.
        private TutorialService? _tourService;

        public VoiceCommandRouter(MainViewModel vm)
        {
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        }

        /// <summary>
        /// Registers the active guided tour so that tour-specific commands are routed to it.
        /// Pass null to deregister (commands fall through to the normal handlers).
        /// </summary>
        public void SetTourService(TutorialService? service)
        {
            _tourService = service;
        }

        /// <summary>
        /// Attempts to match <paramref name="rawCommand"/> against a known voice command
        /// and executes the associated action.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the command was recognised and handled; <c>false</c> otherwise.
        /// </returns>
        public bool TryRoute(string rawCommand)
        {
            if (string.IsNullOrWhiteSpace(rawCommand)) return false;

            string cmd = rawCommand.Trim().ToLowerInvariant();

            // ----------------------------------------------------------------
            // Guided-tour commands — active only while TutorialService is running.
            // These intercept "next" and "repeat" so they go to TourService rather
            // than the 17-step TutorialViewModel when the quick tour is in progress.
            // ----------------------------------------------------------------
            if (_tourService?.IsActive == true)
            {
                if (cmd is "next step" or "next")
                {
                    _tourService.AdvanceStep();
                    return true;
                }
                if (cmd is "repeat step" or "repeat")
                {
                    _tourService.RepeatStep();
                    return true;
                }
                if (cmd is "end tour" or "exit tour" or "stop tour" or "cancel tour")
                {
                    _tourService.EndTour();
                    return true;
                }
            }

            // ----------------------------------------------------------------
            // Panel navigation — "Panel N" or "Go to panel N"
            // ----------------------------------------------------------------
            if (cmd is "panel 1" or "go to panel 1" or "panel one" or "go to panel one")
            {
                _vm.FocusPanel1();
                return true;
            }

            if (cmd is "panel 2" or "go to panel 2" or "panel two" or "go to panel two")
            {
                _vm.FocusPanel2();
                return true;
            }

            if (cmd is "panel 3" or "go to panel 3" or "panel three" or "go to panel three")
            {
                _vm.FocusPanel3();
                return true;
            }

            // Natural-language panel navigation aliases (matches Dragon XML command names)
            if (cmd is "go to chapters" or "go to chapter list")
            {
                _vm.FocusPanel1();
                return true;
            }

            if (cmd is "go to editor" or "open writing editor")
            {
                _vm.FocusPanel2();
                return true;
            }

            if (cmd is "go to assistant" or "open assistant panel")
            {
                _vm.FocusPanel3();
                return true;
            }

            // Tutorial navigation commands
            if (cmd == "next")
            {
                _vm.TryExecuteTutorialNext();
                return true;
            }

            if (cmd == "previous")
            {
                _vm.TryExecuteTutorialPrevious();
                return true;
            }

            if (cmd == "repeat")
            {
                _vm.TryExecuteTutorialRepeat();
                return true;
            }

            if (cmd == "exit tutorial")
            {
                _vm.TryExecuteTutorialExit();
                return true;
            }

            if (cmd == "start tutorial")
            {
                _vm.TryExecuteStartTutorial();
                return true;
            }

            // ----------------------------------------------------------------
            // Context-sensitive help — "what can I say here"
            // ----------------------------------------------------------------

            if (cmd is "what can i say" or "what can i say here" or "what can i do here"
                     or "list commands" or "help commands" or "commands" or "what commands"
                     or "what can i do" or "help me" or "voice commands")
            {
                _vm.AnnounceContextualHelp();
                return true;
            }

            // ----------------------------------------------------------------
            // Specific "open X" commands — must be checked BEFORE the generic
            // "open <project name>" handler below.
            // ----------------------------------------------------------------

            if (cmd == "open prompt library" || cmd == "show prompts")
            {
                _vm.TryOpenPromptLibrary();
                return true;
            }

            if (cmd == "open response cards" || cmd == "show response cards" || cmd == "cards")
            {
                _vm.TryOpenResponseCards();
                return true;
            }

            if (cmd is "go to chat" or "chat tab" or "switch to chat" or "open chat")
            {
                _vm.TryOpenChat();
                return true;
            }

            // ----------------------------------------------------------------
            // Prompt library — variable ID form
            // ----------------------------------------------------------------

            if (cmd.StartsWith("use prompt "))
            {
                _vm.TryUsePromptById(ResolveSpokenPromptId(cmd["use prompt ".Length..].Trim()));
                return true;
            }

            if (cmd.StartsWith("prompt "))
            {
                _vm.TryUsePromptById(ResolveSpokenPromptId(cmd["prompt ".Length..].Trim()));
                return true;
            }

            // ----------------------------------------------------------------
            // Project selection commands
            // ----------------------------------------------------------------

            if (cmd is "new project" or "create project" or "start new project" or "create new project")
            {
                _vm.TryCreateNewProject();
                return true;
            }

            if (cmd is "save as" or "save project as")
            {
                _vm.TrySaveProjectAs();
                return true;
            }

            // Browse / open project file picker
            if (cmd is "open project" or "browse for project" or "browse project"
                     or "open file" or "find project")
            {
                _vm.TryBrowseForProject();
                return true;
            }

            // Import document
            if (cmd is "import document" or "import file" or "import word document"
                     or "import word" or "import docx" or "open document"
                     or "import from word" or "bring in document")
            {
                _vm.TryImportDocument();
                return true;
            }

            // Specific "open …" commands — must precede the generic "open <project name>"
            // handler so these are not routed to TryOpenProjectByName.
            if (cmd == "open settings")
            {
                _vm.TryOpenSettings();
                return true;
            }

            if (cmd is "open feedback" or "open feedback library")
            {
                _vm.TryOpenFeedbackLibrary();
                return true;
            }

            if (cmd.StartsWith("open "))
            {
                // Generic: 'Open <project name>'
                string name = cmd["open ".Length..].Trim();
                _vm.TryOpenProjectByName(name);
                return true;
            }

            // "Save response card" / "Save card"
            if (cmd == "save response card" || cmd == "save card")
            {
                _vm.TrySaveResponseCard();
                return true;
            }

            // "Insert card 3"  /  "insert card three"
            if (cmd.StartsWith("insert card "))
            {
                string num = cmd["insert card ".Length..].Trim();
                int? cardIdx = ParseSpokenNumber(num);
                if (cardIdx.HasValue) _vm.TryInsertCard(cardIdx.Value);
                return true;
            }

            // "Show Editing cards" / "Show Fiction cards"
            if (cmd.StartsWith("show ") && cmd.EndsWith(" cards"))
            {
                string category = cmd[5..(cmd.Length - 6)].Trim();
                if (!string.IsNullOrEmpty(category))
                    _vm.TryFilterCards(category);
                return true;
            }

            // "Delete card 2"  /  "delete card two"
            if (cmd.StartsWith("delete card "))
            {
                string num = cmd["delete card ".Length..].Trim();
                int? delIdx = ParseSpokenNumber(num);
                if (delIdx.HasValue) _vm.TryDeleteCard(delIdx.Value);
                return true;
            }

            // ----------------------------------------------------------------
            // Project commands
            // ----------------------------------------------------------------

            if (cmd is "save" or "save project" or "save now" or "save file" or "save chapter" or "save all")
            {
                _vm.TrySaveProject();
                return true;
            }

            // ----------------------------------------------------------------
            // Chapter management
            // ----------------------------------------------------------------

            if (cmd is "add chapter" or "new chapter" or "add section" or "new section"
                     or "create chapter" or "create section" or "write new chapter")
            {
                _vm.TryAddChapter();
                return true;
            }

            if (cmd is "delete chapter" or "remove chapter" or "delete section"
                     or "delete current chapter" or "remove current chapter"
                     or "erase chapter" or "get rid of chapter")
            {
                _vm.TryDeleteChapter();
                return true;
            }

            if (cmd is "rename chapter" or "rename section" or "rename current chapter" or "rename"
                     or "change chapter name" or "edit title" or "change title")
            {
                _vm.TryRenameChapter();
                return true;
            }

            if (cmd is "move up" or "move chapter up" or "move section up")
            {
                _vm.TryMoveChapterUp();
                return true;
            }

            if (cmd is "move down" or "move chapter down" or "move section down")
            {
                _vm.TryMoveChapterDown();
                return true;
            }

            if (cmd is "change type" or "change section type" or "change chapter type")
            {
                _vm.TryChangeChapterType();
                return true;
            }

            // ----------------------------------------------------------------
            // Chapter navigation
            // ----------------------------------------------------------------

            if (cmd is "next chapter" or "go to next chapter")
            {
                _vm.TrySelectNextChapter();
                return true;
            }

            if (cmd is "previous chapter" or "go to previous chapter" or "prior chapter")
            {
                _vm.TrySelectPreviousChapter();
                return true;
            }

            if (cmd is "open chapter" or "show chapter" or "select chapter")
            {
                _vm.TryOpenChapter();
                return true;
            }

            // ----------------------------------------------------------------
            // Editor reading — chapter content and cursor paragraph
            // ----------------------------------------------------------------

            if (cmd is "read chapter title" or "chapter title" or "what chapter" or "current chapter")
            {
                _vm.TryAnnounceChapterTitle();
                return true;
            }

            if (cmd is "read paragraph" or "read current paragraph" or "read this paragraph")
            {
                _vm.TryReadParagraph();
                return true;
            }

            if (cmd is "read chapter" or "read current chapter" or "read all" or "read chapter content")
            {
                _vm.TryReadChapter();
                return true;
            }

            if (cmd is "stop reading" or "stop speech" or "stop" or "quiet" or "silence")
            {
                _vm.TryStopReading();
                return true;
            }

            // ----------------------------------------------------------------
            // AI feedback
            // ----------------------------------------------------------------

            if (cmd is "analyse book" or "analyze book" or "book analysis"
                     or "book feedback" or "full book feedback" or "review book"
                     or "whole book" or "entire book feedback")
            {
                _vm.TryRunBookFeedback();
                return true;
            }

            if (cmd is "run feedback" or "get feedback" or "feedback"
                     or "run comprehensive" or "comprehensive feedback" or "comprehensive")
            {
                _vm.TryRunAiFeedback("comprehensive");
                return true;
            }

            if (cmd is "pacing" or "pacing feedback" or "run pacing" or "check pacing")
            {
                _vm.TryRunAiFeedback("pacing");
                return true;
            }

            if (cmd is "dialogue" or "dialogue feedback" or "run dialogue" or "check dialogue")
            {
                _vm.TryRunAiFeedback("dialogue");
                return true;
            }

            if (cmd is "style" or "style feedback" or "run style" or "check style")
            {
                _vm.TryRunAiFeedback("style");
                return true;
            }

            if (cmd is "structure" or "structure feedback" or "run structure" or "check structure")
            {
                _vm.TryRunAiFeedback("structure");
                return true;
            }

            // "ask assistant [question]" — Dragon template command sends just the question text
            // after stripping the "ask assistant " prefix.  rawCommand preserves user's case.
            if (cmd.StartsWith("ask assistant "))
            {
                string question = rawCommand.Trim()["ask assistant ".Length..].Trim();
                if (!string.IsNullOrWhiteSpace(question))
                    _vm.TryAskAssistant(question);
                return true;
            }

            if (cmd is "send" or "send message" or "send chat" or "send to claude" or "ask claude")
            {
                _vm.TrySendChat();
                return true;
            }

            // ----------------------------------------------------------------
            // Insert AI response
            // ----------------------------------------------------------------

            if (cmd is "insert at cursor" or "insert here" or "insert response" or "insert response here")
            {
                _vm.TryInsertAtCursor();
                return true;
            }

            if (cmd is "insert at start" or "insert at beginning"
                     or "insert at the start" or "insert at the beginning")
            {
                _vm.TryInsertAtStart();
                return true;
            }

            if (cmd is "insert at end" or "insert at the end"
                     or "append response" or "append to chapter")
            {
                _vm.TryInsertAtEnd();
                return true;
            }

            // ----------------------------------------------------------------
            // AI response management
            // ----------------------------------------------------------------

            if (cmd is "read response" or "read the response" or "read ai response" or "read answer")
            {
                _vm.TryReadAiResponse();
                return true;
            }

            if (cmd is "keep response" or "save response" or "keep this response")
            {
                _vm.TrySaveResponseCard();
                return true;
            }

            if (cmd is "discard response" or "clear response" or "remove response" or "delete response")
            {
                _vm.TryDiscardResponse();
                return true;
            }

            // ----------------------------------------------------------------
            // Export
            // ----------------------------------------------------------------

            if (cmd is "export word" or "export docx" or "export manuscript"
                     or "export as word" or "save as word")
            {
                _vm.TryExportDocx();
                return true;
            }

            if (cmd is "export pdf" or "export as pdf" or "save as pdf"
                     or "create pdf" or "make pdf")
            {
                _vm.TryExportPdf();
                return true;
            }

            // "chat" alone (without "open") also maps to the chat tab
            if (cmd == "chat")
            {
                _vm.TryOpenChat();
                return true;
            }

            // ----------------------------------------------------------------
            // Settings
            // ----------------------------------------------------------------

            if (cmd is "toggle voice" or "voice on" or "voice off"
                     or "mute app" or "unmute app" or "toggle app voice")
            {
                _vm.TryToggleAppTts();
                return true;
            }

            if (cmd is "set api key" or "configure api" or "add api key" or "api key"
                     or "configure ai" or "set up ai" or "enter api key" or "add key")
            {
                _vm.TrySetApiKey();
                return true;
            }

            // ----------------------------------------------------------------
            // Settings dialog
            // ----------------------------------------------------------------

            if (cmd is "open settings" or "settings" or "show settings"
                     or "open preferences" or "preferences")
            {
                _vm.TryOpenSettings();
                return true;
            }

            if (cmd is "set project folder" or "change project folder"
                     or "default project folder" or "set default folder"
                     or "change default folder" or "project folder")
            {
                _vm.TryOpenDefaultFolderPicker();
                return true;
            }

            // ----------------------------------------------------------------
            // Application status and lifecycle
            // ----------------------------------------------------------------

            if (cmd is "application status" or "app status" or "status" or "current status"
                     or "what is the status" or "what is happening")
            {
                _vm.TryAnnounceApplicationStatus();
                return true;
            }

            if (cmd is "close application" or "quit application" or "exit application"
                     or "close voicebook" or "quit voicebook" or "exit voicebook")
            {
                _vm.TryCloseApplication();
                return true;
            }

            // ----------------------------------------------------------------
            // Library navigation — Prompts
            // ----------------------------------------------------------------

            if (cmd is "what's in my prompt library" or "read prompt categories" or "prompt categories"
                     or "what prompts do i have" or "show prompt categories"
                     or "list prompt categories" or "prompt library")
            {
                _vm.TryReadPromptCategories();
                return true;
            }

            // "Read Prompt A" / "Read Prompts A"
            if ((cmd.StartsWith("read prompt ") || cmd.StartsWith("read prompts ")) &&
                cmd.Length >= 13)
            {
                string letter = cmd.Split(' ').Last().ToUpper();
                if (letter.Length == 1 && char.IsLetter(letter[0]))
                {
                    _vm.TryReadPromptCategory(letter);
                    return true;
                }
            }

            // "Add new prompt" / "Add prompt"
            if (cmd is "add new prompt" or "add prompt" or "create prompt" or "new prompt"
                     or "create a new prompt")
            {
                _vm.TryAddPrompt();
                return true;
            }

            // ----------------------------------------------------------------
            // Library navigation — Cards
            // ----------------------------------------------------------------

            if (cmd is "what's in my card library" or "read card categories" or "card categories"
                     or "what cards do i have" or "show card categories" or "list card categories")
            {
                _vm.TryReadCardCategories();
                return true;
            }

            // "Read Cards A" / "Read Card A"
            if ((cmd.StartsWith("read cards ") || cmd.StartsWith("read card ")) &&
                cmd.Length >= 12)
            {
                string letter = cmd.Split(' ').Last().ToUpper();
                if (letter.Length == 1 && char.IsLetter(letter[0]))
                {
                    _vm.TryReadCardCategory(letter);
                    return true;
                }
            }

            // "Open feedback library" / "Feedback library"
            if (cmd is "open feedback library" or "open feedback" or "feedback library"
                     or "show feedback library" or "my feedback library")
            {
                _vm.TryOpenFeedbackLibrary();
                return true;
            }

            // ----------------------------------------------------------------
            // Library navigation — Feedback
            // ----------------------------------------------------------------

            if (cmd is "what's in my feedback library" or "read feedback categories"
                     or "feedback categories" or "what feedback do i have"
                     or "show feedback categories" or "list feedback categories")
            {
                _vm.TryReadFeedbackCategories();
                return true;
            }

            // "Read Feedback B" / "Read My Feedback B" / "Read My Pacing feedback"
            if (cmd.StartsWith("read feedback ") && cmd.Length >= 15)
            {
                string part = cmd["read feedback ".Length..].Trim().ToUpper();
                if (part.Length == 1 && char.IsLetter(part[0]))
                {
                    _vm.TryReadFeedbackCategory(part);
                    return true;
                }
                // Map spoken category names to letters
                string feedbackLetter = part switch
                {
                    "COMPREHENSIVE" => "A",
                    "PACING"        => "B",
                    "DIALOGUE"      => "C",
                    "STYLE"         => "D",
                    "STRUCTURE"     => "E",
                    _               => string.Empty
                };
                if (feedbackLetter.Length > 0)
                {
                    _vm.TryReadFeedbackCategory(feedbackLetter);
                    return true;
                }
            }

            if (cmd.StartsWith("read my ") && cmd.EndsWith(" feedback"))
            {
                string type = cmd["read my ".Length..^" feedback".Length].Trim().ToUpper();
                string feedbackLetter = type switch
                {
                    "COMPREHENSIVE" => "A",
                    "PACING"        => "B",
                    "DIALOGUE"      => "C",
                    "STYLE"         => "D",
                    "STRUCTURE"     => "E",
                    _               => string.Empty
                };
                if (feedbackLetter.Length > 0)
                {
                    _vm.TryReadFeedbackCategory(feedbackLetter);
                    return true;
                }
            }

            // "Read A1" / "Read B3" — routes to correct library based on last context
            if (cmd.StartsWith("read ") && cmd.Length >= 7)
            {
                string id = cmd["read ".Length..].Trim().ToUpper();
                if (id.Length >= 2 && char.IsLetter(id[0]) && int.TryParse(id[1..], out _))
                {
                    _vm.TryReadEntry(id);
                    return true;
                }
            }

            // "Insert Card A1" / "Use Card A1"
            if ((cmd.StartsWith("insert card ") || cmd.StartsWith("use card ")) &&
                cmd.Length >= 13)
            {
                string id = cmd.Split(' ').Last().ToUpper();
                if (id.Length >= 2 && char.IsLetter(id[0]) && int.TryParse(id[1..], out _))
                {
                    _vm.TryInsertCardByLetterNumber(id);
                    return true;
                }
            }

            // ----------------------------------------------------------------
            // Resume reading
            // ----------------------------------------------------------------

            if (cmd is "resume reading" or "continue reading" or "resume")
            {
                _vm.ResumeLibraryReading();
                return true;
            }

            // ----------------------------------------------------------------
            // Tutorial continuation (voice alternative to pressing Enter on
            // steps that require "continue" to advance)
            // ----------------------------------------------------------------

            // "hello" serves as the mic test confirmation in the tutorial audio check step
            if (cmd is "continue" or "continue tutorial" or "yes" or "yes continue"
                     or "hello" or "audio ok" or "audio confirmed" or "audio good"
                     or "i can hear you" or "i hear you" or "testing")
            {
                _vm.TryExecuteTutorialContinue();
                return true;
            }

            // ----------------------------------------------------------------
            // Tutorial skip step (voice alternative to pressing S in tutorial)
            // ----------------------------------------------------------------

            if (cmd is "skip step" or "skip this step" or "skip")
            {
                _vm.TryExecuteTutorialSkip();
                return true;
            }

            return false;
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Parses a number from either a digit string ("3") or a spoken word form
        /// ("three") as produced by the built-in speech recogniser.
        /// Returns null if the input cannot be resolved.
        /// </summary>
        private static int? ParseSpokenNumber(string word)
        {
            if (int.TryParse(word, out int n)) return n;

            return word switch
            {
                "one"       => 1,
                "two"       => 2,
                "three"     => 3,
                "four"      => 4,
                "five"      => 5,
                "six"       => 6,
                "seven"     => 7,
                "eight"     => 8,
                "nine"      => 9,
                "ten"       => 10,
                "eleven"    => 11,
                "twelve"    => 12,
                "thirteen"  => 13,
                "fourteen"  => 14,
                "fifteen"   => 15,
                "sixteen"   => 16,
                "seventeen" => 17,
                "eighteen"  => 18,
                "nineteen"  => 19,
                "twenty"    => 20,
                _           => null
            };
        }

        /// <summary>
        /// Converts a spoken prompt identifier into the compact alphanumeric form used
        /// by the prompt library. Handles both built-in mic (spoken words) and Dragon
        /// (digits or pre-combined IDs like "a1").
        ///
        /// Examples:
        ///   "a one"  → "a1"
        ///   "b three" → "b3"
        ///   "a1"     → "a1"  (Dragon / keyboard input — passed through unchanged)
        ///   "j ten"  → "j10"
        /// </summary>
        private static string ResolveSpokenPromptId(string raw)
        {
            // Already compact (e.g. Dragon said "a1") — pass through.
            if (raw.Length >= 2 && char.IsLetter(raw[0]) && char.IsDigit(raw[1]))
                return raw;

            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && parts[0].Length == 1 && char.IsLetter(parts[0][0]))
            {
                string digit = parts[1] switch
                {
                    "one"   or "1"  => "1",
                    "two"   or "2"  => "2",
                    "three" or "3"  => "3",
                    "four"  or "4"  => "4",
                    "five"  or "5"  => "5",
                    "six"   or "6"  => "6",
                    "seven" or "7"  => "7",
                    "eight" or "8"  => "8",
                    "nine"  or "9"  => "9",
                    "ten"   or "10" => "10",
                    _               => parts[1]
                };
                return parts[0] + digit;
            }

            return raw; // unrecognised pattern — pass through and let SelectById report "not found"
        }
    }
}

