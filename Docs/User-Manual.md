# VoiceBook Studio — User Manual

Version: Current Release · June 2026

---

## Contents

1. Starting the App
2. The Three Panels
3. Managing Your Project
4. Writing and Dictating
5. The Whole Book View
6. AI Feedback
7. The Prompt Library
8. Response Cards
9. The Feedback Library
10. Exporting Your Book
11. Settings
12. All Voice Commands
13. All Keyboard Shortcuts
14. Sounds Reference

---

## 1. Starting the App

When VoiceBook Studio opens, it speaks these announcements in order:

1. Which accessibility tools were detected — JAWS, Dragon, JSay
2. Microphone status — whether the built-in voice recogniser started or Dragon owns the mic
3. "VoiceBook Studio ready. Focus is on the chapter list."

When you close the app:

- A soft falling chime plays
- The app says "VoiceBook Studio closing. Goodbye."

These announcements always play regardless of which tools are running.

### First Launch

On first launch, a Welcome dialog opens and speaks a greeting. You have two choices:

- **Start Guided Tour** — a 17-step interactive tutorial that walks through every feature
- **Skip Tour** — go straight to the main window

The tour can be started again at any time by saying "start tutorial" or opening Help → Welcome / Tutorial.

---

## 2. The Three Panels

The main window has three panels side by side.

### Panel 1 — Chapters

Press **Ctrl+1** or say "panel one" or "go to chapters."

This is the chapter list. At the top is always "Whole Book" — a live view of every chapter combined. Below it are all your chapters in order.

**What you can do:**

- Add, rename, delete, and reorder chapters
- Select a chapter to open it in the editor
- Select "Whole Book" to view or analyse the full manuscript

**Voice commands in Panel 1:**

| Say | What happens |
|---|---|
| Add chapter | Adds a new chapter |
| New chapter | Same as above |
| Rename chapter | Renames the selected chapter |
| Delete chapter | Deletes the selected chapter |
| Move up | Moves selected chapter up |
| Move down | Moves selected chapter down |
| Change type | Changes the section type |
| Next chapter | Selects the next chapter |
| Previous chapter | Selects the previous chapter |
| Go to chapters | Focuses Panel 1 |
| Go to chapter list | Same as above |

**Keyboard shortcuts in Panel 1:**

| Keys | Action |
|---|---|
| Ctrl+A | Add chapter |
| Ctrl+D | Rename chapter |
| Ctrl+Delete | Delete chapter |
| Alt+Up | Move chapter up |
| Alt+Down | Move chapter down |

---

### Panel 2 — Writing Editor

Press **Ctrl+2** or say "panel two" or "go to editor."

This is the writing area. When a chapter is selected in Panel 1, it opens here for dictation and editing. When "Whole Book" is selected, the editor shows the full manuscript in read-only mode.

The editor works identically to Microsoft Word for Dragon — dictate naturally, correct with "Correct that," delete with "Scratch that," navigate with voice cursor commands.

**Voice commands in Panel 2:**

| Say | What happens |
|---|---|
| Read chapter | Reads the full chapter aloud |
| Read paragraph | Reads the paragraph at the cursor |
| Read chapter title | Announces the chapter name |
| Current chapter | Same as above |
| Stop reading | Stops speech |
| Stop / Quiet / Silence | Same as above |
| Go to editor | Focuses Panel 2 |
| Open writing editor | Same as above |

**Keyboard shortcuts in Panel 2:**

| Keys | Action |
|---|---|
| Ctrl+S | Save |
| Ctrl+F | Comprehensive AI feedback |
| F4 | Read current paragraph |
| Escape | Return to chapter list |

---

### Panel 3 — AI Assistant

Press **Ctrl+3** or say "panel three" or "go to assistant."

This panel has four tabs: Chat, Prompts, Cards, and Feedback.

**Chat tab** — ask Claude questions, request feedback, load prompts. Type or dictate into the chat box and press Enter or say "send."

**Prompts tab** — browse and use your saved writing prompts.

**Cards tab** — browse and insert saved response cards.

**Feedback tab** — read saved AI feedback entries.

**Voice commands in Panel 3:**

| Say | What happens |
|---|---|
| Go to assistant | Focuses Panel 3 |
| Open assistant panel | Same as above |
| Go to chat | Switches to the Chat tab |
| Open prompt library | Switches to the Prompts tab |
| Open response cards | Switches to the Cards tab |
| Open feedback library | Switches to the Feedback tab |
| Send / Send message | Sends the chat input |
| Ask Claude | Same as above |

---

## 3. Managing Your Project

