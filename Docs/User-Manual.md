# VoiceBook Studio — User Manual

Version: Current Release · June 2026
Designed for: JAWS · Dragon NaturallySpeaking · JSay · Windows built-in voice recognition

---

## Contents

1. What VoiceBook Studio Is
2. Starting the App
3. The Three Panels
4. Managing Your Project
5. Writing and Dictating
6. The Whole Book View
7. AI Feedback
8. The Prompt Library
9. Response Cards
10. The Feedback Library
11. Exporting Your Book
12. Settings
13. How JAWS Works with This App
14. How Dragon Works with This App
15. Voice Command Reference
16. Keyboard Shortcut Reference
17. Sounds Reference

---

## 1. What VoiceBook Studio Is

VoiceBook Studio is a book-writing application built for writers who use assistive technology. Every feature — creating a project, writing chapters, getting AI feedback, saving, and exporting — can be done by voice or keyboard alone. You never need to touch the mouse.

**Two supported modes:**

- **JAWS running:** JAWS reads everything — all controls, all panels, all app announcements, the full tutorial. The app produces no TTS voice of its own so there is never any overlap. JAWS is the sole audio source.
- **JAWS not running:** The app has its own built-in voice that speaks status messages, the tutorial, system announcements, and AI feedback. This works with Dragon alone, with JSay, or with no assistive technology at all.

---

## 2. Starting the App

### When JAWS Is Running

The main window opens and JAWS reads it naturally — window title, focused control, status. No startup announcement plays from the app. JAWS is the only voice.

A Welcome dialog opens on first launch (see below).

### When JAWS Is Not Running

The app speaks one startup announcement:

- If Dragon is detected: *"Dragon NaturallySpeaking is running. Microphone is controlled by Dragon. Use ScrollLock to toggle voice commands. VoiceBook Studio is ready."*
- If Dragon is not detected: *"Built-in voice recognition is active. Say a command at any time. VoiceBook Studio is ready."*

### When You Close the App

- A soft falling chime plays
- The app says *"VoiceBook Studio closing. Goodbye."* (plays only when JAWS is not running)

### First Launch — Welcome Dialog

On first launch, a Welcome dialog opens automatically. It speaks a greeting that reflects what was actually detected:

- If Dragon is running: the greeting describes how to give voice commands with Dragon
- If Dragon is not running: the greeting describes the built-in voice recognition

Two choices:

- **Start Guided Tour** — a 17-step interactive tutorial that walks through every feature
- **Skip Tour** — go straight to the main window

The tour can be started again at any time by saying "start tutorial" or from Help → Welcome / Tutorial.

---

## 3. The Three Panels

The main window has three panels side by side. Switch between them with keyboard shortcuts or voice commands.

### Panel 1 — Chapters

**Keyboard:** Ctrl+1 · **Voice:** "panel one" or "go to chapters"

The chapter list. At the top is always **"Whole Book"** — a read-only view of all chapters combined. Below it are your chapters and sections in document order.

**Buttons in Panel 1 (Dragon users: say "click [button name]" after setting up MyCommands):**

| Button | What it does |
|---|---|
| Add Chapter | Adds a new chapter |
| Rename | Renames the selected chapter |
| Delete | Deletes the selected chapter |
| Change Type | Changes the section type (Chapter, Prologue, Dedication, Appendix, etc.) |
| Move Chapter Up | Moves the selected chapter earlier in the book |
| Move Chapter Down | Moves the selected chapter later in the book |
| Previous Chapter | Selects the previous chapter in the list |
| Next Chapter | Selects the next chapter in the list |

**Voice commands in Panel 1:**

| Say | Action |
|---|---|
| Panel one | Focus Panel 1 |
| Go to chapters / Go to chapter list | Focus Panel 1 |
| Add chapter / New chapter | Add a chapter |
| Rename chapter | Rename selected chapter |
| Delete chapter | Delete selected chapter |
| Change type | Change section type |
| Move up | Move selected chapter up |
| Move down | Move selected chapter down |
| Next chapter | Select the next chapter |
| Previous chapter | Select the previous chapter |

**Keyboard shortcuts:**

