# VoiceBook Studio — Complete User Guide

**Version 1.0 | Voice-first book writing for visually impaired authors**

---

## Table of Contents

1. [What VoiceBook Studio Does](#1-what-voicebook-studio-does)
2. [System Requirements](#2-system-requirements)
3. [Installation](#3-installation)
4. [First Launch](#4-first-launch)
5. [The Main Interface](#5-the-main-interface)
6. [Keyboard Shortcuts](#6-keyboard-shortcuts)
7. [Voice Commands — Complete Reference](#7-voice-commands--complete-reference)
8. [Working with Projects](#8-working-with-projects)
9. [Chapters and Section Types](#9-chapters-and-section-types)
10. [Writing in the Editor](#10-writing-in-the-editor)
11. [AI Feedback](#11-ai-feedback)
12. [AI Chat](#12-ai-chat)
13. [Prompt Library](#13-prompt-library)
14. [Response Cards](#14-response-cards)
15. [Importing a Word Document](#15-importing-a-word-document)
16. [Exporting Your Manuscript](#16-exporting-your-manuscript)
17. [Configuring Your API Key](#17-configuring-your-api-key)
18. [Configuring Voice (Azure TTS)](#18-configuring-voice-azure-tts)
19. [App Voice Toggle](#19-app-voice-toggle)
20. [Settings Reference](#20-settings-reference)
21. [The Tutorial](#21-the-tutorial)
22. [Known Issues](#22-known-issues)
23. [Appendix A — All 14 Section Types](#appendix-a--all-14-section-types)
24. [Appendix B — Prompt Library Contents](#appendix-b--prompt-library-contents)
25. [Appendix C — Voice Commands Quick Reference Card](#appendix-c--voice-commands-quick-reference-card)

---

## 1. What VoiceBook Studio Does

VoiceBook Studio is a Windows desktop application built for visually impaired authors who write using a screen reader (JAWS) and/or a voice dictation tool (Dragon NaturallySpeaking). It provides:

- A structured three-panel writing environment (chapters, editor, AI assistant)
- Full JAWS screen reader compatibility with live regions for all status updates
- Comprehensive voice command routing — type commands prefixed with `!` in the chat box
- AI-powered writing feedback and chat via the Anthropic Claude API (claude-sonnet-4-6)
- A library of 100 ready-made writing prompts across 10 categories
- Reusable response cards to store and re-insert frequently used AI responses
- Export to professionally formatted Word (.docx) and PDF files
- Import from existing Word documents with automatic chapter detection
- Auto-save every 30 seconds once a file path is set

---

## 2. System Requirements

- **OS:** Windows 10 or Windows 11 (64-bit)
- **Screen reader:** JAWS (optional but fully supported; detected automatically)
- **Voice dictation:** Dragon NaturallySpeaking (optional; detected in tutorial)
- **AI features:** Anthropic API key (starts with `sk-ant-`) — requires a paid Anthropic account
- **Voice (optional):** Azure Cognitive Services key for high-quality neural voices, or Windows built-in SAPI voices (free)
- **Internet:** Required for AI features only

---

## 3. Installation

1. Run `VoiceBookStudio_Setup_v1.0.0.exe`
2. Follow the installer prompts (installs to `Program Files\VoiceBookStudio` by default)
3. Optionally create a desktop shortcut (unchecked by default)
4. Click **Finish** — optionally launch immediately

The installer places your prompt library at `Data\PromptLibrary\prompts.json` inside the install folder. Your project files, response cards, and settings are stored separately in your user profile and are preserved if you uninstall and reinstall.

**File locations after install:**

| What | Where |
|------|-------|
| Application | `C:\Program Files\VoiceBookStudio\` |
| Project files (.vbk) | `Documents\VoiceBook Projects\` (default save location) |
| Response cards | `%APPDATA%\VoiceBookStudio\ResponseCards\cards.json` |
| Settings & API key | Windows Registry: `HKCU\SOFTWARE\VoiceBookStudio` |

---

## 4. First Launch

When you open VoiceBook Studio for the first time:

1. **JAWS is detected** — if JAWS is running, the app disables its own built-in voice so JAWS handles all speech
2. **App settings load** from the registry
3. **The main window opens** and the app announces: *"VoiceBook Studio ready"*
4. **Welcome dialog appears** with the message:
   > *"Voice-first book writing tools for visually impaired writers. This short tutorial will demonstrate voice commands, microphone and speaker checks, and core workflows."*

   - Press **Start Tutorial** (or Enter) to begin the 10-step tutorial
   - Press **Skip Tutorial** to go straight to the app
   - Check **Don't show this again on startup** to suppress this dialog in future

5. After the Welcome dialog, the status bar announces whether your AI is configured:
   - *"Ready. AI assistant is active."* — if an API key is saved
   - *"Ready. AI not configured. Click the AI Not Set button to add your Anthropic API key."* — if no key is saved

---

## 5. The Main Interface

The app has a single window divided into three resizable panels separated by drag-handles.

### Panel 1 — Chapters (left, default width 260px)

Lists all chapters and sections in your book in order. Each entry shows:
- The chapter title
- A sub-label showing its group: *Front Matter*, *Body*, or *Back Matter*
- Non-chapter sections display as `[Section Type]  Title` (e.g. `[Dedication]  My Dedication`)

**Buttons below the list:**

| Button | Action |
|--------|--------|
| Add Chapter | Adds a new Body chapter, prompts for title |
| Rename | Renames the selected chapter |
| Delete | Deletes the selected chapter (confirmation required) |
| Change Type | Opens the Section Type picker |
| Move Up | Moves the selected chapter up in its group |
| Move Down | Moves the selected chapter down in its group |

**Focus this panel:** `Ctrl+1`

### Panel 2 — Editor (centre, fills remaining space)

The writing area. Displays the content of whichever chapter is selected in Panel 1.

- Header shows: `EDITOR — [chapter title]`
- Word count is shown above the editor and updated live as you type
- Word count is also announced in the status bar (live region, read by JAWS)
- Font: Segoe UI 14pt
- Supports full dictation via Dragon or keyboard entry
- Tab and Enter are accepted (multi-paragraph text)

**Focus this panel:** `Ctrl+2`

### Panel 3 — AI Assistant (right, default width 300px)

Contains three tabs:

- **Chat tab** — Interact with Claude, run AI feedback, insert responses into your chapter
- **Prompts tab** — Browse and use the 100 built-in writing prompts
- **Cards tab** — Browse, insert, and manage your saved response cards

**Focus Panel 3 (chat input):** `Ctrl+3`

### Menu Bar

| Menu | Contains |
|------|----------|
| File | New, Open, Save, Save As, Import .docx, Export .docx, Export PDF, Exit |
| Chapters | Add, Rename, Delete, Move Up, Move Down |
| AI | API key status (clickable), Comprehensive Feedback, Pacing, Dialogue, Style, Structure |
| Settings | Configure Voice, voice status, App Voice toggle, JAWS status |
| Help | Keyboard Shortcuts, About |

### Toolbar

From left to right: New Project, Open Project, Save, Import .docx | Add Chapter, Move Up, Move Down | API key status button | JAWS status label, App Voice toggle button

### Status Bar

- **Left:** Current status message (JAWS live region — all status changes are announced)
- **Right:** Live word count for the current chapter

---

## 6. Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+1` | Focus Panel 1 (Chapters) |
| `Ctrl+2` | Focus Panel 2 (Editor) |
| `Ctrl+3` | Focus Panel 3 (Chat input) |
| `Ctrl+N` | New Project |
| `Ctrl+O` | Open Project |
| `Ctrl+S` | Save |
| `Ctrl+Shift+S` | Save As |
| `Ctrl+I` | Import .docx |
| `Ctrl+A` | Add Chapter |
| `Ctrl+D` | Rename Chapter |
| `Ctrl+Delete` | Delete Chapter |
| `Alt+Up` | Move Chapter Up |
| `Alt+Down` | Move Chapter Down |
| `Ctrl+F` | Run Comprehensive AI Feedback |

> **Note:** The panel headers currently display "(F1)", "(F2)", "(F3)" on screen. These labels are incorrect — the actual shortcuts are **Ctrl+1, Ctrl+2, Ctrl+3**. This will be corrected in a future update.

### Chat Input Box — Special Keys

- **Enter** — Sends your message to Claude (or routes a voice command if it starts with `!`)
- **Shift+Enter** — Inserts a line break without sending
- **`!command`** — Routes a voice command (e.g. type `!feedback` to run AI feedback)

---

## 7. Voice Commands — Complete Reference

Voice commands are entered by typing them in the **Chat input box** prefixed with `!`, or by using Dragon NaturallySpeaking to dictate them directly into the chat box.

**Format:** Type `!` followed by the command phrase, then press Enter.
**Example:** Type `!feedback` and press Enter to run comprehensive AI feedback.

Commands are **not case-sensitive**.

---

### Panel Navigation

| Command | Action |
|---------|--------|
| `!panel 1` | Focus the Chapters panel |
| `!go to panel 1` | Focus the Chapters panel |
| `!panel one` | Focus the Chapters panel |
| `!go to panel one` | Focus the Chapters panel |
| `!panel 2` | Focus the Editor panel |
| `!go to panel 2` | Focus the Editor panel |
| `!panel two` | Focus the Editor panel |
| `!go to panel two` | Focus the Editor panel |
| `!panel 3` | Focus the AI Assistant panel |
| `!go to panel 3` | Focus the AI Assistant panel |
| `!panel three` | Focus the AI Assistant panel |
| `!go to panel three` | Focus the AI Assistant panel |
| `!go to chat` | Switch to the Chat tab |
| `!chat tab` | Switch to the Chat tab |
| `!switch to chat` | Switch to the Chat tab |
| `!chat` | Switch to the Chat tab |

> **Note:** `!open chat`, `!open prompt library`, and `!open response cards` do not currently work due to a routing conflict. Use the alternatives listed above or click the tabs directly.

---

### Project Commands

| Command | Action |
|---------|--------|
| `!save` | Save the current project |
| `!save project` | Save the current project |
| `!save now` | Save the current project |
| `!save file` | Save the current project |
| `!new project` | Create a new project |
| `!browse for project` | Open file browser to find a project |
| `!browse project` | Open file browser to find a project |
| `!open [project name]` | Attempt to open a project by name from Documents\VoiceBook Projects |

---

### Chapter Management

| Command | Action |
|---------|--------|
| `!add chapter` | Add a new chapter (prompts for title) |
| `!new chapter` | Add a new chapter |
| `!add section` | Add a new chapter |
| `!new section` | Add a new chapter |
| `!delete chapter` | Delete the selected chapter (confirmation required) |
| `!remove chapter` | Delete the selected chapter |
| `!delete section` | Delete the selected chapter |
| `!delete current chapter` | Delete the selected chapter |
| `!remove current chapter` | Delete the selected chapter |
| `!rename chapter` | Rename the selected chapter |
| `!rename section` | Rename the selected chapter |
| `!rename current chapter` | Rename the selected chapter |
| `!rename` | Rename the selected chapter |
| `!move up` | Move selected chapter up |
| `!move chapter up` | Move selected chapter up |
| `!move section up` | Move selected chapter up |
| `!move down` | Move selected chapter down |
| `!move chapter down` | Move selected chapter down |
| `!move section down` | Move selected chapter down |
| `!change type` | Open the Section Type picker |
| `!change section type` | Open the Section Type picker |
| `!change chapter type` | Open the Section Type picker |

---

### AI Feedback

| Command | Action |
|---------|--------|
| `!feedback` | Run comprehensive feedback on the current chapter |
| `!run feedback` | Run comprehensive feedback |
| `!get feedback` | Run comprehensive feedback |
| `!run comprehensive` | Run comprehensive feedback |
| `!comprehensive feedback` | Run comprehensive feedback |
| `!comprehensive` | Run comprehensive feedback |
| `!pacing` | Run pacing feedback |
| `!pacing feedback` | Run pacing feedback |
| `!run pacing` | Run pacing feedback |
| `!check pacing` | Run pacing feedback |
| `!dialogue` | Run dialogue feedback |
| `!dialogue feedback` | Run dialogue feedback |
| `!run dialogue` | Run dialogue feedback |
| `!check dialogue` | Run dialogue feedback |
| `!style` | Run style feedback |
| `!style feedback` | Run style feedback |
| `!run style` | Run style feedback |
| `!check style` | Run style feedback |
| `!structure` | Run structure feedback |
| `!structure feedback` | Run structure feedback |
| `!run structure` | Run structure feedback |
| `!check structure` | Run structure feedback |

---

### AI Chat

| Command | Action |
|---------|--------|
| `!send` | Send the chat message to Claude |
| `!send message` | Send the chat message |
| `!send chat` | Send the chat message |
| `!send to claude` | Send the chat message |
| `!ask claude` | Send the chat message |

---

### Insert AI Response into Editor

| Command | Action |
|---------|--------|
| `!insert at cursor` | Insert the AI response at the last cursor position in the editor |
| `!insert here` | Insert at cursor |
| `!insert response` | Insert at cursor |
| `!insert response here` | Insert at cursor |
| `!insert at start` | Insert the AI response at the beginning of the chapter |
| `!insert at beginning` | Insert at start |
| `!insert at the start` | Insert at start |
| `!insert at the beginning` | Insert at start |
| `!insert at end` | Insert the AI response at the end of the chapter |
| `!insert at the end` | Insert at end |
| `!append response` | Insert at end |
| `!append to chapter` | Insert at end |

---

### Prompt Library

| Command | Action |
|---------|--------|
| `!show prompts` | Open the Prompts tab |
| `!open prompt library` | ⚠️ Currently broken — use `!show prompts` instead |
| `!prompt [ID]` | Select and use a prompt by ID (e.g. `!prompt F3`) |

> **Note:** `!use prompt [ID]` does not currently work correctly. Use `!prompt [ID]` instead (e.g. `!prompt F3`, `!prompt A1`).

---

### Response Cards

| Command | Action |
|---------|--------|
| `!show response cards` | Open the Cards tab |
| `!cards` | Open the Cards tab |
| `!open response cards` | ⚠️ Currently broken — use `!cards` instead |
| `!save card` | Save the current AI response as a card |
| `!save response card` | Save as a card |
| `!insert card [N]` | Insert card number N from the current list (e.g. `!insert card 2`) |
| `!delete card [N]` | Delete card number N (e.g. `!delete card 1`) |
| `!show [category] cards` | Filter cards by category (e.g. `!show General cards`) |

---

### Export

| Command | Action |
|---------|--------|
| `!export word` | Export manuscript as Word (.docx) |
| `!export docx` | Export as Word |
| `!export manuscript` | Export as Word |
| `!export as word` | Export as Word |
| `!save as word` | Export as Word |
| `!export pdf` | Export as PDF |
| `!export as pdf` | Export as PDF |
| `!save as pdf` | Export as PDF |
| `!create pdf` | Export as PDF |
| `!make pdf` | Export as PDF |

---

### Settings

| Command | Action |
|---------|--------|
| `!toggle voice` | Toggle app voice on/off |
| `!voice on` | Toggle app voice |
| `!voice off` | Toggle app voice |
| `!mute app` | Toggle app voice |
| `!unmute app` | Toggle app voice |
| `!toggle app voice` | Toggle app voice |
| `!set api key` | Open the API key dialog |
| `!configure api` | Open the API key dialog |
| `!add api key` | Open the API key dialog |
| `!api key` | Open the API key dialog |

---

### Tutorial

| Command | Action |
|---------|--------|
| `!start tutorial` | Start the tutorial |
| `!next` | Advance to the next tutorial step |
| `!previous` | Go back to the previous tutorial step |
| `!repeat` | Repeat the current tutorial step announcement |
| `!exit tutorial` | Exit the tutorial |

---

## 8. Working with Projects

### Project File Format

Projects are saved as `.vbk` files — plain UTF-8 JSON that you can open in any text editor. Each file stores:
- Book title and author name
- All chapters with their content, section type, and sort order
- Creation and modification timestamps

Word counts are calculated live and are not stored in the file.

### Creating a New Project

- **Menu:** File → New Project
- **Keyboard:** `Ctrl+N`
- **Voice:** `!new project`

You will be prompted for a **Book Title** and an **Author Name**. The project opens with one empty body chapter titled "Chapter 1".

### Opening a Project

- **Menu:** File → Open Project
- **Keyboard:** `Ctrl+O`
- **Voice:** `!browse for project`

Browse to your `.vbk` file and open it.

### Saving

- **Save:** `Ctrl+S` or `!save` — saves to the current file (prompts for a location if unsaved)
- **Save As:** `Ctrl+Shift+S` — saves to a new location

**Auto-save** runs every 30 seconds after you have saved for the first time. It only triggers if the project has unsaved changes. The status bar announces *"Auto-saved: [filename]"* each time — JAWS users will hear this announcement every 30 seconds while working.

### Default Save Location

The file save dialog opens at `Documents\VoiceBook Projects\` by default. This folder is created automatically if it does not exist.

---

## 9. Chapters and Section Types

### The Three Groups

Every section in your book belongs to one of three groups, and the app always maintains this order:

1. **Front Matter** — appears before the main text
2. **Body** — the main content (your chapters)
3. **Back Matter** — appears after the main text

Chapters can be moved up and down freely within their group. The group order (Front → Body → Back) is fixed.

### All 14 Section Types

See [Appendix A](#appendix-a--all-14-section-types) for the complete list. When you add a chapter, it defaults to **Body / Chapter**. Use **Change Type** (or `!change type`) to assign any of the 14 types.

### Adding a Chapter

1. Press `Ctrl+A` or click **Add Chapter**
2. Type the chapter title in the dialog
3. Press Enter or click OK

The new chapter is inserted into the correct group position based on its section type.

### Renaming a Chapter

1. Select the chapter in Panel 1
2. Press `Ctrl+D` or click **Rename**
3. Edit the title and press Enter

### Deleting a Chapter

1. Select the chapter
2. Press `Ctrl+Delete` or click **Delete**
3. Confirm the deletion in the dialog

### Moving Chapters

- `Alt+Up` / **Move Up** / `!move up` — moves the chapter up within its group
- `Alt+Down` / **Move Down** / `!move down` — moves the chapter down within its group

### Changing Section Type

1. Select the chapter
2. Click **Change Type** or use `!change type`
3. The Section Type dialog shows all 14 types grouped by Front / Body / Back
4. Select the desired type and click OK (or double-click)

The chapter is re-sorted into its correct group position immediately.

---

## 10. Writing in the Editor

Select any chapter in Panel 1 to load it into the editor. You can then:

- **Type** directly into the editor
- **Dictate** using Dragon NaturallySpeaking (focus the editor with `Ctrl+2` first)
- **Paste** text from the clipboard

The live word count updates on every keystroke and is shown above the editor and in the status bar.

Paragraphs are separated by blank lines. During export, each blank-line-separated block becomes a separate paragraph.

---

## 11. AI Feedback

AI feedback analyses the content of the currently selected chapter and returns structured notes from Claude (claude-sonnet-4-6).

> **Requires:** An Anthropic API key configured in Settings. See [Section 17](#17-configuring-your-api-key).

### Five Feedback Types

| Type | What it analyses |
|------|-----------------|
| **Comprehensive** | Overall assessment with sections: Overview, Strengths, Quick Wins, Improvements, Encouragement |
| **Pacing** | Where the chapter drags or rushes; narrative rhythm; specific suggestions |
| **Dialogue** | Naturalness, character voice distinction, dialogue tags, whether lines advance plot or character |
| **Style** | Repeated words, passive voice, weak verbs, adverb overuse, sentence length variety, clichés |
| **Structure** | Opening hook, scene transitions, paragraph purpose, chapter ending, sections to split or merge |

### Running Feedback

- **Buttons** in the Chat tab: Comprehensive, Pacing, Dialogue, Style, Structure
- **AI menu:** Comprehensive Feedback, Pacing, Dialogue, Style, Structure
- **Keyboard:** `Ctrl+F` for Comprehensive
- **Voice:** `!feedback`, `!pacing`, `!dialogue`, `!style`, `!structure`

The response appears in the read-only AI response box. JAWS announces new content automatically via the live region.

### After You Get Feedback

Once a response is shown, three **Insert** buttons appear:

| Button | What it does |
|--------|-------------|
| **At Cursor** | Inserts the response text at the last position your cursor was in the editor |
| **At Start** | Inserts at the very beginning of the chapter content |
| **At End** | Appends to the end of the chapter content |

A **Save as Card** button also appears — see [Section 14](#14-response-cards).

---

## 12. AI Chat

The chat input at the bottom of the Chat tab lets you have a free-form conversation with Claude about your writing.

1. Type your question or request in the chat input box
2. Press **Enter** or click **Send** (or `!send`)

If a chapter is selected in Panel 1, Claude automatically receives the full chapter content as context. Claude will answer your question in the context of that chapter.

If no chapter is selected, Claude answers as a general writing assistant without chapter context.

**Example questions:**
- *"What are three ways I could end this chapter with more tension?"*
- *"Can you suggest a better name for the character called John?"*
- *"This scene feels flat — what's missing?"*

Chat responses appear in the AI response box and can be inserted into the editor using the same Insert buttons as feedback.

---

## 13. Prompt Library

The Prompt Library contains **100 ready-made writing prompts** across 10 categories. Use them to give Claude a specific task for your current chapter.

### Accessing the Library

- Click the **Prompts** tab in Panel 3
- Voice: `!show prompts`

### Using a Prompt

1. Filter by category using the **Category** dropdown (or leave on "All")
2. Browse the list — each entry shows the prompt ID and title
3. Select a prompt to see a preview of its content
4. Click **Use Prompt** to send it to Claude with your current chapter as context

**Voice shortcut:** `!prompt [ID]` — for example `!prompt F3` or `!prompt A1`

> **Note:** `!use prompt [ID]` does not currently work. Use `!prompt [ID]` instead.

### Prompt Categories

| Category | Description |
|----------|-------------|
| A1–A5 | Editing |
| B1–B5 | Fiction |
| C1–C5 | Structure |
| D1–D5 | Non-fiction |
| E1–E5 | Research |
| F1–F10 | Description & Atmosphere |
| G1–G10 | Dialogue & Voice |
| H1–H10 | Plot & Structure |
| I1–I10 | Character Development |
| J1–J10 | Opening Hooks & Endings |

See [Appendix B](#appendix-b--prompt-library-contents) for the full list of all 100 prompts.

---

## 14. Response Cards

Response Cards let you save AI responses you find useful and re-insert them later — into any chapter, at any time.

### Saving a Response as a Card

After Claude produces a response:

1. Click **Save as Card** (appears below the AI response box)
2. Enter a title for the card (default = first 40 characters of the response)
3. Enter a category (default = "General")
4. The card is saved to `%APPDATA%\VoiceBookStudio\ResponseCards\cards.json`

**Voice:** `!save card`

### Using a Card

1. Click the **Cards** tab in Panel 3
2. Filter by category if needed
3. Select a card to preview its content
4. Click **Insert Card** to insert it at the cursor position in the editor

**Voice:** `!insert card [N]` where N is the card's position in the current filtered list (1-based)

### Filtering Cards

Use the **Category** dropdown in the Cards tab to filter by category.
**Voice:** `!show [category] cards` (e.g. `!show General cards`)

### Deleting a Card

Select the card and click **Delete Card**.
**Voice:** `!delete card [N]`

> **Note:** There is currently no confirmation prompt when deleting a card. Deletion is immediate.

Cards persist between sessions and are preserved when you uninstall and reinstall the app.

---

## 15. Importing a Word Document

You can import an existing `.docx` file and VoiceBook Studio will attempt to split it into chapters automatically.

**Menu:** File → Import .docx
**Keyboard:** `Ctrl+I`

### How Chapter Detection Works

1. The app first scans the document for **Word heading styles** (Heading 1, Heading 2) and common **chapter patterns** — e.g. "Chapter 1", "CHAPTER ONE", numbered headings like "1. Title"
2. If 2 or more chapter breaks are found, they are used directly — no API call is made
3. If fewer than 2 breaks are found and an API key is configured, Claude analyses the document and suggests natural chapter breaks based on scene changes, time jumps, perspective shifts, and topic changes
4. If no breaks can be identified, the entire document is imported as a single chapter titled "Imported Chapter"

### Chapter Confirmation Dialog

After detection, the **Confirm Detected Chapters** dialog shows all suggested chapter titles.

- **Accept All** — imports each section as a separate chapter; you are then prompted to confirm or edit each title one at a time
- **Import as Single** — imports the entire document as one chapter
- **Cancel** — currently triggers single-chapter import (same as Import as Single)

### Editing Titles After Accept All

For each detected chapter, an edit dialog appears with the suggested title. You can:
- Keep the title and press Enter
- Type a new title and press Enter
- Press Cancel to keep the suggested title and move to the next chapter

---

## 16. Exporting Your Manuscript

### Export as Word (.docx)

**Menu:** File → Export Manuscript (.docx)
**Voice:** `!export word`

Produces a Word document with:
- All sections in order: Front Matter → Body → Back Matter
- Page break between every section
- **Title Page:** book title 26pt bold centred, author 14pt centred
- **Chapter headings:** 16pt bold, dark blue (#1F3864)
- **Body text:** 12pt black
- **Copyright:** 10pt grey, left-aligned
- **Dedication / Epigraph:** centred body text

### Export as PDF

**Menu:** File → Export as PDF
**Voice:** `!export pdf`

Produces an A4 PDF (Times New Roman throughout) with:
- **Page 1 — Title page:** book title 36pt bold centred, author 20pt grey centred
- **All other pages:** 3cm margins; running header (book title, 9pt italic grey); page numbers in footer ("— N —")
- **Chapter headings:** 18pt bold dark blue (#1F3864)
- **Prologue / Epilogue headings:** 16pt
- **All other section headings:** 14pt
- **Body text:** 12pt, 1.5 line spacing, near-black (#111111)
- **Copyright body:** 10pt
- **Dedication / Epigraph body:** centred
- Page break between every section

---

## 17. Configuring Your API Key

AI features (feedback, chat, smart import) require an Anthropic API key.

### Getting an API Key

1. Create an account at [console.anthropic.com](https://console.anthropic.com)
2. Generate an API key — it will start with `sk-ant-`
3. Keep it private — treat it like a password

### Entering Your Key

- **Toolbar:** Click the **AI Not Set** button
- **AI menu:** Click the key status item at the top of the menu
- **Voice:** `!set api key`

In the dialog:
1. Paste your key into the field
2. Use **Show / Hide** to verify what you typed
3. Click **Save**

The key is stored in the Windows Registry at `HKCU\SOFTWARE\VoiceBookStudio\AnthropicApiKey`.

### Removing Your Key

Open the API Key dialog and click **Clear Key** (confirmation required).

---

## 18. Configuring Voice (Azure TTS)

By default, VoiceBook Studio uses your Windows built-in SAPI voice. For higher quality neural voices, you can configure Azure Cognitive Services.

**Menu:** Settings → Configure Voice

### Available Azure Voices

| Voice | Description |
|-------|-------------|
| en-US-AriaNeural | Warm, conversational female **(recommended)** |
| en-US-JennyNeural | Friendly, clear female |
| en-US-JaneNeural | Warm, expressive female |
| en-US-GuyNeural | Casual, natural male |
| en-US-DavisNeural | Professional male |
| en-US-JasonNeural | Energetic male |
| en-US-TonyNeural | Confident male |
| en-US-NancyNeural | Pleasant female |

Use **Test Voice** in the dialog to hear a preview before saving.

### Free Options

- **Windows 11 neural voices** — free in Windows Settings → Time & Language → Speech → Manage voices
- **Azure free tier** — 500,000 characters per month free

### Reverting to SAPI

Click **Clear** in the Configure Voice dialog to remove Azure settings and revert to the Windows SAPI voice.

---

## 19. App Voice Toggle

When JAWS is **not** running, VoiceBook Studio speaks status messages using its own voice. You can turn this off if you prefer silence.

- **Toolbar:** App Voice button (shows "App voice: On" or "App voice: Off")
- **Settings menu:** The toggleable App Voice item
- **Voice:** `!toggle voice`

When **JAWS is detected at startup**, the app voice is automatically suppressed and JAWS handles all speech.

---

## 20. Settings Reference

All settings are stored in the Windows Registry at `HKCU\SOFTWARE\VoiceBookStudio`.

| Setting | Default | Notes |
|---------|---------|-------|
| Show Welcome on Startup | Yes | Uncheck in the Welcome dialog to suppress |
| Tutorial Completed | No | Set automatically when you finish the tutorial |
| Azure Speech Key | (empty) | Your Azure Cognitive Services key |
| Azure Speech Region | (empty) | e.g. `eastus`, `westeurope` |
| Azure Voice Name | en-US-AriaNeural | Selected voice for Azure TTS |
| Anthropic API Key | (empty) | Must start with `sk-ant-` |

**Runtime-only (not saved between sessions):**
- JAWS detected: re-checked every time the app starts
- Last used prompt ID: resets on restart

---

## 21. The Tutorial

The tutorial is a 10-step guided walkthrough that introduces the app's key areas.

**To start:** Click **Start Tutorial** on the Welcome dialog, or type `!start tutorial`

Each step is announced as: *"Step N of 10: [Title]. [Content]."*

| Step | Title | What happens |
|------|-------|-------------|
| 1 | Welcome & Audio Test | Checks you can hear the app voice |
| 2 | Microphone Test | Prompts you to say "Testing microphone" |
| 3 | JAWS Detection | Announces whether JAWS was detected at startup |
| 4 | Voice Commands Test | Announces whether Dragon was detected; prompts you to try `!panel 1` |
| 5 | Panel 1: Chapter Manager | Introduces the Chapters panel |
| 6 | Panel 2: Writing Editor | Introduces the Editor panel |
| 7 | Panel 3: Claude Assistant | Introduces the AI Assistant panel |
| 8 | Prompt Library | Introduces the Prompts tab |
| 9 | Response Cards | Introduces the Cards tab |
| 10 | Wrap-up | Marks the tutorial as complete |

**Navigation:** Next, Previous, Repeat, Exit Tutorial buttons.

Tutorial announcements always use the app's voice even when JAWS is detected — you will hear them regardless of your voice settings.

---

## 22. Known Issues

The following issues are present in the current version and will be fixed in an upcoming update:

| # | Issue | Workaround |
|---|-------|------------|
| 1 | Panel headers show **(F1) (F2) (F3)** but actual shortcuts are **Ctrl+1, Ctrl+2, Ctrl+3** | Use Ctrl+1/2/3 |
| 2 | `!open prompt library`, `!open response cards`, `!open chat` do not work | Use `!show prompts`, `!cards`, `!chat` |
| 3 | `!use prompt [ID]` does not work | Use `!prompt [ID]` (e.g. `!prompt F3`) |
| 4 | Recent projects list is always empty | Use File → Open Project to browse |
| 5 | Auto-save announces to JAWS every 30 seconds | Expected behaviour in current version |
| 6 | AI buttons appear active without an API key | Error shown on click — configure key first |
| 7 | Cancelling the Chapter Confirmation dialog triggers single-chapter import | Click Import as Single to get the same result intentionally |
| 8 | Delete Card has no confirmation prompt | Deletion is immediate — double-check before deleting |
| 9 | Last used prompt ID resets on restart | Re-select your prompt each session |

---

## Appendix A — All 14 Section Types

### Front Matter

| Type | Default Title | Export notes |
|------|--------------|-------------|
| Title Page | Title Page | Book title and author centred |
| Copyright | Copyright | Smaller grey text |
| Dedication | Dedication | Body text centred |
| Epigraph | Epigraph | Body text centred |
| Table of Contents | Table of Contents | |
| Foreword | Foreword | |
| Preface | Preface | |
| Introduction | Introduction | |
| Prologue | Prologue | 16pt heading in PDF |

### Body

| Type | Default Title | Export notes |
|------|--------------|-------------|
| Chapter | New Chapter | 18pt heading in PDF |

### Back Matter

| Type | Default Title | Export notes |
|------|--------------|-------------|
| Epilogue | Epilogue | 16pt heading in PDF |
| Afterword | Afterword | |
| Appendix | Appendix | |
| About the Author | About the Author | |

---

## Appendix B — Prompt Library Contents

### A — Editing
A1 Proofread for errors · A2 Tighten the prose · A3 Check dialogue punctuation · A4 Find repetitive words · A5 Improve sentence variety

### B — Fiction
B1 Strengthen character voice · B2 Add sensory details · B3 Increase tension · B4 Show don't tell · B5 Improve pacing

### C — Structure
C1 Outline the chapter · C2 Identify the turning point · C3 Check scene structure · C4 Improve transitions · C5 Strengthen the opening hook

### D — Non-Fiction
D1 Clarify the main argument · D2 Add supporting examples · D3 Improve readability · D4 Strengthen the conclusion · D5 Check logical flow

### E — Research
E1 Fact-check this passage · E2 Suggest credible sources · E3 Identify unsupported claims · E4 Add citations · E5 Verify historical accuracy

### F — Description & Atmosphere
F1 Expand with sensory details · F2 Describe the setting · F3 Add metaphors and similes · F4 Deepen the atmosphere · F5 Show character through environment · F6 Create vivid action · F7 Write the weather · F8 Describe a character's appearance · F9 Build tension through setting · F10 Paint the emotional landscape

### G — Dialogue & Voice
G1 Sharpen the dialogue · G2 Add conflict to the conversation · G3 Differentiate character voices · G4 Write subtext · G5 Show emotion through dialogue · G6 Write a realistic argument · G7 Add dialogue beats · G8 Write witty banter · G9 Craft a confession or revelation · G10 Improve exposition in dialogue

### H — Plot & Structure
H1 Brainstorm plot twists · H2 Find the story stakes · H3 Create obstacles · H4 Write the next scene · H5 Reverse engineer the climax · H6 Add a subplot · H7 Strengthen the inciting incident · H8 Build to a cliffhanger · H9 Create cause and effect · H10 Map character arcs

### I — Character Development
I1 Deepen character motivation · I2 Create character contradictions · I3 Write an internal monologue · I4 Develop character relationships · I5 Give the character a secret · I6 Show character growth · I7 Create a character quirk or habit · I8 Explore character backstory · I9 Test character values · I10 Write character reactions

### J — Opening Hooks & Endings
J1 Write a compelling opening line · J2 Strengthen the first paragraph · J3 Open with dialogue · J4 Start in medias res · J5 Create mystery in the opening · J6 Write a satisfying chapter ending · J7 Craft an emotional ending · J8 Stick the landing · J9 Write a circular ending · J10 Create a bittersweet resolution

---

## Appendix C — Voice Commands Quick Reference Card

Type any of these in the chat input box preceded by `!` and press Enter.

```
NAVIGATION
  panel 1 / panel 2 / panel 3
  go to panel one / two / three
  chat / chat tab / switch to chat
  show prompts
  cards

PROJECT
  save / save file / save now / save project
  new project
  browse for project

CHAPTERS
  add chapter / new chapter
  rename / rename chapter
  delete chapter / remove chapter
  move up / move down
  change type

AI FEEDBACK
  feedback / comprehensive
  pacing / dialogue / style / structure

AI CHAT
  send / send message / ask claude

INSERT RESPONSE
  insert at cursor / insert here
  insert at start / insert at beginning
  insert at end / append response

PROMPT LIBRARY
  show prompts
  prompt [ID]              e.g.  !prompt F3

RESPONSE CARDS
  cards / show response cards
  save card
  insert card [N]          e.g.  !insert card 2
  delete card [N]
  show [category] cards    e.g.  !show General cards

EXPORT
  export word / export manuscript
  export pdf / create pdf

SETTINGS
  set api key / api key
  toggle voice / voice on / voice off

TUTORIAL
  start tutorial
  next / previous / repeat
  exit tutorial
```

---

*VoiceBook Studio — built for authors who write by voice.*
