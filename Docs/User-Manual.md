# VoiceBook Studio — User Manual

Version: Current Release · August 2026
Designed for: JAWS · Dragon NaturallySpeaking · JSay · Windows built-in voice recognition

---

## Contents

1. What VoiceBook Studio Is
2. Starting the App
3. The Four Panels
4. Managing Your Project
5. Writing and Dictating
6. The Whole Book View
7. AI Feedback, Chat, Insert & Replace
8. The Prompt Library
9. Response Cards
10. The Feedback Library
11. Word Count
12. Exporting Your Book
13. Settings
14. How JAWS Works with This App
15. How Dragon Works with This App
16. Voice Command Reference
17. Keyboard Shortcut Reference
18. Sounds Reference
19. Appendix (Section Types, File Locations)

---

## 1. What VoiceBook Studio Is

VoiceBook Studio is a book-writing application built for writers who use assistive technology. Every feature — creating a project, writing chapters, getting AI feedback, saving, and exporting — can be done by voice or keyboard alone. You never need to touch the mouse.

**Two supported modes:**

- **JAWS running:** JAWS reads everything — all controls, all panels, all app announcements, the full tutorial. The app produces no TTS voice of its own so there is never any overlap. JAWS is the sole audio source.
- **JAWS not running:** The app has its own built-in voice that speaks status messages, the tutorial, system announcements, and AI feedback. This works with Dragon alone, with JSay, or with no assistive technology at all.

### The Layout, at a Glance

The main window is divided into four panels plus a status bar. Three panels sit side by side across the top of the window; the AI Assistant is a chat panel that always runs along the bottom, visible no matter which of the other panels has focus.

```
 ┌───────────────┬───────────────────────┬───────────────────┐
 │  PANEL 1       │  PANEL 2              │  PANEL 4          │
 │  Chapter       │  Writing Editor       │  Library          │
 │  Manager       │  (dictate/type here)  │  Prompts · Cards  │
 │  (chapter      │                       │  · Feedback       │
 │  list)         │                       │                   │
 ├───────────────┴───────────────────────┴───────────────────┤
 │  PANEL 3 — AI ASSISTANT (always visible along the bottom)  │
 │  Claude's response  |  Insert/Replace buttons  |  Chat box │
 └──────────────────────────────────────────────────────────┘
                    Status bar (word count, save state)
```

**How it works, end to end:** you pick or create a chapter in Panel 1, write or dictate it in Panel 2, and — whenever you want help — ask Claude a question or request feedback in Panel 3 at the bottom. Claude's reply can be inserted into your chapter or used to directly replace a passage it rewrote. Panel 4 is where you keep reusable material: pre-written prompts to send to Claude, response cards you've saved for later, and a history of every AI feedback report. Nothing here requires the mouse — every panel, button, and action has a keyboard shortcut and a voice command, covered section by section below and summarized in the two reference sections at the end.

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

- **Start Guided Tour** — a 18-step interactive tutorial that walks through every panel, writing your first chapter, getting a response from Claude, using the Prompt Library, and saving a response card
- **Skip Tour** — go straight to the main window (the Welcome dialog will offer the tour again next launch, since it was skipped rather than completed)

The tour can be started again at any time by saying "start tutorial" or from Help → Welcome / Tutorial.

---

## 3. The Four Panels

Switch between panels with keyboard shortcuts or voice commands from anywhere in the app — including while actively dictating into the editor, for the three panels that have a dedicated F-key (see the note at the end of this section).

### Panel 1 — Chapter Manager

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

**Keyboard:** F2 or Ctrl+2 · **Voice:** "panel two" or "go to editor"

The writing area. When a chapter is selected in Panel 1, it opens here for dictation and editing. When "Whole Book" is selected, the editor shows the full manuscript in read-only mode.

The editor uses the same text layer as Microsoft Word, so Dragon NaturallySpeaking works here exactly as it does in Word — dictate naturally, correct with "Correct that," delete with "Scratch that," navigate with all Dragon cursor commands.

