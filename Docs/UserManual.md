# VoiceBook Studio — User Manual

**Version 1.0**  
Accessibility-first book writing software for JAWS, Dragon Professional, and JSay users.

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [System Requirements](#2-system-requirements)
3. [Installation](#3-installation)
4. [First Launch](#4-first-launch)
5. [Interface Overview](#5-interface-overview)
6. [Projects](#6-projects)
7. [Chapters and Sections](#7-chapters-and-sections)
8. [Writing Editor](#8-writing-editor)
9. [AI Assistant](#9-ai-assistant)
10. [Writing Prompts](#10-writing-prompts)
11. [Response Cards](#11-response-cards)
12. [Export — Word (.docx)](#12-export--word-docx)
13. [Export — PDF](#13-export--pdf)
14. [Keyboard Shortcuts](#14-keyboard-shortcuts)
15. [Voice Commands](#15-voice-commands)
16. [Dragon Professional Setup](#16-dragon-professional-setup)
17. [JSay 23 Setup](#17-jsay-23-setup)
18. [JAWS 25 Setup](#18-jaws-25-setup)
19. [AI Configuration (Anthropic API key)](#19-ai-configuration-anthropic-api-key)
20. [Settings](#20-settings)
21. [Troubleshooting](#21-troubleshooting)

---

## 1. Introduction

VoiceBook Studio is a book writing application designed from the ground up for writers who use  
assistive technology. Every feature is fully accessible without a mouse. The app is tested with:

- **JAWS 25** (screen reader)
- **Dragon Professional** (speech-to-text / voice control)
- **JSay 23** (voice shortcuts)

The AI writing assistant is powered by Anthropic Claude. It provides feedback on pacing, dialogue,  
style, and structure. It also answers questions and helps you expand scenes.

---

## 2. System Requirements

| Component | Minimum |
|---|---|
| Operating system | Windows 10 (64-bit) |
| RAM | 4 GB |
| Disk space | 200 MB |
| .NET runtime | .NET 8 (included in installer) |
| Internet | Required for AI features only |
| Screen reader | JAWS 25 (optional — app works without) |
| Voice input | Dragon Professional / JSay 23 (optional) |

---

## 3. Installation

1. Run the installer: `VoiceBookStudio-Setup.exe`
2. Follow the on-screen steps. All fields are labelled for JAWS.
3. Accept the default install location or choose another folder.
4. The installer creates a desktop shortcut and a Start Menu entry.
5. .NET 8 is installed automatically if not already present.

After installation, launch VoiceBook Studio from the Start Menu or desktop.

---

## 4. First Launch

When VoiceBook Studio opens for the first time you will hear:

> "VoiceBook Studio loading… VoiceBook Studio ready."

A **Welcome dialog** will open. It explains the three panels and offers to start a tutorial.  
Press **Start Tutorial** to hear a guided introduction, or press **Skip** to go straight to writing.

The tutorial covers:
- Creating your first project
- Adding a chapter
- Using the AI assistant
- Exporting your manuscript

You can restart the tutorial any time by saying **"Start tutorial"**.

---

## 5. Interface Overview

VoiceBook Studio has three panels:

### Panel 1 — Chapters (left)

Lists all your chapters and sections in book order:
- Front matter (Title Page, Copyright, Dedication, etc.)
- Body chapters
- Back matter (Epilogue, Afterword, Appendix, About the Author)

**Focus**: Press **Ctrl+1** or say "Panel 1".  
Navigate the list with **Up/Down arrow keys**.  
JAWS announces each chapter name and its section group.

### Panel 2 — Writing Editor (centre)

The main text editing area. Type, dictate, or paste your chapter content here.  
Word count is displayed above the editor and announced when it changes.

**Focus**: Press **Ctrl+2** or say "Panel 2".  
Dragon NaturallySpeaking dictation works directly in this field.

### Panel 3 — AI Assistant (right)

Contains three tabs:

- **Chat** — Ask Claude any question; run AI feedback on the current chapter
- **Prompts** — Browse 75 writing prompts sorted into 10 categories
- **Cards** — Saved AI responses (Response Cards) for reuse

**Focus**: Press **Ctrl+3** or say "Panel 3".

### Menu Bar

Access all features via the menu bar: **File**, **Chapters**, **AI**, **Settings**, **Help**.  
Press **Alt** to open the menu bar, then use arrow keys.

### Status Bar

The status bar at the bottom of the window announces every action:  
save confirmations, word counts, AI completions, and errors.  
JAWS reads the status bar automatically via live regions.

---

## 6. Projects

A **project** stores your entire book: all sections, chapter content, and settings.  
Projects are saved as `.vbk` files (JSON format).

### Create a new project

- **Menu**: File → New Project
- **Keyboard**: Ctrl+N
- **Voice**: "New project"

You will be asked to enter a project title. The title appears in the title page when you export.

### Open an existing project

- **Menu**: File → Open Project
- **Keyboard**: Ctrl+O
- **Voice**: "Open project" / "Browse for project" / "Find project"

A file chooser opens. Navigate to your `.vbk` file.

### Save

- **Menu**: File → Save
- **Keyboard**: Ctrl+S
- **Voice**: "Save" / "Save project" / "Save now"

The project auto-saves every 30 seconds when there are unsaved changes.  
Auto-save is silent and never interrupts dictation.

### Save As

- **Menu**: File → Save As
- **Keyboard**: Ctrl+Shift+S
- **Voice**: "Save as"

Creates a copy of the project under a new file name.

### Import a Word document

- **Menu**: File → Import .docx
- **Keyboard**: Ctrl+I
- **Voice**: "Import document"

Imports a `.docx` Word document. If your Anthropic API key is configured,  
Claude will automatically detect chapter breaks and suggest titles.  
You review and confirm each detected chapter before import.

---

## 7. Chapters and Sections

VoiceBook Studio organises your book into sections with 14 types:

### Front Matter (shown first)
| Type | Default title | Purpose |
|---|---|---|
| Title Page | Title Page | Book title, author, series info |
| Copyright | Copyright | Legal/copyright notice |
| Dedication | Dedication | Dedication to person or persons |
| Epigraph | Epigraph | Opening quote |
| Table of Contents | Table of Contents | Chapter listing |
| Foreword | Foreword | Introductory note by another author |
| Preface | Preface | Author's note on the book's origins |
| Introduction | Introduction | Introduction to content |
| Prologue | Prologue | Scene-setting narrative before Chapter 1 |

### Body (main content)
| Type | Default title | Purpose |
|---|---|---|
| Chapter | Chapter | Main narrative chapters |

### Back Matter (shown last)
| Type | Default title | Purpose |
|---|---|---|
| Epilogue | Epilogue | Narrative scene after the story ends |
| Afterword | Afterword | Author's reflection after the story |
| Appendix | Appendix | Supplementary material |
| About the Author | About the Author | Author biography |

### Adding a chapter or section

- **Menu**: Chapters → Add Chapter
- **Keyboard**: Ctrl+A
- **Voice**: "Add chapter" / "New chapter" / "New section"

A dialog opens listing all 14 section types grouped by Front / Body / Back matter.  
Select the type, then confirm or edit the title.

JAWS reads the section type list automatically. Arrow keys navigate the list.

### Renaming

- **Keyboard**: Ctrl+D
- **Voice**: "Rename chapter" / "Rename"

### Deleting

- **Keyboard**: Ctrl+Delete
- **Voice**: "Delete chapter" / "Remove chapter"

A confirmation prompt appears before deleting. Press **Yes** or **No**.

### Reordering

- **Keyboard**: Alt+Up / Alt+Down
- **Voice**: "Move up" / "Move down" / "Move chapter up" / "Move chapter down"

Chapters within the same section group (front/body/back) can be reordered freely.  
The section group structure (front → body → back) is always preserved.

### Changing section type

- **Button**: "Change Type…" in Panel 1
- **Voice**: "Change type" / "Change section type"

Opens the section type picker. After changing, the chapter moves to its new group position.

---

## 8. Writing Editor

The writing editor is a standard text field. All standard editing operations work:

| Operation | Key |
|---|---|
| New paragraph | Enter |
| Tab stop | Tab |
| Undo | Ctrl+Z |
| Redo | Ctrl+Y |
| Select all | Ctrl+A |
| Cut | Ctrl+X |
| Copy | Ctrl+C |
| Paste | Ctrl+V |
| Find | Ctrl+F (note: opens AI feedback, not a find dialog) |
| Jump to start | Ctrl+Home |
| Jump to end | Ctrl+End |

### Word count

Displayed above the editor. Updated as you type.  
JAWS announces the word count via a live region.  
The status bar also shows word count.

### Dragon dictation

When the writing editor is focused (Ctrl+2), Dragon NaturallySpeaking dictates directly into the field.  
All Dragon correction and formatting commands work normally.

### Auto-save

Content is auto-saved to the project file every 30 seconds.  
There is no notification — saving happens silently in the background.

---

## 9. AI Assistant

The AI assistant uses Anthropic Claude. An API key is required (see section 19).

### Running feedback

Focus a chapter, then use one of these:

| Feedback type | Voice command | Keyboard | Button |
|---|---|---|---|
| Comprehensive | "Run feedback" / "Comprehensive" | Ctrl+F | Comprehensive |
| Pacing | "Pacing" / "Check pacing" | — | Pacing |
| Dialogue | "Dialogue" / "Check dialogue" | — | Dialogue |
| Style | "Style" / "Check style" | — | Style |
| Structure | "Structure" / "Check structure" | — | Structure |

Feedback appears in the AI response text box. JAWS announces "Analysis complete" when ready.

### Chat

Type or dictate a question in the chat input box (at the bottom of the Chat tab).  
Press **Enter** or click **Send** to send to Claude.  
Claude has access to the content of the currently selected chapter.

- **Voice**: Say "Send" / "Send message" / "Ask Claude" to send the current input
- After getting a response, say "Insert at cursor" to add it to your chapter

### Inserting AI responses into your chapter

Three insert buttons appear below the AI response:

| Button | Voice command | Where it inserts |
|---|---|---|
| At Cursor | "Insert at cursor" / "Insert here" | Where your cursor was before switching to Panel 3 |
| At Start | "Insert at start" / "Insert at beginning" | Beginning of the chapter |
| At End | "Insert at end" / "Append response" | End of the chapter |

After inserting, focus automatically returns to the editor and JAWS announces the insert position.

### Saving responses as Cards

If you get an AI response you want to keep for later:
1. Click **Save as Card…** (visible when a response is present)
2. Or say "Save card" / "Save response card"
3. Enter a title and category

The card is saved permanently and available in the Cards tab.

---

## 10. Writing Prompts

The **Prompts tab** contains 75 writing prompts in 10 categories:

| Category | Focus |
|---|---|
| A — Character | Character development |
| B — Conflict | Tension and confrontation |
| C — Setting | World-building and atmosphere |
| D — Dialogue | Conversation and voice |
| E — Emotion | Inner life and feeling |
| F — Sensory | Sight, sound, smell, taste, touch |
| G — Backstory | History and motivation |
| H — Subtext | What is left unsaid |
| I — Pacing | Rhythm and timing |
| J — Theme | Meaning and resonance |

### Using a prompt

1. Press **Ctrl+3** to focus the AI panel, then **Ctrl+Tab** to reach the Prompts tab.
2. Or say "Open prompts" / "Show prompts".
3. Select a category from the dropdown (or leave on "All").
4. Navigate the list with arrow keys. JAWS reads each prompt's label and category.
5. Click **Use Prompt** or say "Use prompt".
6. The prompt text loads into the chat input box and you switch to the Chat tab.
7. Edit the prompt if you like, then press Enter or say "Send".

### Using a prompt by ID (voice shortcut)

Each prompt has an ID (e.g. F3, A7, J2).
Say **"Use prompt F3"** to load that prompt directly without navigating the list.

---

## 11. Response Cards

Response Cards are saved AI responses you can insert into any chapter at any time.

### The Cards tab

Navigate to it:
- Press **Ctrl+3** then **Ctrl+Tab** twice
- Or say "Open response cards" / "Cards"

The panel has:
- **Category filter** — filter cards by category
- **Cards list** — all saved cards; arrow keys navigate; JAWS reads title and category
- **Preview** — shows the full text of the selected card
- **Insert Card** — inserts selected card at cursor position in editor
- **Delete Card** — deletes selected card (confirmation prompt)

### Voice commands for cards

| Command | Action |
|---|---|
| "Save card" / "Save response card" | Saves the current AI response as a card |
| "Insert card 2" | Inserts card number 2 from the filtered list |
| "Delete card 1" | Deletes card number 1 (asks for confirmation) |
| "Show Fiction cards" | Filters list to show only Fiction category cards |
| "Open response cards" | Switches to Cards tab |

### Card storage

Cards are saved to:  
`%APPDATA%\VoiceBookStudio\ResponseCards\cards.json`

They persist between sessions. You can back up this file manually.

---

## 12. Export — Word (.docx)

Exports your complete manuscript as a Word `.docx` file with:
- All sections in correct order (front → body → back)
- Title page with book title and author name
- Section headings as Word Heading 1 styles (navigable in Word's accessibility mode)
- Body text as Normal style
- Page breaks between sections
- Copyright, Dedication, and Epigraph formatted appropriately

### How to export

- **Menu**: File → Export Manuscript (.docx)
- **Voice**: "Export Word" / "Export manuscript" / "Export docx"

A file chooser opens. Choose your save location.  
JAWS announces "Exporting manuscript. Please wait" then "Exported: [filename]" when done.

### Opening the .docx with JAWS

The exported `.docx` opens in Word. All headings use Word's built-in Heading 1 style,  
so JAWS's virtual cursor can navigate by heading (H key in virtual mode).

---

## 13. Export — PDF

Exports your manuscript as a formatted PDF with:
- Title page (title, author, any title page content you've added)
- Running header on each page showing the book title
- Page numbers in the footer
- Headings in dark blue, body text in near-black
- Dedication and Epigraph sections centred

### How to export

- **Menu**: File → Export as PDF
- **Voice**: "Export PDF" / "Export as PDF" / "Create PDF"

A file chooser opens. Choose your save location.  
JAWS announces "Exporting PDF. Please wait" then "Exported: [filename]" when done.

### Accessibility of the PDF

The exported PDF is suitable for reading. For a fully tagged accessible PDF (with JAWS navigation),  
open the exported PDF in Adobe Acrobat Pro and run **Accessibility → Make Accessible**.

---

## 14. Keyboard Shortcuts

### File

| Shortcut | Action |
|---|---|
| Ctrl+N | New project |
| Ctrl+O | Open project |
| Ctrl+S | Save |
| Ctrl+Shift+S | Save As |
| Ctrl+I | Import .docx |

### Chapters

| Shortcut | Action |
|---|---|
| Ctrl+A | Add chapter / section |
| Ctrl+D | Rename chapter |
| Ctrl+Delete | Delete chapter |
| Alt+Up | Move chapter up |
| Alt+Down | Move chapter down |

### Panel focus

| Shortcut | Action |
|---|---|
| Ctrl+1 | Focus Chapter list |
| Ctrl+2 | Focus Writing editor |
| Ctrl+3 | Focus AI Assistant (Chat input) |

### AI

| Shortcut | Action |
|---|---|
| Ctrl+F | Run comprehensive feedback |

### Navigation

| Key | Action |
|---|---|
| Tab | Next control |
| Shift+Tab | Previous control |
| Enter | Activate focused control |
| Escape | Close dialog / cancel |
| Alt | Open menu bar |
| Ctrl+Tab | Next tab in AI panel |
| Ctrl+Shift+Tab | Previous tab in AI panel |

---

## 15. Voice Commands

VoiceBook Studio's voice command router handles these spoken phrases.  
Connect via Dragon Professional custom commands, JSay scripts, or via the built-in  
microphone (active when Dragon is not detected — see sections 16 and 17).

### Context-sensitive help

```
What can I say          What can I say here     What can I do here
List commands           Help commands           Commands
Voice commands          Help me
```

### Panel navigation

```
Panel 1         Go to panel 1       Panel one
Panel 2         Go to panel 2       Panel two
Panel 3         Go to panel 3       Panel three
Go to chat      Chat tab            Switch to chat      Chat
Open prompt library     Show prompts
Open response cards     Show response cards     Cards
```

### Project

```
New project         Create project          Start new project
Save                Save project            Save now            Save file
Save as             Save project as
Open project        Browse for project      Find project
Import document     Open document           Bring in document
```

### Chapters

```
Add chapter         New chapter             New section         Write new chapter
Delete chapter      Remove chapter          Erase chapter       Get rid of chapter
Rename chapter      Rename
Move up             Move chapter up
Move down           Move chapter down
Change type         Change section type
```

### AI feedback — chapter

```
Feedback            Run feedback        Get feedback    Comprehensive
Pacing              Check pacing        Pacing feedback
Dialogue            Check dialogue      Dialogue feedback
Style               Check style         Style feedback
Structure           Check structure     Structure feedback
Send                Send message        Send chat       Ask Claude
```

### AI feedback — whole book

```
Analyse book        Analyze book        Book analysis
Book feedback       Full book feedback  Review book
Whole book          Entire book feedback
```

### Insert AI response

```
Insert at cursor    Insert here         Insert response
Insert at start     Insert at beginning
Insert at end       Append response
```

### Prompt library

```
Prompt library          What prompts do I have
Show prompt categories  List prompt categories
Read prompt [A–J]       e.g. "Read prompt C"
Add prompt              New prompt              Create prompt
Use prompt [ID]         e.g. "Use prompt F3"    (Dragon / chat box only)
Prompt [ID]             e.g. "Prompt A1"        (Dragon / chat box only)
```

### Response cards

```
Card categories         What cards do I have
Show card categories    List card categories
Read card [A–J]         e.g. "Read card B"
Save card               Save response card
Insert card [N]         e.g. "Insert card 2"
Delete card [N]         e.g. "Delete card 1"
Show [category] cards   e.g. "Show Fiction cards"
```

### Feedback library

```
Open feedback library   Feedback library        Show feedback library
Feedback categories     What feedback do I have
Show feedback categories                        List feedback categories
Read feedback [A–E]                             e.g. "Read feedback B"
Read feedback comprehensive     Read feedback pacing
Read feedback dialogue          Read feedback style     Read feedback structure
Read my comprehensive feedback  Read my pacing feedback
Read my dialogue feedback       Read my style feedback  Read my structure feedback
Read [ID]               e.g. "Read A1"          (Dragon / chat box only)
Resume reading          Continue reading
```

### Export

```
Export Word         Export manuscript       Export docx     Save as word
Export PDF          Export as PDF           Create PDF
```

### Settings

```
Settings            Open settings       Show settings
Open preferences    Preferences
Toggle voice        Voice on            Voice off
Set API key         Configure AI        Enter API key      API key
Set project folder  Change project folder   Project folder
```

### Tutorial

```
Start tutorial
Next            Previous        Repeat          Exit tutorial
Continue        Yes             Skip step       Skip
```

---

## 16. Dragon Professional Setup

See `Docs/DragonCommands.md` for the complete Dragon configuration guide.

### Quick setup

1. Add keyboard shortcut commands in **Dragon Tools → Add New Command → MyCommand**:
   - "Chapter panel" → Ctrl+1
   - "Editor panel" → Ctrl+2
   - "AI panel" → Ctrl+3
   - "Save project" → Ctrl+S
   - "Add chapter" → Ctrl+A
   - "Run feedback" → Ctrl+F

2. Focus the editor with Ctrl+2, then dictate normally.

3. Use Dragon's native **"Click [button name]"** to click any labelled button.

4. Add technical terms to Dragon vocabulary:  
   VoiceBook, front matter, back matter, Prologue, Epilogue, Epigraph, Afterword

### Full Text Control
Dragon uses UI Automation for WPF apps. If recognition is poor in the editor:
- Dragon settings → Miscellaneous → Enable **Use UI Automation**

---

## 17. JSay 23 Setup

See `Docs/JSayScripts.md` for the complete JSay configuration guide.

### Quick setup

1. Open JSay Settings → Scripts
2. Add these entries:

```
"chapter panel"   = {ctrl+1}
"editor panel"    = {ctrl+2}
"AI panel"        = {ctrl+3}
"save project"    = {ctrl+s}
"add chapter"     = {ctrl+a}
"run feedback"    = {ctrl+f}
```

3. Set Target Application to VoiceBook Studio.
4. Enable Continuous recognition mode.

---

## 18. JAWS 25 Setup

See `Docs/JAWSSettings.md` for the complete JAWS configuration guide.

### Quick setup

1. Open VoiceBook Studio.
2. Press **JAWS key + F2** to open Settings Center for VoiceBook Studio.
3. Set **Virtual PC Cursor** to Off.
4. In VoiceBook Settings menu, set **App Voice** to Off.
5. JAWS will now read all live regions, status bar updates, and announcements.

### Navigation tip
Use **Ctrl+1 / Ctrl+2 / Ctrl+3** instead of Tab to jump between panels.  
This is faster and avoids the toolbar buttons.

---

## 19. AI Configuration (Anthropic API key)

AI features require a free Anthropic account and API key.

### Getting a key

1. Go to **console.anthropic.com** and create an account
2. Go to **API Keys** and click **Create Key**
3. Copy the key (starts with `sk-ant-...`)

### Entering the key in VoiceBook Studio

1. Click the **AI Not Set** button in the toolbar, or
2. Go to **AI menu → API Key**
3. Paste your key and click OK

The key is stored securely in the Windows Credential Store.  
JAWS announces "AI key saved. AI features are now active."

### Usage and cost

Claude API usage is pay-per-use. For casual writing feedback:
- A typical feedback request costs less than $0.01
- Monthly cost for daily use is typically $1–5 USD
- Free tier credits are available for new accounts

---

## 20. Settings

### App Voice (text-to-speech)

Controls the app's built-in SAPI voice. This is separate from JAWS.

- **Settings menu → App voice: On/Off**
- **Voice command**: "Toggle voice" / "Voice on" / "Voice off"

When using JAWS, set App Voice to Off.  
When not using a screen reader, set App Voice to On for spoken feedback.

### API Key

Configure the Anthropic Claude API key.

- **AI menu → API Key**
- **Toolbar**: Click the AI status button
- **Voice**: "Set API key" / "API key"

### Auto-save

Auto-save is always on and cannot be disabled.  
The interval is 30 seconds. Projects are only auto-saved if they have a file path  
(i.e. you have saved at least once using Ctrl+S or File → Save As).

---

## 21. Troubleshooting

### JAWS does not announce status updates

1. Ensure Virtual PC Cursor is Off for VoiceBook Studio (JAWS key + F2)
2. Check that "Announce live regions" is On in JAWS verbosity settings
3. The status bar uses a live region — it should announce automatically

### Dragon does not dictate into the editor

1. Make sure the editor is focused: press Ctrl+2
2. Verify Dragon's Full Text Control is using UI Automation (Dragon Settings → Misc)
3. Try clicking directly in the editor area, then start dictating

### AI button is greyed out

The AI button is inactive if:
- No chapter is selected (click a chapter in Panel 1 first)
- No API key is configured (click the AI Not Set button to add one)

### Export produces an error

- Check that the output folder is writable
- Close any other applications that may have the file open
- Try saving to the Desktop first to rule out permission issues

### Auto-save does not work

Auto-save only works after the project has been saved at least once with a file name.  
Use Ctrl+S or File → Save As to save the project file, then auto-save will work.

### App voice and JAWS overlap

Go to **Settings menu → App Voice** and turn it off.  
JAWS handles all audio independently.

### Response cards list is empty

Cards are stored in:  
`%APPDATA%\VoiceBookStudio\ResponseCards\cards.json`

If the file does not exist, save your first card to create it.  
If the file is corrupted, delete it and start fresh.

### Section type dialog is hard to read

The dialog lists types grouped by Front Matter, Body, Back Matter.  
If JAWS reads the group headers strangely, navigate using the Up/Down arrow keys —  
JAWS will read each item's name and its group (e.g. "Dedication — Front Matter").

---

## Appendix A — Section Types Reference

| Section type | Group | Typical position |
|---|---|---|
| Title Page | Front matter | First |
| Copyright | Front matter | Second |
| Dedication | Front matter | Third |
| Epigraph | Front matter | Fourth |
| Table of Contents | Front matter | Fifth |
| Foreword | Front matter | Sixth |
| Preface | Front matter | Seventh |
| Introduction | Front matter | Eighth |
| Prologue | Front matter | Ninth (last front matter) |
| Chapter | Body | All body chapters |
| Epilogue | Back matter | First back |
| Afterword | Back matter | Second back |
| Appendix | Back matter | Third back |
| About the Author | Back matter | Last |

---

## Appendix B — File Locations

| Item | Location |
|---|---|
| Project files | Wherever you saved them (.vbk) |
| Response cards | `%APPDATA%\VoiceBookStudio\ResponseCards\cards.json` |
| API key | Windows Credential Store (not a file) |
| App settings | `%APPDATA%\VoiceBookStudio\settings.json` |
| Writing prompts | In the application folder: `Data\PromptLibrary\prompts.json` |

---

*VoiceBook Studio is designed for Anna's dad and everyone who writes with their voice.*
