using System;
using VoiceBookStudio.ViewModels;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Routes text-based voice commands (from Dragon NaturallySpeaking, JSay,
    /// or any other speech-to-text front-end) to the appropriate ViewModel action.
    ///
    /// Dragon NaturallySpeaking users: configure a custom MyCommand for each phrase
    /// that presses the matching keyboard shortcut (Ctrl+1, Ctrl+2, Ctrl+3).
    ///
    /// JSay users: map the phrases "Panel 1", "Panel 2", "Panel 3" to those shortcuts.
    /// </summary>
    public sealed class VoiceCommandRouter
    {
        private readonly MainViewModel _vm;

        public VoiceCommandRouter(MainViewModel vm)
        {
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
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

            // Project selection commands
            if (cmd.StartsWith("open "))
            {
                // Expecting 'Open <project name>'
                string name = cmd.Substring(5).Trim();
                _vm.TryOpenProjectByName(name);
                return true;
            }

            if (cmd == "new project")
            {
                _vm.TryCreateNewProject();
                return true;
            }

            if (cmd == "browse for project" || cmd == "browse project")
            {
                _vm.TryBrowseForProject();
                return true;
            }

            // ----------------------------------------------------------------
            // Prompt library commands
            // ----------------------------------------------------------------

            if (cmd == "open prompt library" || cmd == "show prompts")
            {
                _vm.TryOpenPromptLibrary();
                return true;
            }

            if (cmd.StartsWith("use prompt ") || cmd.StartsWith("prompt "))
            {
                string arg = cmd.Contains(" ") ? cmd.Substring(cmd.IndexOf(' ') + 1).Trim() : string.Empty;
                _vm.TryUsePromptById(arg);
                return true;
            }

            // ----------------------------------------------------------------
            // Response card commands
            // ----------------------------------------------------------------

            // "Open response cards" / "Show response cards" / "Cards"
            if (cmd == "open response cards" || cmd == "show response cards" || cmd == "cards")
            {
                _vm.TryOpenResponseCards();
                return true;
            }

            // "Save response card" / "Save card"
            if (cmd == "save response card" || cmd == "save card")
            {
                _vm.TrySaveResponseCard();
                return true;
            }

            // "Insert card 3"
            if (cmd.StartsWith("insert card "))
            {
                string num = cmd.Substring("insert card ".Length).Trim();
                if (int.TryParse(num, out int cardIdx))
                    _vm.TryInsertCard(cardIdx);
                return true;
            }

            // "Show Editing cards" / "Show Fiction cards"
            if (cmd.StartsWith("show ") && cmd.EndsWith(" cards"))
            {
                string category = cmd.Substring(5, cmd.Length - 5 - 6).Trim();
                if (!string.IsNullOrEmpty(category))
                    _vm.TryFilterCards(category);
                return true;
            }

            // "Delete card 2"
            if (cmd.StartsWith("delete card "))
            {
                string num = cmd.Substring("delete card ".Length).Trim();
                if (int.TryParse(num, out int delIdx))
                    _vm.TryDeleteCard(delIdx);
                return true;
            }

            // ----------------------------------------------------------------
            // Project commands
            // ----------------------------------------------------------------

            if (cmd is "save" or "save project" or "save now" or "save file")
            {
                _vm.TrySaveProject();
                return true;
            }

            // ----------------------------------------------------------------
            // Chapter management
            // ----------------------------------------------------------------

            if (cmd is "add chapter" or "new chapter" or "add section" or "new section")
            {
                _vm.TryAddChapter();
                return true;
            }

            if (cmd is "delete chapter" or "remove chapter" or "delete section"
                     or "delete current chapter" or "remove current chapter")
            {
                _vm.TryDeleteChapter();
                return true;
            }

            if (cmd is "rename chapter" or "rename section" or "rename current chapter" or "rename")
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
            // AI feedback
            // ----------------------------------------------------------------

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

            // ----------------------------------------------------------------
            // Tab navigation
            // ----------------------------------------------------------------

            if (cmd is "go to chat" or "chat tab" or "switch to chat" or "open chat" or "chat")
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

            if (cmd is "set api key" or "configure api" or "add api key" or "api key")
            {
                _vm.TrySetApiKey();
                return true;
            }

            return false;
        }
    }
}