**Voice commands in Panel 2:**

| Say | Action |
|---|---|
| Panel two | Focus Panel 2 |
| Go to editor / Open writing editor | Focus Panel 2 |
| Read chapter | Read the full chapter aloud |
| Read paragraph | Read the paragraph at the cursor |
| Read chapter title / Current chapter | Announce the chapter name and its word count |
| Word count / Chapter word count / Book word count | Announce a word count on demand (see Section 11) |
| Stop reading / Stop / Quiet / Silence | Stop speech |

**Keyboard shortcuts:**

| Keys | Action |
|---|---|
| Ctrl+S | Save |
| Ctrl+F | Comprehensive AI feedback on this chapter |
| F4 | Read the current paragraph |
| Ctrl+F4 | Stop reading |
| F5 | Announce the current chapter title and word count |
| Escape | Return to the chapter list (Panel 1) |

---

### Panel 3 — AI Assistant

**Keyboard:** F3 or Ctrl+3 · **Voice:** "panel three" or "go to assistant"

Unlike the other panels, the AI Assistant is not a tab you switch away from — it's a chat panel that runs along the bottom of the window at all times, so Claude's last response and the chat input box are always one command away, whichever of the other panels you're working in.

It has three parts: Claude's response text, the Insert/Replace buttons, and the chat input box.

**Getting a response:**
Type or dictate a question or feedback request into the chat box and say "send" or press Enter. Claude answers using your currently open chapter (or the whole book) as context.

**Using the response — four ways:**

| Say | Action |
|---|---|
| Insert at cursor | Insert the response at your last cursor position in the editor |
| Insert at start | Insert at the beginning of the chapter |
| Insert at end | Insert at the end of the chapter |
| Replace / Replace in chapter | Claude finds the exact original passage the response rewrote and swaps it in directly — no selecting text or copy/pasting |

Replace is the fastest way to act on a targeted rewrite (e.g. "rewrite paragraph 4" or "punch up the opening line") — it finds and replaces that specific passage on its own. If Claude can't confidently identify a single matching passage, it tells you instead of guessing, and you can use one of the Insert buttons.

**Other response actions:**

| Say | Action |
|---|---|
| Read response | Read Claude's response aloud |
| Save card / Save response card | Save the response to the Card Library (Panel 4) |
| Discard response | Remove the response |

**Voice commands to reach Panel 3 itself:**

| Say | Action |
|---|---|
| Panel three | Focus Panel 3 |
| Go to assistant / Open assistant panel / Go to chat / Chat tab | Focus the chat input box |
| Send / Send message / Ask Claude | Send chat input to Claude |
| Ask assistant [your question] | Send a question to Claude in one phrase, without needing to type it into the box first |

---

### Panel 4 — Library

**Keyboard:** F11 or Ctrl+4 · **Voice:** "panel four" or "go to library"

Three tabs, reached either by navigating within Panel 4 once focused, or directly by voice command from anywhere in the app:

| Tab | What it holds | Reach it directly by saying |
|---|---|---|
| Prompts | 81 pre-written writing prompts organised by category — see Section 8 | "Open prompt library" or "Show prompts" |
| Cards | AI responses you've saved to reuse — see Section 9 | "Open response cards" or "Cards" |
| Feedback | Every AI feedback report, saved automatically — see Section 10 | "Open feedback library" |

Saying any of the three commands above switches straight to that tab and moves keyboard focus there, from any panel in the app — you don't need to go to Panel 4 first.

---

### A Note on Panel-Switching While Dictating

F2, F3, and F11 work everywhere — including while your cursor is actively inside the Writing Editor mid-dictation — because they're wired at both the app level and the editor level. Ctrl+1 through Ctrl+4 only work when focus is *outside* the editor (chapter list, chat box, Library panel). This is why Panel 1 (Chapter Manager) has no dedicated F-key of its own: to jump back to it while dictating, press **Escape** instead. Escape is wired specifically to the editor for this purpose — pressed while the Writing Editor has focus, it always jumps straight back to Panel 1. It has no effect in the other panels; from Panel 3 or Panel 4, use Ctrl+1 or say "panel one" instead.

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

