using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using VoiceBookStudio.Services;

namespace VoiceBookStudio.Views
{
    public partial class HelpDialog : Window
    {
        private readonly AudioFeedbackService? _audio;
        private bool _showingTopic;

        public HelpDialog(AudioFeedbackService? audio = null)
        {
            InitializeComponent();
            _audio = audio;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowTopicList();
            TopicListBox.Focus();

            // Read topic names aloud so the user knows what's available without tabbing
            _audio?.Speak(
                "Help topics. " +
                "One: Getting Started. " +
                "Two: Navigating the Three Panels. " +
                "Three: Managing Chapters. " +
                "Four: Writing in the Editor. " +
                "Five: AI Assistant. " +
                "Six: Voice Commands. " +
                "Seven: Dragon and JSay Guide. " +
                "Eight: Keyboard Shortcuts. " +
                "Nine: Exporting and Importing. " +
                "Press a number key to open a topic.");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            if (e.Key == Key.Escape)
            {
                if (_showingTopic) ShowTopicList();
                else               Close();
                e.Handled = true;
                return;
            }

            // Number keys 1–9 jump straight to a topic
            int idx = e.Key switch
            {
                Key.D1 or Key.NumPad1 => 0,
                Key.D2 or Key.NumPad2 => 1,
                Key.D3 or Key.NumPad3 => 2,
                Key.D4 or Key.NumPad4 => 3,
                Key.D5 or Key.NumPad5 => 4,
                Key.D6 or Key.NumPad6 => 5,
                Key.D7 or Key.NumPad7 => 6,
                Key.D8 or Key.NumPad8 => 7,
                Key.D9 or Key.NumPad9 => 8,
                _                     => -1
            };

            if (idx >= 0)
            {
                OpenTopic(idx);
                e.Handled = true;
            }
        }

        private void Topic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = TopicListBox.SelectedIndex;
            if (idx >= 0) OpenTopic(idx);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => ShowTopicList();

        // ----------------------------------------------------------------

        private void ShowTopicList()
        {
            _showingTopic         = false;
            TopicListBox.Visibility  = Visibility.Visible;
            ContentBox.Visibility    = Visibility.Collapsed;
            BackButton.Visibility    = Visibility.Collapsed;
            HeaderLabel.Text         = "Help Topics";
            FooterHint.Text          = "Press 1–9 to jump to a topic  ·  Enter or click to open  ·  Escape to close";

            TopicListBox.SelectedIndex = -1;
            TopicListBox.Focus();
        }

        private void OpenTopic(int zeroBasedIndex)
        {
            _showingTopic         = true;
            TopicListBox.Visibility  = Visibility.Collapsed;
            ContentBox.Visibility    = Visibility.Visible;
            BackButton.Visibility    = Visibility.Visible;
            FooterHint.Text          = "Arrow keys to read line by line  ·  Ctrl+Down for next paragraph  ·  Escape to go back";

            string title   = TopicTitles[zeroBasedIndex];
            string content = TopicContent[zeroBasedIndex];

            HeaderLabel.Text = title;
            AutomationProperties.SetName(HeaderLabel, title);

            ContentBox.Text = content;
            AutomationProperties.SetName(ContentBox, $"Help topic: {title}");

            ContentBox.ScrollToHome();
            ContentBox.Focus();

            _audio?.Speak(title + ".");
        }

        // ----------------------------------------------------------------
        // Help content
        // ----------------------------------------------------------------

        private static readonly string[] TopicTitles =
        {
            "1. Getting Started",
            "2. Navigating the Three Panels",
            "3. Managing Chapters and Sections",
            "4. Writing in the Editor",
            "5. AI Assistant and Feedback",
            "6. Voice Commands Reference",
            "7. Dragon NaturallySpeaking and JSay Guide",
            "8. Keyboard Shortcuts",
            "9. Exporting and Importing"
        };

