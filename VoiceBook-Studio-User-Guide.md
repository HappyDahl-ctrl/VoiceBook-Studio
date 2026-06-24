# VoiceBook Studio — User Guide

**Version:** Current release (June 2026)
**Designed for:** Dragon NaturallySpeaking · JAWS · JSay · Windows built-in voice recognition

---

## What Is VoiceBook Studio?

VoiceBook Studio is a writing application built from the ground up for complete voice control. Every feature — creating a project, writing chapters, getting AI feedback, saving, and exporting — can be done by speaking. You never need to touch the keyboard or mouse unless you want to.

---

## Part 1 — What Happens When You Open the App

When VoiceBook Studio launches, it speaks a series of announcements before showing the main window:

1. **Assistive technology status** — tells you whether JAWS, Dragon, and JSay were detected
2. **Microphone status** — tells you whether Dragon owns the mic or whether the built-in voice recogniser is starting
3. **"VoiceBook Studio ready. Focus is on the chapter list."** — the app is ready to use

When you close the app, you will hear:
- A soft closing chime
- **"VoiceBook Studio closing. Goodbye."**

These announcements always play, regardless of whether JAWS is running.

---

## Part 2 — The First-Launch Welcome

On your very first launch, the Welcome dialog opens automatically. It speaks:

> *"Welcome to VoiceBook Studio. This is a writing application designed for complete voice control. JAWS will read all controls. Dragon commands are active. Say 'click start guided tour' to begin the introduction, or say 'click skip tour' to go straight to the application."*

- **Start Guided Tour** — begins the 17-step interactive tutorial (recommended on first use)
- **Skip Tour** — goes straight to the main window

If you close the Welcome dialog without making a choice (X button, Alt+F4), it will reappear next time you launch, giving you another chance to take the tour.

Once you complete or explicitly skip within the tutorial, the Welcome dialog will not auto-start again. You can always re-open it from **Help → Welcome / Tutorial** or by saying **"start tutorial"**.

---

## Part 3 — The Three Panels

The main window has three panels side by side. Say the panel name or press the keyboard shortcut to move between them.

### Panel 1 — Chapters (Ctrl+1)

The chapter list. This is where you manage your book structure.

At the top of the list is always **"Whole Book"** — a read-only view of every chapter combined into one continuous manuscript. Below it are all your chapters and sections in document order.

**What you can do here:**
- Add, rename, delete, and reorder chapters
- Select a chapter to open it in the editor
- Select "Whole Book" to view and review the full manuscript

**Panel 1 voice commands:**
| Say | Action |
|---|---|
| Panel one | Focus Panel 1 |
| Go to chapters / Go to chapter list | Focus Panel 1 |
| Add chapter | Add a new chapter |
| Rename chapter | Rename the selected chapter |
| Delete chapter | Delete the selected chapter |
| Change type | Change the section type (Chapter, Dedication, Appendix, etc.) |
| Move up | Move selected chapter up |
| Move down | Move selected chapter down |
| Next chapter | Select the next chapter in the list |
| Previous chapter | Select the previous chapter |

**Keyboard shortcuts in Panel 1:**
- Ctrl+A — Add chapter
- Ctrl+D — Rename chapter
- Ctrl+Delete — Delete chapter
- Alt+Up / Alt+Down — Reorder chapter

---

### Panel 2 — Writing Editor (Ctrl+2)

The writing area. This is where you dictate and edit your chapters.

When a chapter is selected, the editor opens it for writing. When "Whole Book" is selected, the editor shows the full manuscript in read-only mode.

The editor uses the same text layer as Microsoft Word, so Dragon NaturallySpeaking works here exactly as it does in Word — dictate, correct, navigate, and use "Scratch that" / "Select that" normally.

**Panel 2 voice commands:**
| Say | Action |
|---|---|
| Panel two | Focus Panel 2 |
| Go to editor / Open writing editor | Focus Panel 2 |
| Read chapter | Read the entire chapter aloud |
| Read paragraph | Read the paragraph at the cursor |
| Read chapter title / Current chapter | Announce the chapter title |
| Stop reading / Stop / Quiet / Silence | Stop speech |

**Keyboard shortcuts in Panel 2:**
- Ctrl+S — Save
- Ctrl+F — Comprehensive AI feedback on current chapter
- F4 — Read the current paragraph
- Escape or Ctrl+1 — Go to chapter list