Say "import document" or press **Ctrl+I**. Choose a `.docx` file. The app first tries built-in pattern detection (heading styles, chapter breaks) and, only if that finds fewer than two chapters and an API key is set, asks Claude to detect the chapter breaks instead. Either way, it shows you the results for confirmation and creates all chapters. You will hear the chapter count when import is complete.

---

## 5. Writing and Dictating

Select a chapter in Panel 1, then press **F2** or **Ctrl+2** to move to the editor and start dictating.

Dragon NaturallySpeaking works here exactly as in Microsoft Word:

- Dictate text normally
- Say "Correct that" to correct the last dictated text
- Say "Scratch that" to delete the last utterance
- Say "Select [phrase]" to select text
- All Dragon cursor navigation and selection commands work as expected

To hear what you have written, say "read chapter" for the full chapter, or position your cursor and say "read paragraph" for just that section.

Your word count updates on screen as you type, but is not read aloud on every keystroke — see Section 11 for exactly when and how to hear it.

---

## 6. The Whole Book View

The first item in the chapter list is always "Whole Book." Select it to see every chapter combined into a single continuous manuscript in the editor.

The Whole Book view updates automatically as you edit chapters.

When "Whole Book" is selected:

- The editor shows the full manuscript in read-only mode
- The AI assistant uses the entire manuscript as context — all chapters together
- Feedback commands analyse the whole book, not a single chapter
- The word count shown (and spoken right after selection) is the total across every chapter, not any single chapter's count

This is the best way to get feedback on pacing, continuity, character arcs, and how the book holds together as a whole.

---

## 7. AI Feedback, Chat, Insert & Replace

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

Say "book analysis" or "analyse book" from anywhere — you don't need to select "Whole Book" first. Claude always receives the entire manuscript for this command and gives feedback on arc, character consistency, continuity, and book-wide strengths and weaknesses.

### Chat

Type or dictate a question in the chat box (bottom of the window, Panel 3) and say "send" or press Enter. Ask anything — writing questions, character advice, plot help, or a targeted rewrite request — or load a saved prompt from the Library (Section 8).

### Using Claude's Response

See the Panel 3 section above (Section 3) for the full Insert-vs-Replace breakdown. In short:

| Say | Action |
|---|---|
| Read response | Read Claude's response aloud (or press **Space** while focus is on the response text) |
| Stop / Pause | Stop the reading |
| Resume reading | Continue from where reading left off |
| Insert at cursor / Insert at start / Insert at end | Add the response to your chapter at that position |
| Replace / Replace in chapter | Have Claude find the passage the response rewrites and swap it in directly |
| Save card / Save response card | Save the response to the Card Library |
| Discard response | Remove the response |

**Reading a response works even while JAWS is running.** Everywhere else in the app, JAWS is the only voice — but reading a response, a chapter, or a Library entry aloud on request is a deliberate exception: JAWS's own reading commands aren't built for one long, resumable block of app-generated text with real stop/resume, so the app's own voice handles this one thing instead. You'll hear a short "Reading aloud" lead-in so it's always clear which voice is talking; everything else in the app stays JAWS's job as usual.

**While an AI request is in progress**, if it takes more than a few seconds you'll hear a periodic "Still working" reminder so it never reads as the app having frozen.

---

## 8. The Prompt Library

**Reached via Panel 4 — Library, Prompts tab.**

The Prompt Library contains pre-written prompts organised by category. Categories cover editing, fiction, structure, non-fiction, research, description, dialogue, plot, character development, openings and endings, and whole-book feedback.

**Using a prompt:**

