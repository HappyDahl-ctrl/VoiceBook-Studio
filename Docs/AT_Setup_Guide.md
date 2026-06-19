# VoiceBook Studio — Assistive Technology Setup Guide

**Dragon Professional | JSay 23 | JAWS 25**

Version 1.0

---

## Contents

1. [How VoiceBook Handles Voice Input](#1-how-voicebook-handles-voice-input)
2. [Dragon Professional Setup](#2-dragon-professional-setup)
3. [JSay 23 Setup](#3-jsay-23-setup)
4. [JAWS 25 Setup](#4-jaws-25-setup)
5. [Using All Three Together](#5-using-all-three-together)

---

## 1. How VoiceBook Handles Voice Input

VoiceBook Studio has three voice input paths that work independently:

**Path A — Built-in microphone (no Dragon / JSay needed)**
VoiceBook starts its own Windows Speech Recognition engine when Dragon and JSay are not detected. You can say any command from the Master Voice Commands list at any time. The grammar is pre-loaded so the app always hears your commands, even while writing.

**Path B — Dragon Professional**
Dragon dictates text directly into the writing editor (Panel 2) using Text Services Framework. When Dragon is running, VoiceBook's built-in mic turns off automatically. You use Dragon for dictation and Dragon custom commands (MyCommands) for navigation shortcuts. VoiceBook receives Dragon commands either as keyboard shortcuts or as text typed into the chat input box.

**Path C — JSay 23**
JSay maps voice phrases to keyboard keystroke sequences. VoiceBook sees only the resulting keystrokes — the same as pressing the keyboard shortcut manually. JSay and JAWS run side by side.

**Where commands are received:**
- Keyboard shortcuts (Ctrl+1, Ctrl+S, F4, etc.) — always active, any focus
- Chat input box — type or dictate a command, press Enter — VoiceBook routes it
- Built-in mic — active when Dragon/JSay not detected

---

## 2. Dragon Professional Setup

### 2.1 How Dragon Works with VoiceBook

Dragon Professional integrates with VoiceBook at two levels:

1. **Full Text Control in the editor** — Dragon dictates directly into Panel 2 (the writing editor). All Dragon correction, formatting, and dictation commands work normally. This requires no setup.

2. **Custom commands for navigation and control** — Dragon MyCommands let you say phrases like "chapter panel" or "run feedback" and have Dragon press the corresponding keyboard shortcut. These are set up once and work every time VoiceBook is open.

### 2.2 Enable Full Text Control

If Dragon does not dictate into the writing editor:

1. Open Dragon Settings (DragonBar → Tools → Options)
2. Go to Miscellaneous
3. Enable **Use UI Automation**
4. Click OK and restart Dragon

VoiceBook's editor is a WinForms RichTextBox hosted in a WPF window. UI Automation is the correct integration mode.

### 2.3 Add VoiceBook Words to Dragon Vocabulary

Say these terms so Dragon learns them correctly:

1. DragonBar → Words → Add New Word or Phrase
2. Add each of the following as a spoken form:

```
VoiceBook
front matter
back matter
Prologue
Epilogue
Epigraph
Afterword
Appendix
Dedication
```

### 2.4 Create MyCommands — Step by Step

Open the MyCommands editor:

- DragonBar → Tools → Add New Command

Or, for the full editor:

- DragonBar → Tools → Command Browser → New

**For each command below:**
1. Click **New**
2. Set **Name** (this is the phrase you will say)
3. Set **Command type** to **Keystroke**
4. Enter the keystroke in the **Key** field
5. Set **Application** to **VoiceBook Studio** (important — this limits the command to VoiceBook only)
6. Click **Save**

### 2.5 Dragon MyCommands — Panel Navigation

These are the most important commands. Set them up first.

| Say this | Keystroke | What it does |
|---|---|---|
| Chapter panel | {Ctrl+1} | Focus Chapter list (Panel 1) |
| Editor panel | {Ctrl+2} | Focus Writing editor (Panel 2) |
| A I panel | {Ctrl+3} | Focus AI Assistant (Panel 3) |

Note: Say "A I panel" (two letters) to avoid Dragon hearing "AI" as "A.I." punctuation.

### 2.6 Dragon MyCommands — Chapter Management

| Say this | Keystroke | What it does |
|---|---|---|
| Add chapter | {Ctrl+A} | Open Add Chapter dialog |
| Rename chapter | {Ctrl+D} | Rename selected chapter |
| Delete chapter | {Ctrl+Del} | Delete selected chapter |
| Move chapter up | {Alt+Up} | Move chapter up in list |
| Move chapter down | {Alt+Down} | Move chapter down in list |

### 2.7 Dragon MyCommands — File Operations

| Say this | Keystroke | What it does |
|---|---|---|
| Save project | {Ctrl+S} | Save |
| New project | {Ctrl+N} | New project |
| Open project | {Ctrl+O} | Open project |
| Save as project | {Ctrl+Shift+S} | Save As |
| Import document | {Ctrl+I} | Import Word .docx |

### 2.8 Dragon MyCommands — AI and Accessibility

| Say this | Keystroke | What it does |
|---|---|---|
| Run feedback | {Ctrl+F} | Comprehensive AI feedback |
| Read paragraph | {F4} | Read paragraph at cursor |
| Chapter title | {F5} | Announce chapter title |
| Next chapter | {F6} | Go to next chapter |
| Previous chapter | {F7} | Go to previous chapter |
| Read chapter | {F8} | Read entire chapter |
| App status | {F9} | Announce application status |
| Stop reading | {Ctrl+F4} | Stop reading aloud |

### 2.9 Dragon MyCommands — Chat Input Commands

For commands that do not have keyboard shortcuts (such as "insert at cursor" or "use prompt F3"), use a **Step-by-Step** MyCommand that focuses the chat input, types the command, and presses Enter.

**Command type: Step-by-Step**

Example — "Insert at cursor":

1. Name: **Insert at cursor**
2. Command type: **Step-by-Step**
3. Add steps:
   - Step 1: **Keystroke** → {Ctrl+3}  *(focus Panel 3 / chat input)*
   - Step 2: **Type text** → insert at cursor
   - Step 3: **Keystroke** → {Enter}
4. Application: VoiceBook Studio
5. Save

Repeat for any other voice-router commands you want to trigger without typing. The full list of voice commands that benefit from this treatment:

| Say this | Text to type | Action |
|---|---|---|
| Insert at cursor | insert at cursor | Insert AI response at cursor |
| Insert at start | insert at start | Insert at chapter beginning |
| Insert at end | insert at end | Insert at chapter end |
| Export manuscript | export word | Export as Word .docx |
| Export PDF | export pdf | Export as PDF |
| Save card | save card | Save AI response as card |
| Toggle voice | toggle voice | Toggle app TTS on/off |
| App settings | settings | Open Settings dialog |
| Export as word | export word | Export as Word .docx |

### 2.10 Dragon Command for "Use Prompt" with Variable

Dragon supports list commands that accept spoken variables. This lets you say "Use prompt F3" without creating 75 separate commands.

**Method: Script command using Dragon's List type**

1. In Command Browser, click **New**
2. Name: **Use prompt [prompt ID]**
3. Command type: **Step-by-Step**
4. Add steps:
   - Step 1: **Keystroke** → {Ctrl+3}
   - Step 2: **Type text** → use prompt
   - Step 3: **Type text** → {Dictate} *(this inserts the spoken variable)*

   Alternatively, use a Text command that captures what you say after "use prompt".

**Simpler approach — individual commands for common prompts:**

If scripted variables are difficult, add individual commands for your most-used prompts:

| Say this | Text to type |
|---|---|
| Use prompt A one | use prompt a1 |
| Use prompt B three | use prompt b3 |
| Use prompt F three | use prompt f3 |

*(Add whichever prompt IDs you use most.)*

### 2.11 Dictating Into the Editor

No setup is needed for dictation. When Panel 2 (the writing editor) is focused:

- Dragon dictates your words directly as text
- Dragon correction commands work normally ("Scratch that", "Correct [word]")
- Dragon formatting commands work ("New line", "New paragraph", "Cap that", "Bold that")
- Say "editor panel" (your MyCommand) to focus the editor before dictating

### 2.12 Recommended Dragon Workflow

```
1. Open VoiceBook Studio
2. Say "Chapter panel" → navigate to the chapter to work on
3. Say "Editor panel" → Dragon dictates into the editor
4. Dictate your content normally
5. Say "Chapter panel" to return to chapter navigation when done
6. Say "Run feedback" (Ctrl+F shortcut) to request AI feedback
7. Say "A I panel" to read the feedback (Ctrl+3)
8. Say "Insert at cursor" (your Step-by-Step command) to insert
```

---

## 3. JSay 23 Setup

### 3.1 How JSay Works with VoiceBook

JSay 23 is a companion to JAWS that adds voice shortcuts. JSay listens for phrases you define and converts them to keystroke sequences that it sends to the active application. VoiceBook receives only the keystrokes — identical to pressing them manually.

JSay works alongside JAWS. JAWS reads the screen; JSay controls the app with your voice.

### 3.2 Open JSay Scripts Editor

1. Right-click the JSay icon in the system tray
2. Select **Settings**
3. Go to the **Scripts** tab (or **Commands** in some versions)
4. Click **Add** to create each new script entry

### 3.3 Set Target Application

In JSay settings, set the **Target Application** to **VoiceBook Studio**. This ensures your VoiceBook scripts only fire when VoiceBook is the active window.

Some JSay versions use a window title match. Use: **VoiceBook Studio**

### 3.4 Enable Continuous Recognition

In JSay Settings → Recognition:
- Set mode to **Continuous** (not Push-to-Talk)
- This lets JSay hear your commands without pressing a button

### 3.5 JSay Scripts — Panel Navigation

Enter each script exactly as shown. The left side is what you say; the right side is the keystroke JSay sends.

```
"chapter panel"   =  {ctrl+1}
"editor panel"    =  {ctrl+2}
"A I panel"       =  {ctrl+3}
```

### 3.6 JSay Scripts — File Operations

```
"save project"    =  {ctrl+s}
"new project"     =  {ctrl+n}
"open project"    =  {ctrl+o}
"save as"         =  {ctrl+shift+s}
"import document" =  {ctrl+i}
```

### 3.7 JSay Scripts — Chapter Management

```
"add chapter"         =  {ctrl+a}
"rename chapter"      =  {ctrl+d}
"delete chapter"      =  {ctrl+del}
"move chapter up"     =  {alt+up}
"move chapter down"   =  {alt+down}
```

### 3.8 JSay Scripts — Reading and Navigation

```
"read paragraph"      =  {f4}
"chapter title"       =  {f5}
"next chapter"        =  {f6}
"previous chapter"    =  {f7}
"read chapter"        =  {f8}
"app status"          =  {f9}
"stop reading"        =  {ctrl+f4}
```

### 3.9 JSay Scripts — AI Feedback

```
"run feedback"        =  {ctrl+f}
```

### 3.10 JSay Scripts — Complete Script File

Copy the entire block below into your JSay scripts configuration (exact format depends on JSay version — use the Add dialog if copy-paste is not supported):

```
; VoiceBook Studio — JSay Scripts
; Set Target Application = VoiceBook Studio

; Panel navigation
"chapter panel"           {ctrl+1}
"editor panel"            {ctrl+2}
"A I panel"               {ctrl+3}

; File
"save project"            {ctrl+s}
"save now"                {ctrl+s}
"new project"             {ctrl+n}
"open project"            {ctrl+o}
"save as"                 {ctrl+shift+s}
"import document"         {ctrl+i}

; Chapters
"add chapter"             {ctrl+a}
"new chapter"             {ctrl+a}
"rename chapter"          {ctrl+d}
"delete chapter"          {ctrl+del}
"move chapter up"         {alt+up}
"move chapter down"       {alt+down}

; Reading
"read paragraph"          {f4}
"chapter title"           {f5}
"next chapter"            {f6}
"previous chapter"        {f7}
"read chapter"            {f8}
"app status"              {f9}
"stop reading"            {ctrl+f4}
"stop"                    {ctrl+f4}

; AI
"run feedback"            {ctrl+f}
"comprehensive feedback"  {ctrl+f}
```

### 3.11 JSay Workflow

```
1. Open VoiceBook Studio (JAWS starts reading; JSay starts listening)
2. Say "chapter panel" → navigate chapter list with arrow keys
3. Say "editor panel" → JAWS moves to editor
4. Use standard JAWS dictation or type your content
5. Say "run feedback" → AI analysis runs
6. Say "A I panel" → JAWS reads the response
7. Say "chapter panel" → back to navigation
```

---

## 4. JAWS 25 Setup

### 4.1 How JAWS Works with VoiceBook

VoiceBook Studio is built for JAWS. All controls have accessible names, all status changes use live regions, and all dialogs are fully keyboard navigable.

JAWS reads VoiceBook by default without any configuration. The settings below improve the experience significantly.

### 4.2 Disable Virtual PC Cursor for VoiceBook

The Virtual PC Cursor makes JAWS navigate WPF apps as if they were web pages, which conflicts with VoiceBook's panel structure. Turning it off lets JAWS interact with the real WPF focus model.

1. Open VoiceBook Studio
2. Press **JAWS key + F2** to open Settings Center for VoiceBook Studio
3. Find **Virtual PC Cursor** (under Navigation)
4. Set it to **Off**
5. Click **OK**

This setting is saved per-application so it only affects VoiceBook.

### 4.3 Turn Off App Voice

VoiceBook has its own text-to-speech (SAPI) for spoken status feedback. When JAWS is running, VoiceBook detects this and silences its own voice automatically. If you hear both voices:

1. Go to VoiceBook's **Settings** menu
2. Set **App Voice** to **Off**

Or say (or type in chat input): **voice off**

### 4.4 JAWS Live Regions

VoiceBook uses JAWS live regions for all important updates:

- Status bar — announces every action (save, word count, AI results, errors)
- Word count — updated and announced as you type
- AI completion — announces "Analysis complete" and "Response ready"

If live regions are not being announced:

1. Open JAWS Settings Center (JAWS key + F2)
2. Search for **Announce live regions**
3. Set to **On**

### 4.5 Panel Navigation with JAWS

Use **Ctrl+1 / Ctrl+2 / Ctrl+3** to jump between panels. This is faster than Tab and avoids passing through toolbar and menu bar controls.

| Keys | Where JAWS focus lands |
|---|---|
| Ctrl+1 | Chapter list — arrow keys navigate chapters |
| Ctrl+2 | Writing editor — type, dictate, or navigate text |
| Ctrl+3 | Chat input box — type a question or command |

### 4.6 Chapter List Navigation

With focus in Panel 1 (Ctrl+1):

| Key | Action |
|---|---|
| Up arrow | Previous chapter in list |
| Down arrow | Next chapter in list |
| Enter | Open selected chapter in editor |
| F5 | Hear the name of the currently selected chapter |
| F6 | Move to next chapter |
| F7 | Move to previous chapter |

JAWS announces the chapter name and section group (Front Matter, Chapter, Back Matter) as you navigate.

### 4.7 AI Assistant with JAWS

1. Press **Ctrl+F** to run comprehensive feedback — JAWS announces "Running analysis. Please wait."
2. When done, JAWS announces "Analysis complete." via the live region.
3. Press **Ctrl+3** to focus the AI panel and read the response.
4. Navigate the response text with JAWS cursor keys.
5. Press **Tab** to reach the insert buttons (At Cursor, At Start, At End).

### 4.8 Reading Content with JAWS

Two methods are available:

**JAWS native reading** — With editor focus (Ctrl+2), use JAWS cursor commands to read text as normal (JAWS key + Down to read continuously, or navigate by sentence/paragraph).

**VoiceBook TTS reading** — Press F4 to hear the current paragraph, or F8 to hear the entire chapter, using VoiceBook's own voice engine. Press Ctrl+F4 to stop. This is useful when you want to hear text without JAWS cursor moving.

### 4.9 JAWS Key Map (Quick Reference)

| Keys | Context | Action |
|---|---|---|
| Ctrl+1 | Anywhere | Focus Chapter list |
| Ctrl+2 | Anywhere | Focus Writing editor |
| Ctrl+3 | Anywhere | Focus AI Assistant |
| F1 | Any control | JAWS contextual help |
| F4 | Any panel | Read paragraph at cursor |
| F5 | Any panel | Announce chapter title |
| F6 | Any panel | Next chapter |
| F7 | Any panel | Previous chapter |
| F8 | Any panel | Read entire chapter |
| F9 | Any panel | Application status |
| Ctrl+F | Any panel | Run comprehensive AI feedback |
| Ctrl+F4 | Any panel | Stop reading |
| Escape | Editor / Chapter list | Close dialog or return to chapter list |

### 4.10 JAWS Scripts (Optional Advanced Setup)

VoiceBook works fully without custom JAWS scripts. If you want additional JAWS key remapping, JAWS scripting can be used. The JAWS script files for an application are stored at:

```
%AppData%\Freedom Scientific\JAWS\[version]\Settings\enu\
```

The script files would be named:
- `VoiceBookStudio.jss` (JAWS Script Source)
- `VoiceBookStudio.jkm` (JAWS Key Map)

**Example VoiceBookStudio.jkm** (if you want to remap any keys):

```
[COMMON KEYS]
; Add key remappings here if needed
; Example: map NumPad5 to announce word count
; NumPad5=Script_AnnounceWordCount()
```

For most users, no custom JAWS scripts are needed. All functionality is accessible through the keyboard shortcuts in section 4.9.

### 4.11 Recommended JAWS Verbosity Settings

In JAWS Settings Center (JAWS key + F2) for VoiceBook Studio:

| Setting | Recommended value |
|---|---|
| Virtual PC Cursor | Off |
| Announce live regions | On |
| Say all (continuous reading) | On |
| Announce dynamic content changes | On |

---

## 5. Using All Three Together

Many users run JAWS + JSay (without Dragon). Some run JAWS + Dragon (without JSay). A few run all three.

### JAWS + JSay (Most Common)

- JAWS reads the screen
- JSay handles voice navigation commands
- VoiceBook's built-in mic is turned off (JSay is detected)
- Everything works through keyboard shortcuts via JSay scripts

Setup order: Configure JAWS settings first (section 4), then add JSay scripts (section 3).

### JAWS + Dragon

- JAWS reads the screen
- Dragon handles all speech input (dictation + custom commands)
- VoiceBook's built-in mic is turned off (Dragon is detected)
- Turn VoiceBook App Voice OFF (Dragon and JAWS handle all audio)

Setup order: Configure JAWS settings first (section 4), then Dragon MyCommands (section 2).

### JAWS + Dragon + JSay

All three can run simultaneously. Dragon handles dictation and complex commands; JSay handles quick navigation shortcuts; JAWS reads everything.

- Dragon for: dictating chapter content, asking Claude questions
- JSay for: quick shortcuts (chapter panel, read paragraph, next chapter)
- JAWS for: reading content, status announcements

### VoiceBook Standalone (No AT)

If no screen reader or voice input software is running, VoiceBook's built-in mic activates automatically. All commands in the Master Voice Commands list are available by speaking them aloud. App Voice (SAPI TTS) provides spoken feedback.

---

## Troubleshooting

### Dragon does not recognise custom commands

- Check that the Application is set to **VoiceBook Studio** in the command settings
- Make sure VoiceBook Studio is the active window when you say the command
- Try saying the command while VoiceBook has focus

### JSay scripts not firing

- Confirm the Target Application is set to **VoiceBook Studio**
- Check Continuous recognition mode is enabled
- Verify JSay is running (check system tray)

### JAWS reads panel headers repeatedly

- Ensure Virtual PC Cursor is OFF for VoiceBook (JAWS key + F2)
- With Virtual PC Cursor on, JAWS treats panels like web regions and re-reads headings

### Both JAWS and VoiceBook App Voice speak at once

- Set App Voice to **Off** in VoiceBook's Settings menu
- Or say (type in chat input): **voice off**

### Dragon dictates commands as text in the editor

This happens when Panel 2 (the editor) is focused and you say a navigation phrase. The fix:
- Set up MyCommands (section 2.5–2.8) so Dragon sends keystrokes instead of text
- Alternatively, press Ctrl+1 to leave the editor before saying navigation commands

### Built-in mic recognises commands while dictating

This does not apply when Dragon is running — VoiceBook's built-in mic is disabled automatically when Dragon is detected. If the built-in mic is picking up dictation as commands, Dragon may not have started before VoiceBook.

---

*VoiceBook Studio — AT Setup Guide v1.0*
