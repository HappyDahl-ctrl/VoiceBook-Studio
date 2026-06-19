using System;
using System.Collections.Generic;
using System.Globalization;
using System.Speech.Recognition;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Wraps Windows Speech Recognition (System.Speech) so VoiceBook Studio can
    /// listen for voice commands without Dragon, JSay, or any external STT tool.
    ///
    /// Call <see cref="StartListening"/> to activate the microphone.
    /// <see cref="CommandRecognized"/> fires on a thread-pool thread — callers
    /// must marshal to the UI thread (Dispatcher.Invoke) before touching WPF objects.
    /// </summary>
    public sealed class SpeechListenerService : IDisposable
    {
        private SpeechRecognitionEngine? _engine;
        private bool _disposed;

        public bool IsListening { get; private set; }

        /// <summary>
        /// Fires when a command phrase is recognised with >= 70 % confidence.
        /// The string is the recognised text in lower-case, ready to pass to
        /// <see cref="VoiceCommandRouter.TryRoute"/>.
        /// </summary>
        public event EventHandler<string>? CommandRecognized;

        // ----------------------------------------------------------------
        // Start / Stop
        // ----------------------------------------------------------------

        /// <summary>
        /// Opens the default microphone and begins continuous recognition.
        /// Returns <c>true</c> on success, <c>false</c> if no mic is found or
        /// the recogniser cannot start (e.g. Windows Speech Recognition not set up).
        /// </summary>
        public bool StartListening()
        {
            if (_disposed || IsListening) return IsListening;

            try
            {
                _engine = new SpeechRecognitionEngine(new CultureInfo("en-US"));
                _engine.SetInputToDefaultAudioDevice();
                _engine.LoadGrammar(BuildGrammar());
                _engine.SpeechRecognized += OnSpeechRecognized;
                _engine.RecognizeAsync(RecognizeMode.Multiple);
                IsListening = true;
                return true;
            }
            catch
            {
                _engine?.Dispose();
                _engine = null;
                IsListening = false;
                return false;
            }
        }

        public void StopListening()
        {
            if (!IsListening || _engine is null) return;
            _engine.SpeechRecognized -= OnSpeechRecognized;
            try { _engine.RecognizeAsyncStop(); } catch { /* ignore on teardown */ }
            _engine.Dispose();
            _engine = null;
            IsListening = false;
        }

        // ----------------------------------------------------------------
        // Recognition callback
        // ----------------------------------------------------------------

        private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= 0.70f)
                CommandRecognized?.Invoke(this, e.Result.Text.ToLowerInvariant());
        }

        // ----------------------------------------------------------------
        // Grammar construction
        // ----------------------------------------------------------------

        private static Grammar BuildGrammar()
        {
            // All single fixed-phrase commands (what the user says aloud)
            var fixedPhrases = new List<string>
            {
                // Context-sensitive help
                "what can i say", "what can i say here", "what can i do here",
                "list commands", "help commands", "commands", "what commands",
                "what can i do", "help me", "voice commands",

                // Panel navigation
                "panel one", "panel two", "panel three",
                "go to panel one", "go to panel two", "go to panel three",
                "panel 1", "panel 2", "panel 3",
                "go to panel 1", "go to panel 2", "go to panel 3",

                // Tutorial navigation
                "next", "previous", "repeat", "exit tutorial", "start tutorial",
                "continue", "continue tutorial", "yes", "yes continue",
                "skip step", "skip this step", "skip",

                // Library reading
                "resume reading", "continue reading", "resume",

                // Project
                "new project", "create project", "start new project", "create new project",
                "open project", "browse for project", "browse project", "open file", "find project",
                "save", "save project", "save now", "save file",
                "save as", "save project as",

                // Import
                "import document", "import file", "import word document",
                "import word", "import docx", "import from word",
                "open document", "bring in document",

                // Chapter management
                "add chapter", "new chapter", "add section", "new section",
                "create chapter", "create section", "write new chapter",
                "delete chapter", "remove chapter", "delete section",
                "delete current chapter", "remove current chapter",
                "erase chapter", "get rid of chapter",
                "rename chapter", "rename section", "rename current chapter", "rename",
                "change chapter name", "edit title", "change title",
                "move up", "move chapter up", "move section up",
                "move down", "move chapter down", "move section down",
                "change type", "change section type", "change chapter type",

                // AI feedback — chapter level
                "run feedback", "get feedback", "feedback",
                "run comprehensive", "comprehensive feedback", "comprehensive",

                // AI feedback — book level
                "analyse book", "analyze book", "book analysis",
                "book feedback", "full book feedback", "review book",
                "whole book", "entire book feedback",
                "pacing", "pacing feedback", "run pacing", "check pacing",
                "dialogue", "dialogue feedback", "run dialogue", "check dialogue",
                "style", "style feedback", "run style", "check style",
                "structure", "structure feedback", "run structure", "check structure",
                "send", "send message", "send chat", "send to claude", "ask claude",

                // Insert AI response
                "insert at cursor", "insert here", "insert response", "insert response here",
                "insert at start", "insert at beginning",
                "insert at the start", "insert at the beginning",
                "insert at end", "insert at the end",
                "append response", "append to chapter",

                // Export
                "export word", "export docx", "export manuscript",
                "export as word", "save as word",
                "export pdf", "export as pdf", "save as pdf",
                "create pdf", "make pdf",

                // Tab / panel navigation
                "go to chat", "chat tab", "switch to chat", "open chat", "chat",
                "open prompt library", "show prompts",
                "open response cards", "show response cards", "cards",

                // Response cards
                "save response card", "save card",

                // Settings dialog
                "open settings", "settings", "show settings", "open preferences", "preferences",

                // Project folder
                "set project folder", "change project folder", "default project folder",
                "set default folder", "change default folder", "project folder",

                // App voice / TTS
                "toggle voice", "voice on", "voice off",
                "mute app", "unmute app", "toggle app voice",

                // API key
                "set api key", "configure api", "add api key", "api key",
                "configure ai", "set up ai", "enter api key", "add key",

                // Prompt library listing
                "prompt library", "what prompts do i have", "show prompt categories",
                "list prompt categories", "prompt categories", "read prompt categories",

                // Prompt management
                "add prompt", "add new prompt", "create prompt", "new prompt", "create a new prompt",

                // Card library listing
                "card categories", "what cards do i have", "show card categories",
                "list card categories", "read card categories",

                // Feedback library
                "open feedback library", "open feedback", "feedback library",
                "show feedback library", "my feedback library",

                // Feedback library listing
                "feedback categories", "what feedback do i have",
                "show feedback categories", "list feedback categories", "read feedback categories",

                // Feedback by category name
                "read feedback comprehensive", "read feedback pacing",
                "read feedback dialogue", "read feedback style", "read feedback structure",
                "read my comprehensive feedback", "read my pacing feedback",
                "read my dialogue feedback", "read my style feedback", "read my structure feedback",

                // Tutorial audio-check phrases (mic test step)
                "hello", "audio ok", "audio confirmed", "audio good",
                "i can hear you", "i hear you", "testing",

                // Chapter navigation aliases
                "next chapter", "go to next chapter",
                "previous chapter", "go to previous chapter", "prior chapter",
                "open chapter", "show chapter", "select chapter",

                // Reading
                "read paragraph", "read current paragraph", "read this paragraph",
                "read chapter", "read current chapter",
                "read all", "read chapter content", "read chapter title",
                "chapter title", "what chapter", "current chapter",

                // Stop reading / silence TTS
                "stop reading", "stop speech", "stop", "quiet", "silence",

                // AI response management
                "read response", "read the response", "read ai response", "read answer",
                "keep response", "save response", "keep this response",
                "discard response", "clear response", "remove response", "delete response",

                // Application status
                "application status", "app status", "status",
                "current status", "what is the status", "what is happening",

                // Application lifecycle
                "close application", "quit application", "exit application",
                "close voicebook", "quit voicebook", "exit voicebook",

                // Save aliases
                "save chapter", "save all",

                // Panel navigation aliases
                "go to chapters", "go to chapter list",
                "go to editor", "open writing editor",
                "go to assistant", "open assistant panel",
            };

            // Spoken ordinals for card indices 1-20
            var cardNumbers = new Choices(
                "one", "two", "three", "four", "five",
                "six", "seven", "eight", "nine", "ten",
                "eleven", "twelve", "thirteen", "fourteen", "fifteen",
                "sixteen", "seventeen", "eighteen", "nineteen", "twenty");

            // "insert card three"
            var insertCard = new GrammarBuilder("insert card");
            insertCard.Append(cardNumbers);

            // "delete card five"
            var deleteCard = new GrammarBuilder("delete card");
            deleteCard.Append(cardNumbers);

            // "show fiction cards" / "show all cards"
            var categories = new Choices(
                "fiction", "nonfiction", "editing", "characters", "plot",
                "dialogue", "setting", "theme", "opening", "style", "all");
            var showCards = new GrammarBuilder("show");
            showCards.Append(categories);
            showCards.Append("cards");

            // "read prompt a" / "read prompts a" — prompt categories A-J (10 categories)
            var promptLetters = new Choices("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");
            var readPrompt = new GrammarBuilder("read prompt");
            readPrompt.Append(promptLetters);
            var readPrompts = new GrammarBuilder("read prompts");
            readPrompts.Append(promptLetters);

            // "read card a" / "read cards a" — card categories A-J
            var cardLetters = new Choices("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");
            var readCard = new GrammarBuilder("read card");
            readCard.Append(cardLetters);
            var readCards = new GrammarBuilder("read cards");
            readCards.Append(cardLetters);

            // "read feedback a" through "read feedback e" — feedback categories A-E
            var feedbackLetters = new Choices("a", "b", "c", "d", "e");
            var readFeedbackLetter = new GrammarBuilder("read feedback");
            readFeedbackLetter.Append(feedbackLetters);

            // "use prompt a one" … "use prompt j ten"
            // Numbers are spoken words; VoiceCommandRouter.ResolveSpokenPromptId converts them to A1..J10.
            var usePromptLetters  = new Choices("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");
            var usePromptNumbers  = new Choices(
                "one", "two", "three", "four", "five",
                "six", "seven", "eight", "nine", "ten");
            var usePromptPattern = new GrammarBuilder("use prompt");
            usePromptPattern.Append(usePromptLetters);
            usePromptPattern.Append(usePromptNumbers);

            // "prompt a one" shorthand (matches VoiceCommandRouter "prompt " prefix handler)
            var promptShorthand = new GrammarBuilder("prompt");
            promptShorthand.Append(usePromptLetters);
            promptShorthand.Append(usePromptNumbers);

            // Combine everything
            var topLevel = new Choices();
            topLevel.Add(fixedPhrases.ToArray());
            topLevel.Add(insertCard);
            topLevel.Add(deleteCard);
            topLevel.Add(showCards);
            topLevel.Add(readPrompt);
            topLevel.Add(readPrompts);
            topLevel.Add(readCard);
            topLevel.Add(readCards);
            topLevel.Add(readFeedbackLetter);
            topLevel.Add(usePromptPattern);
            topLevel.Add(promptShorthand);

            return new Grammar(new GrammarBuilder(topLevel)) { Name = "VoiceBookCommands" };
        }

        // ----------------------------------------------------------------
        // IDisposable
        // ----------------------------------------------------------------

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopListening();
        }
    }
}