- Say "open prompt library" to jump straight to the Prompts tab from anywhere
- Say "prompt categories" or "read prompt categories" to hear every category and how many prompts it has, each labelled with a letter
- Say "read prompt A" (any category letter) to hear all prompts in that category, each labelled like A1, A2, A3
- Say "use prompt A1" (or just "prompt A1") to send that prompt straight to Claude for your open chapter

**Adding a prompt:**

Say "add new prompt" or click Add Prompt in the Prompts tab.

Prompts are shared across all your projects.

---

## 9. Response Cards

**Reached via Panel 4 — Library, Cards tab.**

When you save a Claude response, it becomes a card. Cards let you keep useful responses and insert them into your writing later — in any project, any time.

| Say | Action |
|---|---|
| Open response cards | Jump straight to the Cards tab from anywhere |
| Card categories / What cards do I have | Announce card categories |
| Insert card one (through twenty) | Insert a card by its number in the current filtered list |
| Insert card A1 / Use card A1 | Insert a card by its category letter and number |
| Delete card one (through twenty) | Delete a card |
| Show [category] cards | Filter the card list to a category, e.g. "show fiction cards" |

---

## 10. The Feedback Library

**Reached via Panel 4 — Library, Feedback tab.**

Every AI analysis is saved automatically to the Feedback Library. You can re-read any previous feedback at any time.

| Say | Action |
|---|---|
| Open feedback library | Jump straight to the Feedback tab from anywhere |
| Feedback categories / What's in my feedback library | Hear the saved feedback categories |
| Read my pacing feedback | Read all saved pacing entries |
| Read my dialogue feedback | Read saved dialogue entries |
| Read my style feedback | Read saved style entries |
| Read my structure feedback | Read saved structure entries |
| Read my comprehensive feedback | Read comprehensive entries |
| Resume reading | Continue reading where you left off |
| Delete feedback entry | Delete the entry currently selected in the Feedback tab |

---

## 11. Word Count

Word count is tracked for both the current chapter and the whole book, and is deliberately **not** spoken on every keystroke — only:

- **Automatically**, right after the chapter title when you select a chapter (e.g. "Chapter loaded: The Storm. 1,204 words.")
- **Automatically**, right after selecting Whole Book (e.g. "Whole Book. Read only. 42,318 words.")
- **On request**, with any of the commands below

| Say | Action |
|---|---|
| Word count / How many words | Speak the word count for whatever is currently open — the chapter, or Whole Book |
| Chapter word count | Speak the currently open chapter's word count specifically, regardless of what's selected |
| Book word count / Whole book word count | Speak the total word count across the whole manuscript, regardless of what's selected |
| Read chapter title / Current chapter | Announce the chapter name together with its word count in one line |

The word count is also always visible on screen, in the editor panel header and the status bar, updating live as you type.

---

## 12. Exporting Your Book

| Say | Action |
|---|---|
| Export Word / Export manuscript | Export as a formatted `.docx` file |
| Export PDF / Create PDF | Export as a PDF with title page and page numbers |

---

## 13. Settings

Say "open settings" or press the settings button in the toolbar.

### API Key

Say "set API key" to enter your Anthropic API key. Required for all AI features. The key is stored locally (encrypted) and sent only to Anthropic's servers.

### Default Project Folder

Say "set project folder" to choose where new projects are saved. Leave blank to be asked each time.

### This Project's Save Folder

While a project is open, Settings has a second folder option just for it — an override that points that specific project's Open and Save As dialogs at a different folder than the default above (useful if that book lives in a synced or shared folder). Leave it blank to fall back to the default project folder, or wherever the project's file already lives.

### Default Export Folder

Say "set export folder" or set it from Settings to choose where Export Word and Export PDF save to by default. Leave blank to use whatever folder Windows last remembers.

### Configure Voice (Azure TTS)

Say "configure voice" or choose Settings → Configure Voice from the menu to set up Azure Neural TTS for a more natural reading voice. Azure is optional — the app uses the best available Windows voice by default.