### Create a New Project

Say "new project" or press **Ctrl+N**. Enter a title when asked. The project saves to your default folder automatically.

### Open a Project

Say "open project" or press **Ctrl+O** to browse for a `.vbk` file.

### Save

Say "save" or press **Ctrl+S**. You will hear "Project saved" when complete.

### Save As

Say "save as" or press **Ctrl+Shift+S**. Choose a new file name or location.

### Import a Word Document

Say "import document" or press **Ctrl+I**. Choose a `.docx` file.

The app reads the document, detects chapter breaks automatically, shows you the results for confirmation, and creates all chapters in your project. You will hear "Importing document..." then "[N] chapters imported" when done.

---

## 4. Writing and Dictating

When you select a chapter in Panel 1, it opens in the Panel 2 editor. Move focus to the editor with **Ctrl+2** and start dictating.

Dragon works here exactly as it does in Microsoft Word:

- Dictate text normally
- Say "Correct that" to correct the last thing Dragon typed
- Say "Scratch that" to delete the last utterance
- Say "Select [phrase]" to select any text in the document
- All Dragon cursor navigation commands work

When you are done writing, say "save" or press **Ctrl+S**.

To hear what you have written, say "read chapter" to hear the full chapter, or position your cursor and say "read paragraph" to hear just that section.

---

## 5. The Whole Book View

The first item in the chapter list is always "Whole Book." Select it to see every chapter combined into a single continuous manuscript in the editor.

The Whole Book view updates automatically as you edit chapters.

When "Whole Book" is selected:

- The editor shows the full manuscript in read-only mode
- The AI assistant uses the entire manuscript as context — all chapters together
- Feedback commands analyse the whole book, not a single chapter

This is the best way to get feedback on pacing, continuity, character arcs, and how the book holds together as a whole.

---

## 6. AI Feedback

AI features require an Anthropic API key. Set it by saying "set API key" or clicking the key icon in the toolbar.

### Chapter Feedback

Select a chapter in Panel 1, then say any of these:

| Say | What Claude analyses |
|---|---|
| Feedback | Overall: pacing, dialogue, style, structure |
| Comprehensive | Same as above |
| Pacing | Where the chapter drags or rushes |
| Dialogue | Naturalness, character voice, dialogue tags |
| Style | Prose, word repetition, passive voice |
| Structure | Hook, transitions, chapter ending |

### Whole Book Analysis

Select "Whole Book" in Panel 1, then say "book analysis" or "analyse book." Claude receives the entire manuscript and gives feedback on arc, character consistency, continuity, and book-wide strengths and weaknesses.

### Chat

Type or dictate a question in the chat box and say "send" or press Enter. Ask anything — writing questions, character advice, plot help, or use a saved prompt.

### Using Claude's Response

After Claude responds:

| Say | What happens |
|---|---|
| Read response | Reads Claude's response aloud |
| Insert at cursor | Inserts the response at your cursor position |
| Insert at start | Inserts at the beginning of the chapter |
| Insert at end | Inserts at the end of the chapter |
| Save card / Save response card | Saves the response to the Card Library |
| Discard response | Removes the response |

---

## 7. The Prompt Library

The Prompt Library contains pre-written prompts organised by category. Categories cover editing, fiction, structure, non-fiction, research, description, dialogue, plot, character development, openings and endings, and whole-book feedback.

**Using a prompt:**

- Say "open prompt library" to browse the Prompts tab
- Say "use prompt A one" (any letter A–K, number 1–10) to load a specific prompt
- Say "read prompt A" to hear all prompts in category A
- Say "what prompts do I have" to hear the category list

**Adding a prompt:**

Say "add new prompt" or click Add Prompt in the Prompts tab.

Prompts are shared across all your projects — they are not project-specific.

---

## 8. Response Cards

When you save a Claude response, it becomes a card. Cards let you keep useful responses and insert them into your writing later.

**Working with cards:**

| Say | What happens |
|---|---|
| Open response cards | Opens the Cards tab |
| What cards do I have | Announces card categories |
| Insert card one | Inserts card number 1 into the chapter |
| Insert card [number] | Inserts any card by number (1–20) |
| Delete card [number] | Deletes a card |

---

## 9. The Feedback Library

Every time you run an AI analysis, the result is saved automatically to the Feedback Library. You can re-read any previous feedback at any time.

**Accessing saved feedback:**