| Keys | Action |
|---|---|
| Ctrl+A | Add chapter |
| Ctrl+D | Rename chapter |
| Ctrl+Delete | Delete chapter |
| Alt+Up | Move chapter up |
| Alt+Down | Move chapter down |
| F6 | Next chapter |
| F7 | Previous chapter |

---

### Panel 2 — Writing Editor

**Keyboard:** Ctrl+2 · **Voice:** "panel two" or "go to editor"

The writing area. When a chapter is selected in Panel 1, it opens here for dictation and editing. When "Whole Book" is selected, the editor shows the full manuscript in read-only mode.

The editor uses the same text layer as Microsoft Word, so Dragon NaturallySpeaking works here exactly as it does in Word — dictate naturally, correct with "Correct that," delete with "Scratch that," navigate with all Dragon cursor commands.

**Voice commands in Panel 2:**

| Say | Action |
|---|---|
| Panel two | Focus Panel 2 |
| Go to editor / Open writing editor | Focus Panel 2 |
| Read chapter | Read the full chapter aloud |
| Read paragraph | Read the paragraph at the cursor |
| Read chapter title / Current chapter | Announce the chapter name |
| Stop reading / Stop / Quiet / Silence | Stop speech |

**Keyboard shortcuts:**

| Keys | Action |
|---|---|
| Ctrl+S | Save |
| Ctrl+F | Comprehensive AI feedback on this chapter |
| F4 | Read the current paragraph |
| F5 | Announce the current chapter title |
| F8 | Read the full chapter aloud |
| Escape | Return to the chapter list |

---

### Panel 3 — AI Assistant

**Keyboard:** Ctrl+3 · **Voice:** "panel three" or "go to assistant"

Four tabs: Chat, Prompts, Cards, Feedback.

- **Chat** — ask Claude questions, request feedback, send prompts
- **Prompts** — browse and load saved writing prompts
- **Cards** — browse and insert saved response cards
- **Feedback** — read saved AI feedback entries

**Voice commands in Panel 3:**

| Say | Action |
|---|---|
| Panel three | Focus Panel 3 |
| Go to assistant / Open assistant panel | Focus Panel 3 |
| Go to chat | Switch to the Chat tab |
| Open prompt library | Switch to the Prompts tab |
| Open response cards | Switch to the Cards tab |
| Open feedback library | Switch to the Feedback tab |
| Send / Send message / Ask Claude | Send chat input to Claude |

---

## 4. Managing Your Project

### Create a New Project

Say "new project" or press **Ctrl+N**. Enter a title. The project saves to your default folder automatically.

### Open a Project

Say "open project" or press **Ctrl+O** to browse for a `.vbk` file.

### Save

Say "save" or press **Ctrl+S**. You will hear "Project saved" (or JAWS announces the status bar update) when complete.

### Save As

Say "save as" or press **Ctrl+Shift+S**. Choose a new file name or location.

### Import a Word Document

Say "import document" or press **Ctrl+I**. Choose a `.docx` file. The app reads the document, detects chapter breaks automatically using Claude (if an API key is set) or built-in pattern detection, shows you the results for confirmation, and creates all chapters. You will hear the chapter count when import is complete.

---

## 5. Writing and Dictating

Select a chapter in Panel 1, then press **Ctrl+2** to move to the editor and start dictating.

Dragon NaturallySpeaking works here exactly as in Microsoft Word:

- Dictate text normally
- Say "Correct that" to correct the last dictated text
- Say "Scratch that" to delete the last utterance
- Say "Select [phrase]" to select text
- All Dragon cursor navigation and selection commands work as expected

To hear what you have written, say "read chapter" for the full chapter, or position your cursor and say "read paragraph" for just that section.

---

## 6. The Whole Book View

The first item in the chapter list is always "Whole Book." Select it to see every chapter combined into a single continuous manuscript in the editor.

The Whole Book view updates automatically as you edit chapters.

When "Whole Book" is selected:

- The editor shows the full manuscript in read-only mode
- The AI assistant uses the entire manuscript as context — all chapters together
- Feedback commands analyse the whole book, not a single chapter

This is the best way to get feedback on pacing, continuity, character arcs, and how the book holds together as a whole.

---

## 7. AI Feedback

AI features require an Anthropic API key. Set it by saying "set API key" or clicking the key icon in the toolbar.

### Chapter Feedback