---

## 14. How JAWS Works with This App

### What JAWS reads — everything

When JAWS is running, it is the sole audio source for the entire application:

- All controls, buttons, input fields, and list items have `AutomationProperties.Name` values that match their visible labels exactly, so JAWS reads the right text when you tab to or click any control
- The chapter list, AI response areas, word count, and status bar are all UIA live regions — JAWS announces changes automatically without you having to navigate there. Word count is a deliberate exception: it does **not** auto-announce on every keystroke (see Section 11) so JAWS doesn't talk over you while you type
- Every dialog (Welcome, Settings, API Key, Azure TTS, Add Prompt, Save Card) has an assertive live region that fires when the dialog opens, so JAWS announces the dialog name and purpose immediately
- Tutorial steps: when you press Next, Previous, or Repeat, JAWS reads the full title and content of each step immediately via `RaiseNotificationEvent` — the most reliable JAWS announcement path
- System events (chapter added, chapter moved, save confirmed, AI complete, errors) are routed through the same UIA notification system — JAWS announces them as they happen
- Voice-triggered panel and tab switches (e.g. "open prompt library") move keyboard focus along with the visible change, so JAWS follows correctly rather than continuing to read whatever had focus before

### What the app does not do when JAWS is running

The app produces **no SAPI voice at all**. Both the general feedback service and the system announcement service are fully silenced at startup when JAWS is detected. There is no overlap and no double-reading under any circumstances.

### No JAWS configuration needed

JAWS works with VoiceBook Studio out of the box. No JAWS scripts, no custom configuration.

**One recommended JAWS setting:** Make sure JAWS output is on the same audio device as Windows default playback (same headset or speakers). This is standard practice and unrelated to this app specifically.

---

## 15. How Dragon Works with This App

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

- On most keyboards, pressing ScrollLock also lights the ScrollLock LED as a Windows side-effect of the key itself — a handy physical indicator, though the app doesn't control the light directly
- Works from anywhere in the app — chapter list, editor, any panel, inside the tutorial
- No Dragon setup required

**Example — getting pacing feedback then back to dictation:**
1. Press ScrollLock — app mic on, Dragon muted
2. Say "pacing feedback"
3. Press ScrollLock — Dragon mic restored
4. Dictate your next paragraph as normal

---

**Option 2 — Command bar (works immediately, no setup)**

Press **Ctrl+Shift+Space** or say "press Control Shift Space" to open the command bar (the chat input box in Panel 3, at the bottom of the window). Type or dictate the command and press Enter.

**Example:** Press Ctrl+Shift+Space → dictate "panel four" → press Enter

This works for every command in the voice command list. It is slower than ScrollLock but requires no setup at all.

---

**Option 3 — Dragon MyCommands (full hands-free, requires one-time setup)**

Create Dragon MyCommands that map spoken phrases to keyboard shortcuts or command bar sequences. After setup, you can say commands like "pacing feedback" or "panel two" directly without pressing ScrollLock or using the command bar.

**Button clicking with Dragon:** WPF buttons in VoiceBook Studio are not standard Win32 controls, so Dragon's built-in "click [button name]" requires the app's `AutomationProperties.Name` to be set correctly — which it is, on every button — but still benefits from a one-time Dragon MyCommands setup for the smoothest experience. To click buttons by voice in Dragon, either rely on Dragon's own UI Automation "click" support against the named control, or create MyCommands that send the corresponding keyboard shortcut or command bar sequence. See `Dragon-Commands-Setup-Guide.md` and `Docs/Dragon_Commands_VoiceBook.xml` for the full list of commands and setup instructions.

---

## 16. Voice Command Reference

### Navigation