---

### Panel 3 — AI Assistant (Ctrl+3)

The AI assistant panel. This has four tabs: Chat, Prompts, Cards, and Feedback.

**Panel 3 voice commands:**
| Say | Action |
|---|---|
| Panel three | Focus Panel 3 |
| Go to assistant / Open assistant panel | Focus Panel 3 |
| Send / Send message / Ask Claude | Send the chat input to Claude |
| Ask assistant [your question] | Send a question directly without typing |

---

## Part 4 — Projects

### Creating a New Project

Say **"new project"** or press **Ctrl+N**. You will be asked for a project title. The project is saved automatically to your default folder (set this in Settings).

### Opening a Project

Say **"open project"** or press **Ctrl+O** to browse for a `.vbk` project file.
Say **"open [project name]"** to open a project by name from your default folder.

### Saving

Say **"save"** or press **Ctrl+S**. You will hear "Project saved" when it completes.

### Importing a Word Document

Say **"import document"** or press **Ctrl+I**. The app will:
1. Ask you to choose a `.docx` file
2. Read the document and detect chapter breaks automatically (using Claude if an API key is set, or built-in pattern detection)
3. Show you the detected chapters for confirmation
4. Create the chapters in your project

You will hear "Importing document..." when it starts, and "[N] chapters imported from document" when it finishes.

### Exporting

| Say | Action |
|---|---|
| Export Word / Export manuscript | Export as a formatted `.docx` file |
| Export PDF / Create PDF | Export as a formatted PDF with title page and page numbers |

---

## Part 5 — Whole Book View

The first item in the chapter list is always **"Whole Book"**. Select it (by clicking or navigating with arrow keys) to see all chapters combined into a single continuous manuscript in the editor.

The Whole Book view updates automatically whenever you make changes to any chapter.

When Whole Book is selected, the AI assistant uses the **full manuscript** as its context — all chapters together — instead of a single chapter. This is the best way to get feedback on overall pacing, continuity, character arcs, and how the book holds together as a whole.

Voice command: Say **"whole book"** or **"book analysis"** from the AI panel.

---

## Part 6 — The AI Assistant

The AI features require an Anthropic API key. Set it by saying **"set API key"** or clicking the key button in the toolbar.

### Chapter Feedback (Panel 3 → Chat tab)

These commands analyse the chapter currently open in the editor. When Whole Book is selected, they analyse the full manuscript instead.

| Say | What Claude analyses |
|---|---|
| Feedback / Comprehensive | Overall: pacing, dialogue, style, structure |
| Pacing | Where the chapter drags or rushes |
| Dialogue | Naturalness, character voice, dialogue tags |
| Style | Prose style, repeated words, passive voice |
| Structure | Hook, transitions, chapter ending |

### Full Book Analysis

Say **"book analysis"** or **"analyse book"** to send the entire manuscript to Claude for book-wide feedback: arc, character consistency, continuity across chapters, recurring strengths and weaknesses.

### Chat

Type or dictate a question into the chat box and say **"send"** or press Enter. You can ask Claude anything — writing questions, feedback requests, character advice, or use the prompt library to load a pre-written prompt.

Say **"ask assistant [your question]"** to send a question directly without going to the chat box first.

### Using the Response

After Claude responds, say:

| Say | Action |
|---|---|
| Insert at cursor | Insert Claude's response where your cursor was |
| Insert at start | Insert at the beginning of the chapter |
| Insert at end | Insert at the end of the chapter |
| Save response card / Save card | Save the response to your Card Library for later |
| Read response | Read the response aloud |
| Discard response / Clear response | Remove the response |

---

## Part 7 — Prompt Library

The Prompt Library contains pre-written writing prompts organised by category. There are prompts for editing, fiction, structure, non-fiction, research, description, dialogue, plot, character development, openings and endings, and whole-book feedback.

**To use a prompt:**
- Say **"use prompt A1"** (or any prompt ID) to load it into the chat box, then say **"send"**
- Say **"open prompt library"** to browse prompts in the Prompts tab
- Say **"what prompts do I have"** to hear the categories
- Say **"read prompts A"** to hear all prompts in category A

**To add a prompt:**
Say **"add new prompt"** or click **Add Prompt** in the Prompts tab.

---

## Part 8 — Response Cards and Feedback Library

### Response Cards