Select a chapter in Panel 1, then say any of these:

| Say | What Claude analyses |
|---|---|
| Feedback / Comprehensive | Overall: pacing, dialogue, style, structure |
| Pacing | Where the chapter drags or rushes |
| Dialogue | Naturalness, character voice, dialogue tags |
| Style | Prose, word repetition, passive voice |
| Structure | Hook, transitions, chapter ending |

### Whole Book Analysis

Select "Whole Book" in Panel 1, then say "book analysis" or "analyse book." Claude receives the entire manuscript and gives feedback on arc, character consistency, continuity, and book-wide strengths and weaknesses.

### Chat

Type or dictate a question in the chat box and say "send" or press Enter. Ask anything — writing questions, character advice, plot help — or load a saved prompt.

### Using Claude's Response

After Claude responds:

| Say | Action |
|---|---|
| Read response | Read Claude's response aloud |
| Insert at cursor | Insert the response at your cursor position |
| Insert at start | Insert at the beginning of the chapter |
| Insert at end | Insert at the end of the chapter |
| Save card / Save response card | Save the response to the Card Library |
| Discard response | Remove the response |

---

## 8. The Prompt Library

The Prompt Library contains pre-written prompts organised by category. Categories cover editing, fiction, structure, non-fiction, research, description, dialogue, plot, character development, openings and endings, and whole-book feedback.

**Using a prompt:**

- Say "open prompt library" to browse the Prompts tab
- Say "use prompt A one" (letter A–K, number one–ten) to load a specific prompt into the chat box
- Say "read prompt A" to hear all prompts in category A
- Say "what prompts do I have" to hear the category list

**Adding a prompt:**

Say "add new prompt" or click Add Prompt in the Prompts tab.

Prompts are shared across all your projects.

---

## 9. Response Cards

When you save a Claude response, it becomes a card. Cards let you keep useful responses and insert them into your writing later.

| Say | Action |
|---|---|
| Open response cards | Switch to the Cards tab |
| What cards do I have | Announce card categories |
| Insert card one | Insert card 1 into the chapter (one through twenty) |
| Delete card one | Delete card 1 (one through five) |

---

## 10. The Feedback Library

Every AI analysis is saved automatically to the Feedback Library. You can re-read any previous feedback at any time.

| Say | Action |
|---|---|
| Open feedback library | Switch to the Feedback tab |
| Feedback categories | Hear the saved feedback categories |
| Read my pacing feedback | Read all saved pacing entries |
| Read my dialogue feedback | Read saved dialogue entries |
| Read my style feedback | Read saved style entries |
| Read my structure feedback | Read saved structure entries |
| Read my comprehensive feedback | Read comprehensive entries |
| Resume reading | Continue reading where you left off |

---

## 11. Exporting Your Book

| Say | Action |
|---|---|
| Export Word / Export manuscript | Export as a formatted `.docx` file |
| Export PDF / Create PDF | Export as a PDF with title page and page numbers |

---

## 12. Settings

Say "open settings" or press the settings button in the toolbar.

### API Key

Say "set API key" to enter your Anthropic API key. Required for all AI features. The key is stored locally and sent only to Anthropic's servers.

### Default Project Folder

Say "set project folder" to choose where new projects are saved. Leave blank to be asked each time.

### Configure Voice (Azure TTS)

Click the voice button in the toolbar to set up Azure Neural TTS for a more natural reading voice. Azure is optional — the app uses the best available Windows voice by default.

---

## 13. How JAWS Works with This App

### What JAWS reads — everything

When JAWS is running, it is the sole audio source for the entire application:

- All controls, buttons, input fields, and list items have `AutomationProperties.Name` values that match their visible labels exactly, so JAWS reads the right text when you tab to or click any control
- The chapter list, AI response areas, and status bar are all UIA live regions — JAWS announces changes automatically without you having to navigate there
- Every dialog (Welcome, Settings, API Key, Azure TTS, Add Prompt, Project Selection) has an assertive live region that fires when the dialog opens, so JAWS announces the dialog name and purpose immediately
- Tutorial steps: when you press Next, Previous, or Repeat, JAWS reads the full title and content of each step immediately via `RaiseNotificationEvent` — the most reliable JAWS announcement path
- System events (chapter added, chapter moved, save confirmed, AI complete, errors) are routed through the same UIA notification system — JAWS announces them as they happen