| Say | Action |
|---|---|
| Panel one / two / three / four | Switch panels |
| Go to chapters / Go to chapter list | Panel 1 |
| Go to editor / Open writing editor | Panel 2 |
| Go to assistant / Open assistant panel / Go to chat / Chat tab | Panel 3 (chat input) |
| Go to library / Open library / Library panel | Panel 4 |
| Open prompt library | Library panel, Prompts tab |
| Open response cards | Library panel, Cards tab |
| Open feedback library | Library panel, Feedback tab |
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
| Open chapter / Show chapter / Select chapter | Focus the chapter list and hear all chapter names — then say "click [chapter name]" to open one |

### Reading Aloud & Word Count

| Say | Action |
|---|---|
| Read chapter | Read the full chapter |
| Read paragraph | Read the paragraph at the cursor |
| Read chapter title / Current chapter | Announce the chapter name and its word count |
| Word count / How many words | Announce the word count for whatever is currently open |
| Chapter word count | Announce the current chapter's word count specifically |
| Book word count / Whole book word count | Announce the total manuscript word count |
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
| Ask assistant [your question] | Send a question straight to Claude in one phrase |
| Read response | Read Claude's response aloud (or press Space with focus on the response) |
| Stop / Pause | Stop reading |
| Resume reading | Continue reading from where it left off |
| Insert at cursor | Insert response at cursor position |
| Insert at start | Insert response at chapter beginning |
| Insert at end | Insert response at chapter end |
| Replace / Replace in chapter / Replace passage | Have Claude find and replace the passage the response rewrites |
| Save card / Save response card | Save response as a card |
| Discard response / Clear response | Remove the response |

### Prompt Library

| Say | Action |
|---|---|
| Open prompt library | Jump to Prompts tab (Panel 4) |
| Prompt categories / What prompts do I have | Hear categories |
| Read prompt A (through K) | Hear all prompts in a category |
| Use prompt A1 / Prompt A1 | Load and send a specific prompt (letter, then number) |
| Add new prompt | Add a prompt |

### Response Cards

| Say | Action |
|---|---|
| Open response cards | Jump to Cards tab (Panel 4) |
| Card categories / What cards do I have | Hear card categories |
| Insert card one (through twenty) | Insert a card by number |
| Insert card A1 / Use card A1 | Insert a card by category letter and number |
| Delete card one (through twenty) | Delete a card |
| Show [category] cards | Filter the card list to a category, e.g. "show fiction cards" |

### Feedback Library

| Say | Action |
|---|---|
| Open feedback library | Jump to Feedback tab (Panel 4) |
| Feedback categories | Hear categories |
| Read my pacing feedback | Read saved pacing entries |
| Read my dialogue feedback | Read saved dialogue entries |
| Read my style feedback | Read saved style entries |
| Read my structure feedback | Read saved structure entries |
| Read my comprehensive feedback | Read comprehensive entries |
| Resume reading | Continue reading where you left off |
| Delete feedback entry | Delete the entry currently selected in the Feedback tab |

### Settings and Help

| Say | Action |
|---|---|
| Set API key | Enter Anthropic API key |
| Open settings | Open settings dialog |
| Set project folder | Choose default project folder |
| Set export folder | Choose default folder for exported Word/PDF files |
| Toggle voice | Toggle app TTS on or off (not relevant when JAWS is running) |
| Configure voice | Open the Configure Voice (Azure TTS) dialog |
| Start tutorial | Open the 18-step guided tutorial |
| Open welcome | Reopen the Welcome dialog (Start Guided Tour / Skip Tour) |

### App

| Say | Action |
|---|---|
| Close VoiceBook / Exit VoiceBook | Close the application |

---

## 17. Keyboard Shortcut Reference