Responses you save become cards. Use cards to keep useful Claude responses and insert them into chapters later.

| Say | Action |
|---|---|
| Open response cards / Cards | Switch to the Cards tab |
| Insert card [number] | Insert a specific card into the chapter |
| Use card [ID] | Insert a card by its letter-number ID |
| Delete card [number] | Delete a card |
| What cards do I have | Hear the card categories |

### Feedback Library

AI feedback is automatically saved to the Feedback Library every time you run an analysis. You can go back and re-read any saved feedback.

| Say | Action |
|---|---|
| Open feedback library | Switch to the Feedback tab |
| Read my pacing feedback | Read all saved pacing feedback entries |
| Read my comprehensive feedback | Read comprehensive feedback entries |
| Resume reading / Continue reading | Resume reading a library entry |

---

## Part 9 — All Voice Commands at a Glance

### Navigation
| Say | Action |
|---|---|
| Panel one / two / three | Switch panels |
| Go to chapters | Panel 1 |
| Go to editor | Panel 2 |
| Go to assistant | Panel 3 |
| What can I say here | Hear available commands for the current panel |
| Application status | Hear current app state |

### Project
| Say | Action |
|---|---|
| New project | Create a new project |
| Open project | Browse for a project file |
| Open [project name] | Open a project by name |
| Save / Save project | Save |
| Save as | Save to a new file |
| Import document / Import Word | Import a .docx file |
| Export Word / Export manuscript | Export to Word |
| Export PDF / Create PDF | Export to PDF |

### Chapters
| Say | Action |
|---|---|
| Add chapter / New chapter | Add a chapter |
| Rename chapter | Rename selected chapter |
| Delete chapter | Delete selected chapter |
| Move up / Move down | Reorder chapter |
| Change type | Change section type |
| Next chapter / Previous chapter | Navigate chapters |

### Writing and Reading
| Say | Action |
|---|---|
| Read chapter | Read chapter aloud |
| Read paragraph | Read current paragraph |
| Read chapter title | Hear chapter title |
| Stop reading / Stop / Quiet | Stop speech |

### AI
| Say | Action |
|---|---|
| Feedback / Comprehensive | Comprehensive chapter feedback |
| Pacing / Dialogue / Style / Structure | Specific analysis types |
| Book analysis / Whole book | Full manuscript analysis |
| Ask assistant [question] | Send a question to Claude |
| Send / Send message | Send chat input |
| Insert at cursor / start / end | Insert Claude response |
| Save card / Save response card | Save response |
| Read response | Read Claude's last response |

### Settings and Help
| Say | Action |
|---|---|
| Set API key | Configure Anthropic API key |
| Open settings | App settings |
| Set project folder | Set default save location |
| Toggle voice | Toggle app TTS on or off |
| Start tutorial | Re-open the 17-step tutorial |

---

## Part 10 — Keyboard Shortcuts

| Keys | Action |
|---|---|
| Ctrl+1 / F1 | Focus Panel 1 (Chapters) |
| Ctrl+2 / F2 | Focus Panel 2 (Editor) |
| Ctrl+3 / F3 | Focus Panel 3 (AI Assistant) |
| Ctrl+N | New project |
| Ctrl+O | Open project |
| Ctrl+S | Save |
| Ctrl+Shift+S | Save As |
| Ctrl+I | Import document |
| Ctrl+A | Add chapter |
| Ctrl+D | Rename chapter |
| Ctrl+Delete | Delete chapter |
| Alt+Up / Alt+Down | Move chapter up / down |
| Ctrl+F | Comprehensive AI feedback |
| F4 | Read current paragraph |
| Escape | Leave editor, return to chapter list |

---

## Part 11 — What to Set Up in JAWS, Dragon, and JSay

### JAWS

**No JAWS-side configuration is needed.** VoiceBook Studio detects JAWS automatically at startup and adjusts its behaviour:

- The app's general TTS is silenced so JAWS handles interface readback without both speaking at once
- Critical system announcements (startup, project events, goodbye) still speak through a separate SAPI channel with a 500 ms gap before each one, so they don't clash with JAWS mid-sentence
- All buttons, list items, input fields, menus, and tabs have automation labels that JAWS reads correctly
- Live regions on the status bar, editor title, and AI response box make JAWS announce changes automatically without the user navigating there