### What the app does not do when JAWS is running

The app produces **no SAPI voice at all**. Both the general feedback service and the system announcement service are fully silenced at startup when JAWS is detected. There is no overlap and no double-reading under any circumstances.

### No JAWS configuration needed

JAWS works with VoiceBook Studio out of the box. No JAWS scripts, no custom configuration.

**One recommended JAWS setting:** Make sure JAWS output is on the same audio device as Windows default playback (same headset or speakers). This is standard practice and unrelated to this app specifically.

---

## 14. How Dragon Works with This App

### Dictation — works immediately, no setup needed

The writing editor (Panel 2) is built on WinForms RichTextBox, which uses the Win32 Text Services Framework — the same layer as Microsoft Word. Dragon dictates into it identically to Word:

- Dictate naturally
- "Correct that" corrects the last dictation
- "Scratch that" deletes the last utterance
- "Select [phrase]" selects text
- All Dragon cursor navigation commands work

No Dragon configuration is needed for dictation.

### App commands with Dragon

When Dragon is running, it owns the microphone and the app's built-in voice recogniser is disabled. For app-level commands (panel switching, save, chapter management, AI requests) you have three options:

---

**Option 1 — ScrollLock toggle (recommended, fastest)**

Press **ScrollLock** once. Dragon's microphone is muted and the app's built-in recogniser activates. Say any command from the voice command list. Press **ScrollLock** again to return the microphone to Dragon.

- The ScrollLock LED on most keyboards lights when the app mic is on, giving you a physical indicator
- Works from anywhere in the app — chapter list, editor, any panel, inside the tutorial
- No Dragon setup required

**Example — getting pacing feedback then back to dictation:**
1. Press ScrollLock — app mic on, Dragon muted
2. Say "pacing feedback"
3. Press ScrollLock — Dragon mic restored
4. Dictate your next paragraph as normal

---

**Option 2 — Command bar (works immediately, no setup)**

Press **Ctrl+Shift+Space** or say "press Control Shift Space" to open the command bar (the chat input box in Panel 3). Type or dictate the command and press Enter.

**Example:** Press Ctrl+Shift+Space → dictate "panel two" → press Enter

This works for every command in the voice command list. It is slower than ScrollLock but requires no setup at all.

---

**Option 3 — Dragon MyCommands (full hands-free, requires one-time setup)**

Create Dragon MyCommands that map spoken phrases to keyboard shortcuts or command bar sequences. After setup, you can say commands like "pacing feedback" or "panel two" directly without pressing ScrollLock or using the command bar.

**Button clicking with Dragon:** WPF buttons in VoiceBook Studio are not standard Win32 controls, so Dragon's built-in "click [button name]" does not work with them. To click buttons by voice in Dragon, create MyCommands that send the corresponding keyboard shortcut or command bar sequence. See `Dragon-Commands-Setup-Guide.md` for the full list of commands and setup instructions.

---

## 15. Voice Command Reference

### Navigation

| Say | Action |
|---|---|
| Panel one / two / three | Switch panels |
| Go to chapters / Go to chapter list | Panel 1 |
| Go to editor / Open writing editor | Panel 2 |
| Go to assistant / Open assistant panel | Panel 3 |
| Go to chat | Chat tab |
| Open prompt library | Prompts tab |
| Open response cards | Cards tab |
| Open feedback library | Feedback tab |
| What can I say here | Hear available commands for the current panel |
| Application status / Status | Announce current app state |

### Project

| Say | Action |
|---|---|
| New project | Create a new project |
| Open project | Browse for a project file |
| Save / Save project / Save now | Save |
| Save as | Save to a new file |
| Import document / Import Word document | Import a .docx file |
| Export Word / Export manuscript | Export to Word |
| Export PDF / Create PDF | Export to PDF |

### Chapters

| Say | Action |
|---|---|
| Add chapter / New chapter | Add a chapter |
| Rename chapter | Rename selected chapter |
| Delete chapter | Delete selected chapter |
| Move up | Move chapter up |
| Move down | Move chapter down |
| Change type | Change section type |
| Next chapter | Select the next chapter |
| Previous chapter | Select the previous chapter |

### Reading Aloud