| Say | What happens |
|---|---|
| Open feedback library | Opens the Feedback tab |
| Feedback categories | Lists the feedback categories |
| Read my pacing feedback | Reads all saved pacing entries |
| Read my dialogue feedback | Reads all saved dialogue entries |
| Read my style feedback | Reads all saved style entries |
| Read my structure feedback | Reads all saved structure entries |
| Read my comprehensive feedback | Reads all comprehensive entries |
| Resume reading | Continues reading where you left off |

---

## 10. Exporting Your Book

| Say | What happens |
|---|---|
| Export Word | Exports as a formatted .docx file |
| Export manuscript | Same as above |
| Export PDF | Exports as a PDF with title page and page numbers |
| Create PDF | Same as above |

---

## 11. Settings

Say "open settings" or press the settings button in the toolbar.

### API Key

Say "set API key" to enter your Anthropic API key. This is required for all AI features. The key is saved locally on your PC and is never transmitted anywhere except to Anthropic.

### Default Project Folder

Say "set project folder" to choose where new projects are saved.

### App Voice

Say "toggle voice" to turn the app's text-to-speech on or off. When off, JAWS or JSay handles all readback. When on, the app also speaks status messages.

---

## 12. All Voice Commands

### Navigation

| Say | Action |
|---|---|
| Panel one / two / three | Switch panels |
| Go to chapters | Panel 1 |
| Go to chapter list | Panel 1 |
| Go to editor | Panel 2 |
| Open writing editor | Panel 2 |
| Go to assistant | Panel 3 |
| Open assistant panel | Panel 3 |
| Go to chat | Chat tab in Panel 3 |
| Open prompt library | Prompts tab |
| Open response cards | Cards tab |
| Open feedback library | Feedback tab |
| What can I say here | Lists available commands for the current panel |
| Application status | Announces current app state |

### Project

| Say | Action |
|---|---|
| New project | Create a project |
| Open project | Browse for a project |
| Save | Save current project |
| Save project | Same |
| Save now | Same |
| Save as | Save to a new file |
| Import document | Import a .docx file |
| Import Word document | Same |

### Chapters

| Say | Action |
|---|---|
| Add chapter | Add a chapter |
| New chapter | Same |
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
| Read chapter title | Announce the chapter name |
| Current chapter | Same |
| Stop reading | Stop speech |
| Stop | Same |
| Quiet | Same |
| Silence | Same |

### AI

| Say | Action |
|---|---|
| Feedback | Comprehensive chapter feedback |
| Comprehensive | Same |
| Pacing | Pacing analysis |
| Dialogue | Dialogue analysis |
| Style | Style analysis |
| Structure | Structure analysis |
| Book analysis | Full manuscript analysis |
| Whole book | Same |
| Send | Send chat input |
| Send message | Same |
| Ask Claude | Same |
| Read response | Read Claude's response aloud |
| Insert at cursor | Insert response at cursor |
| Insert at start | Insert at chapter beginning |
| Insert at end | Insert at chapter end |
| Save card | Save response as a card |
| Save response card | Same |
| Discard response | Remove the response |

### Prompt Library

| Say | Action |
|---|---|
| Open prompt library | Open Prompts tab |
| What prompts do I have | Hear categories |
| Read prompt A through J | Hear prompts in that category |
| Use prompt A one | Load prompt A1 |
| Add new prompt | Add a prompt |

### Cards

| Say | Action |
|---|---|
| Open response cards | Open Cards tab |
| What cards do I have | Hear categories |
| Insert card one through twenty | Insert a card |
| Delete card one through five | Delete a card |

### Feedback Library

| Say | Action |
|---|---|
| Open feedback library | Open Feedback tab |
| Feedback categories | Hear categories |
| Read my [type] feedback | Read saved entries |
| Resume reading | Continue reading |

### Settings

| Say | Action |
|---|---|
| Set API key | Enter Anthropic API key |
| Open settings | Open settings dialog |
| Set project folder | Choose default save folder |
| Toggle voice | Toggle app speech on or off |
| Start tutorial | Open the 17-step tutorial |

### App

| Say | Action |
|---|---|
| Close VoiceBook | Close the application |
| Exit VoiceBook | Same |

---

## 13. All Keyboard Shortcuts

| Keys | Action |
|---|---|
| Ctrl+1 | Panel 1 — Chapters |
| Ctrl+2 | Panel 2 — Editor |
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
| Ctrl+Shift+Space | Open command bar (Dragon entry point) |
| Escape | Leave editor, return to chapter list |

---

## 14. Sounds Reference

| Sound | Meaning |
|---|---|
| Rising chime | App ready |
| Falling chime | App closing |
| Soft pop | Project opened |
| Soft click | Project saved |
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