| Keys | Action | Works while dictating in the editor? |
|---|---|---|
| Ctrl+1 | Panel 1 — Chapter Manager | No — press Escape instead |
| F2 or Ctrl+2 | Panel 2 — Writing Editor | Yes (F2) |
| F3 or Ctrl+3 | Panel 3 — AI Assistant | Yes (F3) |
| F11 or Ctrl+4 | Panel 4 — Library | Yes (F11) |
| Escape | Return to Panel 1 (Chapter Manager) | Yes |
| Ctrl+N | New project | — |
| Ctrl+O | Open project | — |
| Ctrl+S | Save | Yes |
| Ctrl+Shift+S | Save As | — |
| Ctrl+I | Import document | — |
| Ctrl+A | Add chapter | — |
| Ctrl+D | Rename chapter | — |
| Ctrl+Delete | Delete chapter | — |
| Alt+Up | Move chapter up | — |
| Alt+Down | Move chapter down | — |
| Ctrl+F | Comprehensive AI feedback | — |
| F4 | Read current paragraph | Yes |
| Ctrl+F4 | Stop reading | Yes |
| F5 | Announce current chapter title and word count | Yes |
| F6 | Next chapter | — |
| F7 | Previous chapter | — |
| F8 | Read full chapter aloud | — |
| F9 | Announce application status | — |
| ScrollLock | Toggle app microphone on/off (mutes/unmutes Dragon simultaneously) | Yes |
| Ctrl+Shift+Space | Open command bar (focus the chat input) | — |

**Note:** Ctrl+1 through Ctrl+4 only work when keyboard focus is outside the Writing Editor. F2, F3, F11, Escape, F4, Ctrl+F4, F5, and ScrollLock all work from inside the editor too — see the note at the end of Section 3 for why.

F1 is intentionally left unbound — it's reserved for JAWS's own contextual help on whatever control has focus. F10 is also left unbound, since WPF reserves it to activate the menu bar.

---

## 18. Sounds Reference

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
| Soft tick | Text inserted or replaced |
| Bell | Export complete |
| Error tone | Export failed |
| Click | Voice command recognised |
| Low beep | Voice command not recognised |
| Step chime | Tutorial step advanced |
| Fanfare | Tutorial complete |

---

## 19. Appendix

### Section Types Reference

Every chapter or section you add is one of 14 types, grouped into front matter, body, and back matter. The section group order (front → body → back) is always preserved regardless of how you reorder within a group.

| Section type | Group | Typical purpose |
|---|---|---|
| Title Page | Front matter | Book title, author, series info |
| Copyright | Front matter | Legal/copyright notice |
| Dedication | Front matter | Dedication to person or persons |
| Epigraph | Front matter | Opening quote |
| Table of Contents | Front matter | Chapter listing |
| Foreword | Front matter | Introductory note by another author |
| Preface | Front matter | Author's note on the book's origins |
| Introduction | Front matter | Introduction to content |
| Prologue | Front matter | Scene-setting narrative before Chapter 1 |
| Chapter | Body | Main narrative chapters |
| Epilogue | Back matter | Narrative scene after the story ends |
| Afterword | Back matter | Author's reflection after the story |
| Appendix | Back matter | Supplementary material |
| About the Author | Back matter | Author biography |

### File Locations

| Item | Location |
|---|---|
| Project files | Wherever you saved them (`.vbk`) |
| Response cards | `%APPDATA%\VoiceBookStudio\ResponseCards\cards.json` |
| Saved AI feedback | `%APPDATA%\VoiceBookStudio\Feedback\feedback.json` |
| Chat history | `%APPDATA%\VoiceBookStudio\ChatHistory\history.json` |
| App settings (default project folder, first-launch state) | `%APPDATA%\VoiceBookStudio\settings.json` |
| Anthropic API key | `HKEY_CURRENT_USER\SOFTWARE\VoiceBookStudio` (Windows Registry, encrypted at rest with DPAPI — not a plain file, not the Windows Credential Store) |
| Writing prompts (shipped with the app) | `Data\PromptLibrary\prompts.json` in the install folder |

---

*Say "what can I say here" at any time to hear available commands for the current panel. Say "start tutorial" to open the guided tour. For a condensed one-page reference, see `Docs/Cheat-Sheet.md`. For a deeper JAWS settings and keystroke walkthrough, see `Docs/JAWS-Settings-Guide.md`.*