| Say | Action |
|---|---|
| Read chapter | Read the full chapter |
| Read paragraph | Read the paragraph at the cursor |
| Read chapter title / Current chapter | Announce the chapter name |
| Stop reading / Stop / Quiet / Silence | Stop speech |

### AI

| Say | Action |
|---|---|
| Feedback / Comprehensive | Comprehensive chapter feedback |
| Pacing | Pacing analysis |
| Dialogue | Dialogue analysis |
| Style | Style analysis |
| Structure | Structure analysis |
| Book analysis / Whole book / Analyse book | Full manuscript analysis |
| Send / Send message / Ask Claude | Send chat input |
| Read response | Read Claude's response aloud |
| Insert at cursor | Insert response at cursor position |
| Insert at start | Insert at chapter beginning |
| Insert at end | Insert at chapter end |
| Save card / Save response card | Save response as a card |
| Discard response / Clear response | Remove the response |

### Prompt Library

| Say | Action |
|---|---|
| Open prompt library | Open Prompts tab |
| What prompts do I have | Hear categories |
| Read prompt A (through J) | Hear all prompts in a category |
| Use prompt A one | Load a specific prompt (letter A–K, number one–ten) |
| Add new prompt | Add a prompt |

### Response Cards

| Say | Action |
|---|---|
| Open response cards | Open Cards tab |
| What cards do I have | Hear card categories |
| Insert card one (through twenty) | Insert a card |
| Delete card one (through five) | Delete a card |

### Feedback Library

| Say | Action |
|---|---|
| Open feedback library | Open Feedback tab |
| Feedback categories | Hear categories |
| Read my pacing feedback | Read saved pacing entries |
| Read my dialogue feedback | Read saved dialogue entries |
| Read my style feedback | Read saved style entries |
| Read my structure feedback | Read saved structure entries |
| Read my comprehensive feedback | Read comprehensive entries |
| Resume reading | Continue reading where you left off |

### Settings and Help

| Say | Action |
|---|---|
| Set API key | Enter Anthropic API key |
| Open settings | Open settings dialog |
| Set project folder | Choose default save folder |
| Toggle voice | Toggle app TTS on or off (not relevant when JAWS is running) |
| Start tutorial | Open the 17-step guided tutorial |

### App

| Say | Action |
|---|---|
| Close VoiceBook / Exit VoiceBook | Close the application |

---

## 16. Keyboard Shortcut Reference

| Keys | Action |
|---|---|
| Ctrl+1 | Panel 1 — Chapters |
| Ctrl+2 | Panel 2 — Writing Editor |
| Ctrl+3 | Panel 3 — AI Assistant |
| Ctrl+N | New project |
| Ctrl+O | Open project |
| Ctrl+S | Save |
| Ctrl+Shift+S | Save As |
| Ctrl+I | Import document |
| Ctrl+A | Add chapter |
| Ctrl+D | Rename chapter |
| Ctrl+Delete | Delete chapter |
| Alt+Up | Move chapter up |
| Alt+Down | Move chapter down |
| Ctrl+F | Comprehensive AI feedback |
| F4 | Read current paragraph |
| F5 | Announce current chapter title |
| F6 | Next chapter |
| F7 | Previous chapter |
| F8 | Read full chapter aloud |
| F9 | Announce application status |
| ScrollLock | Toggle app microphone on/off (mutes/unmutes Dragon simultaneously) |
| Ctrl+Shift+Space | Open command bar |
| Escape | Leave editor, return to chapter list |

---

## 17. Sounds Reference

| Sound | Meaning |
|---|---|
| Rising chime | App ready |
| Falling chime | App closing |
| Soft pop | Project opened |
| Soft click | Project saved / auto-saved |
| Ascending tone | Chapter added |
| Descending tone | Chapter deleted |
| Shuffle tone | Chapter moved |
| Warm tone | AI responded |
| Alert tone | AI error |
| Soft tick | Text inserted |
| Bell | Export complete |
| Error tone | Export failed |
| Click | Voice command recognised |
| Low beep | Voice command not recognised |
| Step chime | Tutorial step advanced |
| Fanfare | Tutorial complete |

---

*Say "what can I say here" at any time to hear available commands for the current panel. Say "start tutorial" to open the guided tour.*