**One thing to be aware of:** When JAWS is running, some events will be announced twice — once by JAWS reading the control, and once by the app's system announcer. This is intentional for critical events (project opened, save confirmed, etc.) so users are never left wondering if something worked. If this becomes noisy, the app's secondary voice can be silenced with **"toggle voice"** and you can rely on JAWS alone for routine feedback.

**Recommended JAWS settings (optional improvements):**
- Ensure JAWS has a clear audio route — VoiceBook uses the default Windows audio device for its SAPI announcements, so both need to be on the same device
- If using an external audio device or headset, set both Windows default playback and JAWS output to the same device

---

### Dragon NaturallySpeaking

**No Dragon-side configuration is required for basic use.** The writing editor works exactly like Word — dictate naturally, correct with "Correct that", scratch with "Scratch that", navigate with cursor commands.

**For best results:**

1. **Train your profile to the word "VoiceBook"** — Dragon may mishear it initially. Say "VoiceBook" several times in a document or use the Vocabulary Editor to add it.

2. **For app commands:** When Dragon is running, it owns the microphone and the built-in voice recogniser is disabled. App commands (panel navigation, save, chapter management, AI requests) are entered by:
   - Dictating into the **chat input box** in Panel 3 and pressing Enter
   - Using keyboard shortcuts
   - Using the menus (fully accessible with keyboard and Dragon)

3. **Optional — Dragon MyCommands for VoiceBook:** You can add custom Dragon commands that map to VoiceBook keyboard shortcuts. These are optional but speed up the workflow considerably. Example custom commands:
   - "Save VoiceBook" → sends Ctrl+S
   - "New Chapter" → sends Ctrl+A
   - "Switch to Chapters" → sends Ctrl+1
   - "Switch to Editor" → sends Ctrl+2
   - "Switch to Assistant" → sends Ctrl+3
   - "Run Feedback" → sends Ctrl+F
   - "Export Word Document" → sends keystroke for File → Export

   To create these: open Dragon, go to **Tools → Command Browser → New Command** (or say **"Add new command"** in Dragon). Set the type to "Keystroke" and map the shortcut.

4. **"What can I say here"** — say this at any time to hear what commands are available in the current panel. This works from the chat box in Dragon mode.

---

### JSay

**No JSay-side configuration is required.** VoiceBook Studio detects JSay at startup and reports it in the startup announcement. JSay works alongside the app the same way JAWS does — it reads controls, labels, and live regions through the Windows UIA accessibility layer.

All automation labels, help text, and live regions are present and will be read by JSay as you navigate the interface.

---

## Part 12 — Tips for Your Workflow

**Starting a new book:**
1. Say "new project" and give it a title
2. Say "add chapter" for each chapter you want to create
3. Navigate to a chapter, move to the editor (Panel 2), and start dictating

**Importing an existing manuscript:**
1. Say "import document" and choose your .docx file
2. The app will detect chapters automatically and ask you to confirm
3. Your project will have all chapters ready to edit

**Getting feedback on a chapter:**
1. Select the chapter in Panel 1
2. Say "feedback" for comprehensive feedback, or "pacing" / "dialogue" / "style" / "structure" for a specific type
3. Claude responds in the AI panel
4. Say "insert at end" (or cursor / start) to add suggestions to your chapter, or say "save card" to keep the response for later

**Getting feedback on the whole book:**
1. Select "Whole Book" at the top of the chapter list
2. Say "book analysis" or "whole book feedback"
3. Claude analyses all chapters together and responds with manuscript-level feedback

**Saving a useful Claude response:**
1. After Claude responds, say "save card"
2. Give it a title when prompted
3. Later, say "insert card [number]" or "use card [ID]" to bring it back

**If you're not sure what to say:**
Say **"what can I say here"** at any time. The app will read out the commands available in the current panel.

---

## Part 13 — Sounds Guide

The app plays a small sound for each significant event so you always know something happened even if you missed the spoken announcement.

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
| Bell | Export succeeded |
| Error tone | Export failed |
| Click | Voice command recognised |
| Low beep | Voice command not recognised |
| Step chime | Tutorial step advanced |
| Fanfare | Tutorial complete |

---

*VoiceBook Studio is designed so that a writer who cannot see the screen can use every feature independently. If anything doesn't speak as expected, say "what can I say here" to get oriented, or open the tutorial from Help → Welcome / Tutorial.*
