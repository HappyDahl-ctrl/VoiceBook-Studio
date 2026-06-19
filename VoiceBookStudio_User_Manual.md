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
19. [AI Configuration (Anthropic API Key)](#19-ai-configuration-anthropic-api-key)
20. [Settings](#20-settings)
21. [Troubleshooting](#21-troubleshooting)
22. [Appendix A — Section Types Reference](#appendix-a--section-types-reference)
23. [Appendix B — File Locations](#appendix-b--file-locations)
24. [Appendix C — Prompt Library Reference](#appendix-c--prompt-library-reference)

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
| Operating system | Windows 10 64-bit |
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

A **Welcome dialog** will open. It explains the three panels and offers to start a guided tutorial.
Press **Start Tutorial** to hear a step-by-step introduction, or press **Skip** to go straight to writing.

Check **Don't show again** in the Welcome dialog to skip it on future launches.

The tutorial covers:
- Creating your first project
- Adding a chapter
- Using the AI assistant
- Exporting your manuscript

You can restart the tutorial at any time by saying **"Start tutorial"**.

After the Welcome dialog, a **Project Selection dialog** appears. Choose an existing project to open,
or create a new one.

---

## 5. Interface Overview

VoiceBook Studio has three panels:

### Panel 1 — Chapters (left)

Lists all your chapters and sections in book order:
- Front matter (Title Page, Copyright, Dedication, and more)
- Body chapters
- Back matter (Epilogue, Afterword, Appendix, About the Author)

**Focus**: Press **Ctrl+1** or say "Panel 1".  
Navigate the list with **Up/Down arrow keys**.  
JAWS announces each chapter name and its section group.

### Panel 2 — Writing Editor (centre)

The main text editing area. Type, dictate, or paste your chapter content here.  
Word count is displayed above the editor and updated live.

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

You will be asked to enter a project title. The title appears on the title page when you export.

### Open an existing project

- **Menu**: File → Open Project
- **Keyboard**: Ctrl+O
- **Voice**: "Open project"

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

Imports a `.docx` Word document into your project. Chapter detection runs automatically:

1. **Heading styles present** — If your document uses Word Heading 1 or Heading 2 styles,
   chapters are detected instantly without an AI call. The status bar shows the number found.
2. **No heading styles** — If headings are not used, Claude analyses the text and suggests
   natural chapter breaks based on scene changes, time jumps, and topic shifts.

In both cases, a **Chapter Confirmation dialog** appears. Review the detected titles, edit any
you want to change, then click Accept All to import all chapters, or Import as Single Chapter
to bring everything in as one block.

---

## 7. Chapters and Sections

VoiceBook Studio organises your book into sections with 14 types:

### Front Matter (shown first)

| Type | Purpose |
|---|---|
| Title Page | Book title, author, series info |
| Copyright | Legal / copyright notice |
| Dedication | Dedication to a person or persons |
| Epigraph | Opening quote |
| Table of Contents | Chapter listing |
| Foreword | Introductory note by another author |
| Preface | Author's note on the book's origins |
| Introduction | Introduction to the content |
| Prologue | Scene-setting narrative before Chapter 1 |

### Body (main content)

| Type | Purpose |
|---|---|
| Chapter | Main narrative chapters |

### Back Matter (shown last)

| Type | Purpose |
|---|---|
| Epilogue | Narrative scene after the story ends |
| Afterword | Author's reflection after the story |
| Appendix | Supplementary material |
| About the Author | Author biography |

### Adding a chapter or section

- **Menu**: Chapters → Add Chapter
- **Keyboard**: Ctrl+A
- **Voice**: "Add chapter" / "New chapter" / "New section"

A dialog opens listing all 14 section types grouped by Front / Body / Back matter.  
Select the type, then confirm or edit the suggested title.

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

Chapters within the same section group (front / body / back) can be reordered freely.  
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
| Undo | Ctrl+Z |
| Redo | Ctrl+Y |
| Select all | Ctrl+A |
| Cut | Ctrl+X |
| Copy | Ctrl+C |
| Paste | Ctrl+V |
| Jump to start | Ctrl+Home |
| Jump to end | Ctrl+End |

### Word count

Displayed above the editor. Updated as you type.  
JAWS announces the word count via a live region.

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

Focus a chapter, then use any of these:

| Feedback type | Voice command | Keyboard | Button |
|---|---|---|---|
| Comprehensive | "Run feedback" / "Comprehensive" | Ctrl+F | Comprehensive |
| Pacing | "Pacing" / "Check pacing" | — | Pacing |
| Dialogue | "Dialogue" / "Check dialogue" | — | Dialogue |
| Style | "Style" / "Check style" | — | Style |
| Structure | "Structure" / "Check structure" | — | Structure |

Feedback appears in the AI response text box. JAWS announces "Analysis complete" when ready.

### Chat

Type or dictate a question in the chat input box at the bottom of the Chat tab.  
Press **Enter** or click **Send** to send to Claude.  
Claude has access to the content of the currently selected chapter.

- **Voice**: Say "Send" / "Send message" / "Ask Claude" to send the current input.
- After getting a response, say "Insert at cursor" to add it to your chapter.

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
1. Say "Save card" or "Save response card", or click **Save as Card…**
2. Enter a title and category when prompted.

The card is saved permanently and available in the Cards tab (see section 11).

---

## 10. Writing Prompts

The **Prompts tab** contains 75 writing prompts in 10 categories:

| ID prefix | Category | Count | Focus |
|---|---|---|---|
| A | Editing | 5 | Grammar, clarity, prose tightening |
| B | Fiction | 5 | Character voice, tension, show-don't-tell |
| C | Structure | 5 | Chapter outlines, transitions, hooks |
| D | Non-fiction | 5 | Arguments, readability, conclusions |
| E | Research | 5 | Fact-checking, sources, citations |
| F | Description & Atmosphere | 10 | Sensory detail, setting, weather |
| G | Dialogue & Voice | 10 | Subtext, conflict, character voice |
| H | Plot & Structure | 10 | Twists, stakes, cliffhangers, arcs |
| I | Character Development | 10 | Motivation, secrets, contradictions |
| J | Opening Hooks & Endings | 10 | First lines, closings, resolutions |

### Using a prompt

1. Press **Ctrl+3** to focus the AI panel, then **Ctrl+Tab** to reach the Prompts tab.
   Or say "Open prompts" / "Show prompts".
2. Select a category from the dropdown (or leave on "All").
3. Navigate the list with arrow keys. JAWS reads each prompt's title and category.
4. Click **Use Prompt** or say "Use prompt".
5. The prompt text loads into the chat input box and you switch to the Chat tab automatically.
6. Edit the prompt if you like, then press Enter or say "Send".

### Using a prompt by ID (voice shortcut)

Every prompt has a two-character ID (letter + number), for example F3, A2, J8.  
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
- **Insert Card** — inserts the selected card at your cursor position in the editor
- **Delete Card** — deletes the selected card (confirmation prompt)

### Voice commands for cards

| Command | Action |
|---|---|
| "Save card" / "Save response card" | Saves the current AI response as a card |
| "Insert card 2" | Inserts card number 2 from the current filtered list |
| "Delete card 1" | Deletes card number 1 (asks for confirmation) |
| "Show Fiction cards" | Filters the list to show only Fiction category cards |
| "Open response cards" | Switches to the Cards tab |

### Card storage

Cards are saved to:  
`%APPDATA%\VoiceBookStudio\ResponseCards\cards.json`

They persist between sessions. You can back this file up manually.

---

## 12. Export — Word (.docx)

Exports your complete manuscript as a Word `.docx` file with:
- All sections in correct order (front → body → back)
- Title page with book title and author name
- Section headings as Word Heading 1 styles (navigable with JAWS in Word)
- Body text as Normal style
- Page breaks between sections
- Copyright, Dedication, and Epigraph sections formatted appropriately

### How to export

- **Menu**: File → Export Manuscript (.docx)
- **Voice**: "Export Word" / "Export manuscript" / "Export docx"

A file chooser opens. Choose your save location.  
JAWS announces "Exporting manuscript. Please wait" then "Exported: [filename]" when done.

### Opening the .docx with JAWS

The exported `.docx` opens in Word. All headings use Word's built-in Heading 1 style,
so JAWS's virtual cursor can navigate by heading (press H in virtual mode).

---

## 13. Export — PDF

Exports your manuscript as a formatted PDF with:
- Title page (title, author, and any title page content you have added)
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

The exported PDF is suitable for reading. For a fully tagged accessible PDF with JAWS heading
navigation, open the exported file in Adobe Acrobat Pro and run **Accessibility → Make Accessible**.

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
| Ctrl+1 | Focus Chapter list (Panel 1) |
| Ctrl+2 | Focus Writing editor (Panel 2) |
| Ctrl+3 | Focus AI Assistant chat input (Panel 3) |

### AI

| Shortcut | Action |
|---|---|
| Ctrl+F | Run comprehensive feedback on current chapter |

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
| F1 | Read help text for focused control (JAWS) |

---

## 15. Voice Commands

VoiceBook Studio handles these spoken phrases via its voice command router.  
Connect via Dragon Professional custom commands, JSay scripts, or any voice input tool
mapped to keyboard shortcuts (see sections 16 and 17).

### Panel navigation

```
Panel 1           Go to panel 1         Panel one
Panel 2           Go to panel 2         Panel two
Panel 3           Go to panel 3         Panel three
Go to chat        Chat tab              Chat
Open prompts      Show prompts
Open response cards                     Cards
```

### Project

```
Save              Save project          Save now          Save file
New project
```

### Chapters

```
Add chapter       New chapter           New section
Delete chapter    Remove chapter
Rename chapter    Rename
Move up           Move chapter up
Move down         Move chapter down
Change type       Change section type
```

### AI feedback

```
Feedback          Run feedback          Get feedback      Comprehensive
Pacing            Check pacing          Pacing feedback
Dialogue          Check dialogue        Dialogue feedback
Style             Check style           Style feedback
Structure         Check structure       Structure feedback
Send              Send message          Send chat         Ask Claude
```

### Insert AI response

```
Insert at cursor      Insert here       Insert response
Insert at start       Insert at beginning
Insert at end         Append response
```

### Prompts

```
Open prompt library       Show prompts
Use prompt [ID]           e.g. "Use prompt F3"
```

### Response cards

```
Save card                 Save response card
Insert card [N]           e.g. "Insert card 2"
Delete card [N]           e.g. "Delete card 1"
Show [category] cards     e.g. "Show Fiction cards"
```

### Export

```
Export Word           Export manuscript       Export docx       Export as word
Export PDF            Export as PDF           Create PDF
```

### Settings

```
Toggle voice          Voice on          Voice off
Set API key           API key
```

### Tutorial

```
Start tutorial
Next              Previous          Repeat            Exit tutorial
```

---

## 16. Dragon Professional Setup

See `Docs/DragonCommands.md` for the complete Dragon configuration guide.

### Quick setup

1. In **Dragon Tools → Add New Command → MyCommand**, add keyboard shortcut commands:
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
- Dragon Settings → Miscellaneous → enable **Use UI Automation**

---

## 17. JSay 23 Setup

See `Docs/JSayScripts.md` for the complete JSay configuration guide.

### Quick setup

1. Open JSay Settings → Scripts.
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
This is faster and avoids passing through toolbar buttons.

---

## 19. AI Configuration (Anthropic API Key)

AI features require a free Anthropic account and API key.

### Getting a key

1. Go to **console.anthropic.com** and create an account.
2. Go to **API Keys** and click **Create Key**.
3. Copy the key — it begins with `sk-ant-`.

### Entering the key in VoiceBook Studio

1. Click the **AI Not Set** button in the toolbar, or
2. Go to **AI menu → API Key**.
3. Paste your key and click OK.

The key is stored securely in the Windows Credential Store.  
JAWS announces "AI key saved. AI features are now active."

### Usage and cost

Claude API usage is pay-per-use. For typical writing work:
- A single feedback request costs less than $0.01
- Monthly cost for daily use is typically $1–5 USD
- Free tier credits are available for new accounts

---

## 20. Settings

### App Voice (text-to-speech)

Controls the app's built-in SAPI voice. This is separate from JAWS.

- **Settings menu → App voice: On / Off**
- **Voice command**: "Toggle voice" / "Voice on" / "Voice off"

When using JAWS, set App Voice to Off.  
When not using a screen reader, set App Voice to On for spoken status feedback.

### API Key

Configure the Anthropic Claude API key.

- **AI menu → API Key**
- **Toolbar**: Click the AI status button
- **Voice**: "Set API key" / "API key"

### Auto-save

Auto-save is always on. The interval is 30 seconds. Projects are only auto-saved if they have
a file path — meaning you have saved at least once using Ctrl+S or File → Save As.

---

## 21. Troubleshooting

### JAWS does not announce status updates

1. Ensure Virtual PC Cursor is Off for VoiceBook Studio (JAWS key + F2).
2. Check that "Announce live regions" is On in JAWS verbosity settings.
3. The status bar uses a live region and should announce automatically.

### Dragon does not dictate into the editor

1. Make sure the editor is focused: press Ctrl+2.
2. Verify Dragon's Full Text Control is using UI Automation (Dragon Settings → Misc).
3. Try clicking directly in the editor area, then start dictating.

### AI button is greyed out

The AI button is inactive when:
- No chapter is selected — click a chapter in Panel 1 first.
- No API key is configured — click the **AI Not Set** button to add one.

### Export produces an error

- Check that the output folder is writable.
- Close any other applications that may have the file open.
- Try saving to the Desktop first to rule out permission issues.

### Auto-save does not work

Auto-save only works after the project has been saved at least once with a file name.  
Use Ctrl+S or File → Save As to save the project file, then auto-save will work.

### App voice and JAWS overlap

Go to **Settings menu → App Voice** and turn it Off.  
JAWS handles all audio independently.

### Response cards list is empty

Cards are stored in:  
`%APPDATA%\VoiceBookStudio\ResponseCards\cards.json`

If the file does not exist, save your first card to create it.  
If the file is corrupted, delete it and start fresh.

### Tutorial does not appear on first launch

If the Welcome dialog was closed with "Don't show again" checked, say **"Start tutorial"**
at any time to restart it.

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
| API key | Windows Credential Store (not a plain file) |
| App settings | `%APPDATA%\VoiceBookStudio\settings.json` |
| Writing prompts | Application folder: `Data\PromptLibrary\prompts.json` |

---

## Appendix C — Prompt Library Reference

75 prompts across 10 categories. Use any prompt by saying "Use prompt [ID]".

### A — Editing (A1–A5)

| ID | Title |
|---|---|
| A1 | Proofread for grammar and spelling |
| A2 | Tighten prose without losing voice |
| A3 | Check dialogue punctuation |
| A4 | Find repetitive words |
| A5 | Improve sentence variety |

### B — Fiction (B1–B5)

| ID | Title |
|---|---|
| B1 | Strengthen character voice |
| B2 | Add sensory details |
| B3 | Increase tension |
| B4 | Show don't tell |
| B5 | Improve pacing |

### C — Structure (C1–C5)

| ID | Title |
|---|---|
| C1 | Outline this chapter |
| C2 | Identify the turning point |
| C3 | Check scene structure |
| C4 | Improve transitions |
| C5 | Strengthen opening hook |

### D — Non-fiction (D1–D5)

| ID | Title |
|---|---|
| D1 | Clarify main argument |
| D2 | Add supporting examples |
| D3 | Improve readability |
| D4 | Strengthen conclusion |
| D5 | Check logical flow |

### E — Research (E1–E5)

| ID | Title |
|---|---|
| E1 | Fact-check this passage |
| E2 | Suggest credible sources |
| E3 | Identify unsupported claims |
| E4 | Add citations |
| E5 | Verify historical accuracy |

### F — Description & Atmosphere (F1–F10)

| ID | Title |
|---|---|
| F1 | Expand with sensory details |
| F2 | Describe the setting |
| F3 | Add metaphors and similes |
| F4 | Deepen the atmosphere |
| F5 | Show character through environment |
| F6 | Create vivid action |
| F7 | Write the weather |
| F8 | Describe a character's appearance |
| F9 | Build tension through setting |
| F10 | Paint the emotional landscape |

### G — Dialogue & Voice (G1–G10)

| ID | Title |
|---|---|
| G1 | Sharpen the dialogue |
| G2 | Add conflict to conversation |
| G3 | Differentiate character voices |
| G4 | Write subtext |
| G5 | Show emotion through dialogue |
| G6 | Write realistic argument |
| G7 | Add dialogue beats |
| G8 | Write witty banter |
| G9 | Craft a confession or revelation |
| G10 | Improve exposition in dialogue |

### H — Plot & Structure (H1–H10)

| ID | Title |
|---|---|
| H1 | Brainstorm plot twists |
| H2 | Find the story stakes |
| H3 | Create obstacles |
| H4 | Write the next scene |
| H5 | Reverse engineer the climax |
| H6 | Add a subplot |
| H7 | Strengthen the inciting incident |
| H8 | Build to a cliffhanger |
| H9 | Create cause and effect |
| H10 | Map character arcs |

### I — Character Development (I1–I10)

| ID | Title |
|---|---|
| I1 | Deepen character motivation |
| I2 | Create character contradictions |
| I3 | Write an internal monologue |
| I4 | Develop character relationships |
| I5 | Give the character a secret |
| I6 | Show character growth |
| I7 | Create a character quirk or habit |
| I8 | Explore character backstory |
| I9 | Test character values |
| I10 | Write character reactions |

### J — Opening Hooks & Endings (J1–J10)

| ID | Title |
|---|---|
| J1 | Write a compelling opening line |
| J2 | Strengthen the first paragraph |
| J3 | Open with dialogue |
| J4 | Start in medias res |
| J5 | Create mystery in the opening |
| J6 | Write a satisfying chapter ending |
| J7 | Craft an emotional ending |
| J8 | Stick the landing |
| J9 | Write a circular ending |
| J10 | Create bittersweet resolution |

---

*VoiceBook Studio — designed for writers who write with their voice.*