        private static readonly string[] TopicContent =
        {
            // 1. Getting Started
            "GETTING STARTED\n" +
            "================\n\n" +
            "WHAT IS VOICEBOOK STUDIO?\n" +
            "VoiceBook Studio is an accessibility-first book writing app designed for " +
            "JAWS screen reader, Dragon NaturallySpeaking, and JSay voice control.\n\n" +
            "CREATING YOUR FIRST PROJECT\n" +
            "Press Ctrl+N or go to File > New Project.\n" +
            "Type a title for your book and press Enter.\n" +
            "A first chapter is created automatically.\n\n" +
            "OPENING AN EXISTING PROJECT\n" +
            "Press Ctrl+O or go to File > Open Project.\n" +
            "Choose your .vbk project file.\n\n" +
            "WHERE FILES ARE SAVED\n" +
            "Projects are saved as .vbk files in My Documents\\VoiceBook Projects.\n" +
            "The app auto-saves every 30 seconds when there are unsaved changes.\n" +
            "Response cards are saved in AppData\\Roaming\\VoiceBookStudio.\n\n" +
            "ANTHROPIC API KEY (for AI features)\n" +
            "AI features require an Anthropic API key.\n" +
            "Click the AI Not Set button in the toolbar to add your key.\n" +
            "Your key is stored securely in the Windows registry.\n" +
            "Get a key at console.anthropic.com.",

            // 2. Three Panels
            "NAVIGATING THE THREE PANELS\n" +
            "============================\n\n" +
            "VoiceBook Studio has three panels side by side:\n\n" +
            "PANEL 1 — CHAPTER MANAGER  (F1 or Ctrl+1)\n" +
            "Lists all your chapters and sections.\n" +
            "Use the buttons here to add, rename, delete, and reorder chapters.\n\n" +
            "PANEL 2 — WRITING EDITOR  (F2 or Ctrl+2)\n" +
            "Where you type or dictate your book.\n" +
            "Dragon NaturallySpeaking dictates naturally into this field.\n" +
            "Press Escape or F1 to leave the editor and return to the chapter list.\n\n" +
            "PANEL 3 — AI ASSISTANT  (F3 or Ctrl+3)\n" +
            "Chat with Claude, run writing analysis, use prompts and cards.\n" +
            "The Chat input box also works as a voice command router.\n\n" +
            "SWITCHING PANELS\n" +
            "Press F1, F2, or F3 — works from anywhere, including inside the editor.\n" +
            "Alternatively: Ctrl+1, Ctrl+2, or Ctrl+3.\n" +
            "Dragon users: say \"Press F1\", \"Press F2\", or \"Press F3\".\n\n" +
            "TAB ORDER\n" +
            "Pressing Tab moves through controls in each panel in logical order.\n" +
            "JAWS reads each control's name and help text as you tab to it.",

            // 3. Chapters
            "MANAGING CHAPTERS AND SECTIONS\n" +
            "================================\n\n" +
            "ADDING A CHAPTER\n" +
            "Press Ctrl+A, or click Add Chapter in Panel 1, or use File > Add Chapter.\n" +
            "You will be asked to choose a section type, then a title.\n\n" +
            "SECTION TYPES\n" +
            "Front matter:  Title Page, Dedication, Foreword, Introduction, Preface, Epigraph\n" +
            "Body chapters: Chapter (default)\n" +
            "Back matter:   Afterword, Appendix, Glossary, Bibliography, About the Author\n" +
            "The app automatically sorts sections into front matter, chapters, back matter.\n\n" +
            "RENAMING A CHAPTER\n" +
            "Select the chapter in Panel 1, then press Ctrl+D or click Rename.\n\n" +
            "DELETING A CHAPTER\n" +
            "Select the chapter, then press Ctrl+Delete or click Delete.\n" +
            "You will be asked to confirm. This cannot be undone.\n\n" +
            "REORDERING CHAPTERS\n" +
            "Select a chapter and press Alt+Up Arrow to move it up.\n" +
            "Press Alt+Down Arrow to move it down.\n" +
            "Or click the Move Up and Move Down buttons.\n\n" +
            "CHANGING SECTION TYPE\n" +
            "Select a chapter and click Change Type to reassign it.\n" +
            "The chapter will automatically move to the correct group.",

            // 4. Editor
            "WRITING IN THE EDITOR\n" +
            "======================\n\n" +
            "ENTERING THE EDITOR\n" +
            "Press F2 to focus the editor, or click in the text area.\n" +
            "The chapter title and word count are shown above the editor.\n\n" +
            "DICTATING WITH DRAGON\n" +
            "When Dragon NaturallySpeaking is running, click into the editor and dictate.\n" +
            "Dragon types your spoken words directly into the editor.\n" +
            "This is the normal Dragon dictation experience.\n\n" +
            "LEAVING THE EDITOR\n" +
            "Press Escape — returns focus to the Chapter Manager (Panel 1).\n" +
            "Press F1     — same as Escape, goes to Chapter Manager.\n" +
            "Press F3     — goes to the AI Assistant panel.\n" +
            "Dragon users: say \"Press Escape\" or \"Press F1\" to leave the editor.\n\n" +
            "WORD COUNT\n" +
            "The word count updates as you type and is announced by JAWS.\n\n" +
            "AUTO-SAVE\n" +
            "The app saves your work automatically every 30 seconds.\n" +
            "Press Ctrl+S to save immediately.\n\n" +
            "SELECTING A CHAPTER TO EDIT\n" +
            "Press F1 to go to the Chapter Manager.\n" +
            "Use the arrow keys to select a chapter.\n" +
            "The editor loads that chapter automatically.",

            // 5. AI
            "AI ASSISTANT AND FEEDBACK\n" +
            "==========================\n\n" +
            "REQUIREMENTS\n" +
            "AI features require an Anthropic API key.\n" +
            "Click the AI Not Set button in the toolbar to configure it.\n\n" +
            "FEEDBACK TYPES\n" +
            "Select a chapter in Panel 1, then press F3 to go to the AI panel.\n" +
            "Use the feedback buttons to analyse your chapter:\n\n" +
            "  Comprehensive (Ctrl+F)\n" +
            "    Overview, strengths, quick wins, improvements, and encouragement.\n\n" +
            "  Pacing\n" +
            "    Where your chapter drags or rushes; rhythm and flow.\n\n" +
            "  Dialogue\n" +
            "    Naturalness, character voice, dialogue tags.\n\n" +
            "  Style\n" +
            "    Repeated words, passive voice, weak verbs, clichés.\n\n" +
            "  Structure\n" +
            "    Opening hook, transitions, paragraph purpose, chapter ending.\n\n" +
            "CHAT\n" +
            "Type any question in the Chat input box and press Enter.\n" +
            "Claude has access to your current chapter text as context.\n\n" +
            "INSERTING AI RESPONSES\n" +
            "After Claude responds, use the Insert buttons:\n" +
            "  At Cursor — where your cursor was when you left the editor\n" +
            "  At Start  — beginning of the chapter\n" +
            "  At End    — end of the chapter\n\n" +
            "SAVING RESPONSES AS CARDS\n" +
            "Click Save as Card after any AI response.\n" +
            "Give it a title and category, then use it from the Cards tab later.",

            // 6. Voice Commands
            "VOICE COMMANDS REFERENCE\n" +
            "=========================\n\n" +
            "HOW TO USE VOICE COMMANDS\n" +
            "There are two ways to run voice commands:\n\n" +
            "1. Built-in microphone listener\n" +
            "   Click the Mic: Off button in the toolbar to turn it on.\n" +
            "   Say any command listed below aloud.\n" +
            "   Works without Dragon or JSay.\n\n" +
            "2. Chat input box (works with Dragon and JSay)\n" +
            "   Press F3 to go to the AI panel.\n" +
            "   Type or dictate a command into the chat box.\n" +
            "   Press Enter to run it.\n\n" +
            "NAVIGATION\n" +
            "  panel one / panel two / panel three\n" +
            "  go to chat / chat tab\n" +
            "  open prompt library / show prompts\n" +
            "  open response cards / cards\n\n" +
            "PROJECT\n" +
            "  new project / browse for project\n" +
            "  save / save project / save now\n\n" +
            "CHAPTERS\n" +
            "  add chapter / new chapter\n" +
            "  delete chapter / remove chapter\n" +
            "  rename chapter / rename\n" +
            "  move up / move chapter up\n" +
            "  move down / move chapter down\n" +
            "  change type / change section type\n\n" +
            "AI FEEDBACK\n" +
            "  feedback / comprehensive / run feedback\n" +
            "  pacing / check pacing\n" +
            "  dialogue / check dialogue\n" +
            "  style / check style\n" +
            "  structure / check structure\n" +
            "  send / send message / ask claude\n\n" +
            "INSERT AI RESPONSE\n" +
            "  insert at cursor / insert here\n" +
            "  insert at start / insert at beginning\n" +
            "  insert at end / append response\n\n" +
            "RESPONSE CARDS\n" +
            "  save response card / save card\n" +
            "  insert card one  (or two, three... up to twenty)\n" +
            "  delete card one  (or two, three...)\n" +
            "  show fiction cards / show editing cards / show all cards\n\n" +
            "EXPORT\n" +
            "  export word / export manuscript / export docx\n" +
            "  export PDF / export as PDF / create PDF\n\n" +
            "SETTINGS\n" +
            "  toggle voice / voice on / voice off\n" +
            "  set API key / configure API",

            // 7. Dragon / JSay
            "DRAGON NATURALLYSPEAKING AND JSAY GUIDE\n" +
            "=========================================\n\n" +
            "HOW DRAGON WORKS WITH THIS APP\n" +
            "When Dragon is running, it controls the microphone.\n" +
            "The app's built-in microphone button is automatically disabled.\n\n" +
            "DICTATING IN THE EDITOR\n" +
            "Press F2 or click in the editor.\n" +
            "Dragon switches to dictation mode automatically.\n" +
            "Speak normally — Dragon types your words into the chapter.\n\n" +
            "LEAVING THE EDITOR\n" +
            "Say \"Press Escape\" or \"Press F1\" to go to the Chapter Manager.\n" +
            "Say \"Press F3\" to go to the AI Assistant.\n\n" +
            "CLICKING BUTTONS WITH DRAGON\n" +
            "When focus is in a panel with buttons visible, say:\n" +
            "  \"Click Add chapter\"\n" +
            "  \"Click Save project\"\n" +
            "  \"Click Rename chapter\"\n" +
            "  \"Click Run comprehensive feedback\"\n" +
            "Dragon activates any button by its accessible name.\n\n" +
            "RUNNING VOICE COMMANDS WITH DRAGON\n" +
            "1. Say \"Press F3\" to go to the AI panel.\n" +
            "2. Dragon dictates your command into the Chat box.\n" +
            "3. Say \"Press Enter\" to execute the command.\n\n" +
            "Examples:\n" +
            "  Dictate \"add chapter\" then say \"Press Enter\"\n" +
            "  Dictate \"save project\" then say \"Press Enter\"\n" +
            "  Dictate \"feedback\" then say \"Press Enter\"\n\n" +
            "USING MENUS WITH DRAGON\n" +
            "Say \"Click File\" to open the File menu.\n" +
            "Then say \"Click Save\" or \"Click New Project\".\n\n" +
            "KEYBOARD SHORTCUTS WITH DRAGON\n" +
            "Say the shortcut by name:\n" +
            "  \"Press Control S\"   — Save\n" +
            "  \"Press Control A\"   — Add chapter\n" +
            "  \"Press Control F\"   — AI feedback\n" +
            "  \"Press F1\"          — Chapter Manager\n" +
            "  \"Press F3\"          — AI Assistant\n\n" +
            "JSAY\n" +
            "JSay routes speech to the focused control, similar to Dragon.\n" +
            "Use the same Chat box approach: focus the chat input, JSay dictates " +
            "the command, press Enter to run it.\n" +
            "All keyboard shortcuts work the same way.\n" +
            "Configure JSay application commands to map phrases to F1/F2/F3 keystrokes " +
            "for the fastest panel navigation.",

            // 8. Keyboard Shortcuts
            "KEYBOARD SHORTCUTS\n" +
            "===================\n\n" +
            "FILE\n" +
            "  Ctrl+N           New Project\n" +
            "  Ctrl+O           Open Project\n" +
            "  Ctrl+S           Save\n" +
            "  Ctrl+Shift+S     Save As\n" +
            "  Ctrl+I           Import Word Document (.docx)\n\n" +
            "PANEL NAVIGATION\n" +
            "  F1  or  Ctrl+1   Chapter Manager (Panel 1)\n" +
            "  F2  or  Ctrl+2   Writing Editor  (Panel 2)\n" +
            "  F3  or  Ctrl+3   AI Assistant    (Panel 3)\n" +
            "  Escape           Leave editor, return to Chapter Manager\n\n" +
            "CHAPTERS\n" +
            "  Ctrl+A           Add Chapter\n" +
            "  Ctrl+D           Rename Chapter\n" +
            "  Ctrl+Delete      Delete Chapter\n" +
            "  Alt+Up Arrow     Move Chapter Up\n" +
            "  Alt+Down Arrow   Move Chapter Down\n\n" +
            "AI\n" +
            "  Ctrl+F           Comprehensive AI Feedback\n\n" +
            "TUTORIAL WINDOW\n" +
            "  N                Next step\n" +
            "  P                Previous step\n" +
            "  R                Repeat current step\n" +
            "  Escape           Exit tutorial\n\n" +
            "HELP WINDOW\n" +
            "  1 through 9      Open that help topic\n" +
            "  Escape           Back to topic list or close\n\n" +
            "DRAGON NATURALLYSPEAKING (say these phrases)\n" +
            "  \"Press F1\"             Go to Chapter Manager\n" +
            "  \"Press F3\"             Go to AI Assistant\n" +
            "  \"Press Escape\"         Leave editor\n" +
            "  \"Press Control S\"      Save project\n" +
            "  \"Press Control A\"      Add chapter\n" +
            "  \"Press Control F\"      Run AI feedback\n" +
            "  \"Click [button name]\"  Activate any visible button",

            // 9. Export / Import
            "EXPORTING AND IMPORTING\n" +
            "========================\n\n" +
            "EXPORTING AS WORD DOCUMENT (.docx)\n" +
            "Go to File > Export Manuscript (.docx).\n" +
            "Choose where to save the file.\n" +
            "The export includes all chapters in the correct book order:\n" +
            "front matter first, body chapters in order, then back matter.\n" +
            "Each chapter has a formatted heading and body text.\n\n" +
            "EXPORTING AS PDF\n" +
            "Go to File > Export as PDF.\n" +
            "The PDF includes a title page, running header with book title,\n" +
            "page numbers at the bottom, and all chapters in book order.\n\n" +
            "IMPORTING A WORD DOCUMENT\n" +
            "Go to File > Import .docx.\n" +
            "Choose the Word document to import.\n" +
            "If you have an API key, Claude will detect chapter breaks automatically.\n" +
            "If not, heuristic pattern matching looks for chapter headings.\n" +
            "You can review and approve the detected chapters before importing.\n" +
            "If only one chapter is detected, the whole document imports as one chapter.\n\n" +
            "FILE LOCATIONS\n" +
            "Projects (.vbk):   My Documents\\VoiceBook Projects\\\n" +
            "Response cards:    %AppData%\\VoiceBookStudio\\ResponseCards\\cards.json\n" +
            "Exports:           You choose the location in the Save dialog\n\n" +
            "AUTO-SAVE\n" +
            "The app auto-saves every 30 seconds when there are unsaved changes.\n" +
            "Auto-save is silent — no dialog or sound.\n" +
            "The status bar shows \"Auto-saved\" when it runs.\n" +
            "Press Ctrl+S to save manually at any time."
        };
    }
}
