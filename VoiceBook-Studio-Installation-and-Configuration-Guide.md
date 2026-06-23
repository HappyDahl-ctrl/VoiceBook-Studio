# VoiceBook Studio — Installation and Configuration Guide

For a PC that already has Dragon NaturallySpeaking Professional, JAWS, and JSay installed.

---

## Contents

1. System Requirements
2. Installing VoiceBook Studio
3. First Launch
4. Getting Your Anthropic API Key
5. Configuring JAWS
6. Configuring Dragon — App Settings
7. Configuring Dragon — MyCommands Setup (201 commands)
8. Configuring JSay
9. Windows System Settings
10. Verifying Everything Works
11. Troubleshooting

---

## 1. System Requirements

- Windows 10 or Windows 11 (64-bit)
- .NET 8 Desktop Runtime (installed automatically if missing)
- Dragon NaturallySpeaking Professional (any recent version)
- JAWS (any version that supports UIA)
- JSay (optional — detected automatically)
- Internet connection (for AI features only — the app itself works offline)
- A microphone (Dragon uses it; the app's built-in recogniser is disabled when Dragon is running)

---

## 2. Installing VoiceBook Studio

1. Download the VoiceBook Studio installer (`VoiceBookStudio-Setup.exe`)
2. Right-click the installer and choose **Run as administrator**
3. Follow the installer steps — accept the defaults unless you have a reason to change the install folder
4. The installer creates a shortcut on the Desktop and in the Start Menu
5. If Windows asks about .NET 8, click Yes to install it — it is free and required

**Default install location:**
```
C:\Program Files\VoiceBook Studio\
```

**Data location (projects, settings, prompts, cards, feedback):**
```
C:\Users\[username]\AppData\Roaming\VoiceBookStudio\
```

Do not delete or move the AppData folder — it contains all your writing projects and library data.

---

## 3. First Launch

Launch VoiceBook Studio from the Desktop shortcut.

The app will:
1. Speak what accessibility tools it detected (JAWS, Dragon, JSay)
2. Announce microphone status
3. Open the Welcome dialog
4. Say "VoiceBook Studio ready"

On first launch, choose **Start Guided Tour** to walk through the 17-step interactive tutorial. It covers every feature with voice guidance. Takes about 10 minutes and is strongly recommended.

---

## 4. Getting Your Anthropic API Key

The AI features (feedback, chat, book analysis) require a free Anthropic account and API key.

1. Go to `console.anthropic.com` in a web browser
2. Sign up for an account or sign in
3. Click **API Keys** in the left menu
4. Click **Create Key** — give it a name like "VoiceBook"
5. Copy the key (it starts with `sk-ant-`)

**Enter the key in VoiceBook Studio:**

1. Launch VoiceBook Studio
2. Say "set API key" or click the key icon in the toolbar
3. Paste the API key into the field and click Save
4. You will hear "API key saved"

The key is stored locally in:
```
C:\Users\[username]\AppData\Roaming\VoiceBookStudio\settings.json
```

It is never shared with anyone other than Anthropic when you use the AI features.

**Billing:** Anthropic charges per use. For typical book writing and feedback sessions, cost is very low — under a few dollars per month for heavy use. Set a spending limit at `console.anthropic.com` under **Billing** if you want a cap.

---

## 5. Configuring JAWS

**No JAWS configuration is required.** VoiceBook Studio detects JAWS at launch and adjusts automatically:

- The app's general TTS is silenced — JAWS handles interface readback
- Critical announcements (startup, project events, errors, goodbye) still speak through a separate SAPI channel with a 500 ms delay so they do not clash with JAWS
- All buttons, lists, input fields, menus, and tabs have UIA accessibility labels that JAWS reads correctly
- Live regions on the status bar, editor title, and AI response box cause JAWS to announce changes automatically

### Recommended JAWS Settings

**Audio output:** Make sure JAWS and Windows use the same audio output device. Go to:

```
JAWS → Options → Sound → Audio Scheme
```

Set the output device to match the Windows default playback device (check in Windows: Settings → System → Sound → Output).

**Verbosity:** Default verbosity works well. If JAWS reads too much when navigating the chapter list, reduce list item verbosity:

```
JAWS → Settings Center → Virtual Cursor → List Items → Verbosity → Low
```

**Speech history:** JAWS Speech History (Insert+Space, then H) is useful for reviewing what the app announced if you missed it.

### Note on Dual Announcements

Some events are announced both by JAWS (reading the control) and by the app's system announcer. This is intentional — it ensures critical events (project saved, import complete, AI responded) are never missed. If this becomes repetitive, say "toggle voice" in VoiceBook to silence the app's secondary voice and rely on JAWS alone.

---

## 6. Configuring Dragon — App Settings

### Check Dragon Version

VoiceBook Studio works with Dragon Professional Individual and Dragon Professional (any recent version). The XML MyCommands import format was removed in newer Dragon versions — this guide uses manual entry in the Command Browser, which works in all versions.

### Train "VoiceBook"

Dragon may mishear "VoiceBook" initially. Add it to Dragon's vocabulary:

1. Open Dragon
2. Say "Add new word" or go to **Vocabulary → Add new word or phrase**
3. Type: `VoiceBook`
4. Click Add
5. Optionally record a spoken sample of "VoiceBook"

### Verify the Writing Editor Works

1. Open VoiceBook Studio
2. Create a new project (say "new project")
3. Add a chapter (say "add chapter")
4. Move to the editor (press Ctrl+2)
5. Dictate a sentence

Dragon should accept dictation in the editor exactly as in Word. If dictation does not work in the editor, check that Dragon is in Normal mode (not Command mode) — the Dragon bar should show a green microphone icon.

### Microphone and the Built-in Recogniser

When Dragon is running, VoiceBook's built-in voice recogniser is automatically disabled. Dragon owns the microphone. The startup announcement will say "Dragon detected — using Dragon for voice input."

App voice commands are issued by:
- Using Dragon MyCommands (the 201 commands you set up in Section 7)
- Pressing Ctrl+Shift+Space to open the command bar, then dictating a command

---

## 7. Configuring Dragon — MyCommands Setup

This is the main setup task. You will create 201 Dragon MyCommands that cover every VoiceBook Studio feature.

### Before You Start

1. Launch VoiceBook Studio so it appears in Dragon's application list
2. Open Dragon
3. Say "Open Command Browser" or click **Tools → Command Browser**
4. Click **New** to open the MyCommand editor

### How to Create Each Command

**For Type A — Keystroke commands:**

1. Click New in Command Browser
2. Name field: type the command phrase exactly as shown in the tables below
3. Application: click the dropdown and select **VoiceBook Studio**
4. Command Type: select **Keystroke**
5. In the keystroke field: click in it, then press the keyboard shortcut on your keyboard
6. Click Save
7. Repeat

**For Type B — Step-by-Step commands:**

1. Click New in Command Browser
2. Name field: type the command phrase
3. Application: select **VoiceBook Studio**
4. Command Type: select **Step-by-Step**
5. Add 4 steps in this exact order:
   - Step 1: **Press Key** → press Ctrl+Shift+Space on your keyboard
   - Step 2: **Wait** → 300 milliseconds
   - Step 3: **Type Text** → type the command text shown in the table
   - Step 4: **Press Key** → press Enter on your keyboard
6. Click Save

---

### Section A — Panel Navigation (Type A — Keystroke)

| Command Name | Keystroke |
|---|---|
| Go to chapters | Ctrl+1 |
| Go to chapter list | Ctrl+1 |
| Panel one | Ctrl+1 |
| Go to editor | Ctrl+2 |
| Open writing editor | Ctrl+2 |
| Panel two | Ctrl+2 |
| Go to assistant | Ctrl+3 |
| Open AI assistant | Ctrl+3 |
| Panel three | Ctrl+3 |

---

### Section B — Project Commands (Type A — Keystroke)

| Command Name | Keystroke |
|---|---|
| New VoiceBook project | Ctrl+N |
| Create new project | Ctrl+N |
| Open VoiceBook project | Ctrl+O |
| Open project | Ctrl+O |
| Save project | Ctrl+S |
| Save now | Ctrl+S |
| Save VoiceBook | Ctrl+S |
| Save project as | Ctrl+Shift+S |
| Import Word document | Ctrl+I |
| Import document | Ctrl+I |

---

### Section C — Chapter Management (Type A — Keystroke)

| Command Name | Keystroke |
|---|---|
| Add chapter | Ctrl+A |
| Add new chapter | Ctrl+A |
| New chapter | Ctrl+A |
| Rename chapter | Ctrl+D |
| Rename this chapter | Ctrl+D |
| Delete chapter | Ctrl+Delete |
| Delete this chapter | Ctrl+Delete |
| Move chapter up | Alt+Up |
| Move up | Alt+Up |
| Move chapter down | Alt+Down |
| Move down | Alt+Down |

Note for Delete chapter: in the Keystroke field, press the Ctrl key and then the Delete key (not Backspace). The field should show "Ctrl+Delete."

---

### Section D — AI Feedback — Direct Shortcut (Type A — Keystroke)

| Command Name | Keystroke |
|---|---|
| Run feedback | Ctrl+F |
| Comprehensive feedback | Ctrl+F |
| Get feedback | Ctrl+F |

---

### Section E — Command Bar Shortcut (Type A — Keystroke)

Create this first — it is the foundation for all Type B commands.

| Command Name | Keystroke |
|---|---|
| VoiceBook command | Ctrl+Shift+Space |
| Open command bar | Ctrl+Shift+Space |

---

### Section F — AI Feedback — Additional Types (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Pacing feedback | pacing |
| Check pacing | pacing |
| Dialogue feedback | dialogue |
| Check dialogue | dialogue |
| Style feedback | style |
| Check style | style |
| Structure feedback | structure |
| Check structure | structure |
| Book analysis | book analysis |
| Analyse full book | book analysis |
| Full book feedback | book analysis |
| Whole book feedback | book analysis |

---

### Section G — Chapter Navigation (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Next chapter | next chapter |
| Go to next chapter | next chapter |
| Previous chapter | previous chapter |
| Go to previous chapter | previous chapter |
| Prior chapter | previous chapter |

---

### Section H — Reading Aloud (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Read chapter | read chapter |
| Read this chapter | read chapter |
| Read paragraph | read paragraph |
| Read this paragraph | read paragraph |
| Read chapter title | read chapter title |
| What chapter am I in | read chapter title |
| Stop reading | stop |
| Stop speech | stop |
| Silence | stop |

---

### Section I — Inserting AI Responses (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Insert at cursor | insert at cursor |
| Insert here | insert at cursor |
| Insert at start | insert at start |
| Insert at beginning | insert at start |
| Insert at end | insert at end |
| Append to chapter | insert at end |
| Read AI response | read response |
| Read the response | read response |
| Save response card | save card |
| Keep this response | save card |
| Discard response | discard response |
| Clear response | discard response |

---

### Section J — Export (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Export Word document | export word |
| Export manuscript | export word |
| Save as Word | export word |
| Export PDF | export pdf |
| Save as PDF | export pdf |
| Create PDF | export pdf |

---

### Section K — Prompt Library — Browse (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Open prompt library | open prompt library |
| Show prompts | open prompt library |
| What prompts do I have | what prompts do i have |
| Prompt categories | prompt categories |
| Read prompt categories | read prompt categories |
| Add new prompt | add new prompt |
| Create a prompt | add new prompt |

---

### Section L — Prompt Library — Read Category (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Read prompt A | read prompt a |
| Read prompt B | read prompt b |
| Read prompt C | read prompt c |
| Read prompt D | read prompt d |
| Read prompt E | read prompt e |
| Read prompt F | read prompt f |
| Read prompt G | read prompt g |
| Read prompt H | read prompt h |
| Read prompt I | read prompt i |
| Read prompt J | read prompt j |

---

### Section M — Prompt Library — Use Prompt (Type B — Step-by-Step)

Format: say "use prompt [letter] [number]" — for example "use prompt A one."

| Command Name | Text in Step 3 |
|---|---|
| Use prompt A one | use prompt a one |
| Use prompt A two | use prompt a two |
| Use prompt A three | use prompt a three |
| Use prompt A four | use prompt a four |
| Use prompt A five | use prompt a five |
| Use prompt B one | use prompt b one |
| Use prompt B two | use prompt b two |
| Use prompt B three | use prompt b three |
| Use prompt B four | use prompt b four |
| Use prompt B five | use prompt b five |
| Use prompt C one | use prompt c one |
| Use prompt C two | use prompt c two |
| Use prompt C three | use prompt c three |
| Use prompt C four | use prompt c four |
| Use prompt C five | use prompt c five |
| Use prompt D one | use prompt d one |
| Use prompt D two | use prompt d two |
| Use prompt D three | use prompt d three |
| Use prompt D four | use prompt d four |
| Use prompt D five | use prompt d five |
| Use prompt E one | use prompt e one |
| Use prompt E two | use prompt e two |
| Use prompt E three | use prompt e three |
| Use prompt E four | use prompt e four |
| Use prompt E five | use prompt e five |
| Use prompt F one | use prompt f one |
| Use prompt F two | use prompt f two |
| Use prompt F three | use prompt f three |
| Use prompt F four | use prompt f four |
| Use prompt F five | use prompt f five |
| Use prompt F six | use prompt f six |
| Use prompt F seven | use prompt f seven |
| Use prompt F eight | use prompt f eight |
| Use prompt F nine | use prompt f nine |
| Use prompt F ten | use prompt f ten |
| Use prompt G one | use prompt g one |
| Use prompt G two | use prompt g two |
| Use prompt G three | use prompt g three |
| Use prompt G four | use prompt g four |
| Use prompt G five | use prompt g five |
| Use prompt G six | use prompt g six |
| Use prompt G seven | use prompt g seven |
| Use prompt G eight | use prompt g eight |
| Use prompt G nine | use prompt g nine |
| Use prompt G ten | use prompt g ten |
| Use prompt H one | use prompt h one |
| Use prompt H two | use prompt h two |
| Use prompt H three | use prompt h three |
| Use prompt H four | use prompt h four |
| Use prompt H five | use prompt h five |
| Use prompt H six | use prompt h six |
| Use prompt H seven | use prompt h seven |
| Use prompt H eight | use prompt h eight |
| Use prompt H nine | use prompt h nine |
| Use prompt H ten | use prompt h ten |
| Use prompt I one | use prompt i one |
| Use prompt I two | use prompt i two |
| Use prompt I three | use prompt i three |
| Use prompt I four | use prompt i four |
| Use prompt I five | use prompt i five |
| Use prompt I six | use prompt i six |
| Use prompt I seven | use prompt i seven |
| Use prompt I eight | use prompt i eight |
| Use prompt I nine | use prompt i nine |
| Use prompt I ten | use prompt i ten |
| Use prompt J one | use prompt j one |
| Use prompt J two | use prompt j two |
| Use prompt J three | use prompt j three |
| Use prompt J four | use prompt j four |
| Use prompt J five | use prompt j five |
| Use prompt J six | use prompt j six |
| Use prompt J seven | use prompt j seven |
| Use prompt J eight | use prompt j eight |
| Use prompt J nine | use prompt j nine |
| Use prompt J ten | use prompt j ten |
| Use prompt K one | use prompt k one |

---

### Section N — Response Cards (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Open response cards | open response cards |
| Show my cards | open response cards |
| What cards do I have | what cards do i have |
| Card categories | card categories |
| Insert card one | insert card one |
| Insert card two | insert card two |
| Insert card three | insert card three |
| Insert card four | insert card four |
| Insert card five | insert card five |
| Insert card six | insert card six |
| Insert card seven | insert card seven |
| Insert card eight | insert card eight |
| Insert card nine | insert card nine |
| Insert card ten | insert card ten |
| Delete card one | delete card one |
| Delete card two | delete card two |
| Delete card three | delete card three |
| Delete card four | delete card four |
| Delete card five | delete card five |

---

### Section O — Feedback Library (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Open feedback library | open feedback library |
| Show my feedback | open feedback library |
| Feedback categories | feedback categories |
| Read my comprehensive feedback | read my comprehensive feedback |
| Read my pacing feedback | read my pacing feedback |
| Read my dialogue feedback | read my dialogue feedback |
| Read my style feedback | read my style feedback |
| Read my structure feedback | read my structure feedback |
| Resume reading | resume reading |
| Continue reading | resume reading |

---

### Section P — Settings and App Management (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Set API key | set api key |
| Configure API | set api key |
| Open settings | open settings |
| Set project folder | set project folder |
| Toggle app voice | toggle voice |
| Mute VoiceBook | toggle voice |
| Application status | status |
| What is the status | status |
| Start tutorial | start tutorial |
| What can I say | what can i say here |

---

### Section Q — Close the App (Type B — Step-by-Step)

| Command Name | Text in Step 3 |
|---|---|
| Close VoiceBook | close voicebook |
| Exit VoiceBook | close voicebook |
| Quit VoiceBook | close voicebook |

---

### Section R — Tutorial Navigation (Type B — Step-by-Step)

Only needed during the guided tutorial.

| Command Name | Text in Step 3 |
|---|---|
| Tutorial next | next |
| Tutorial previous | previous |
| Tutorial repeat | repeat |
| Tutorial skip | skip step |
| Tutorial exit | exit tutorial |
| Tutorial continue | continue |

---

### Total Command Count

| Section | Commands |
|---|---|
| A — Panel navigation | 9 |
| B — Project | 10 |
| C — Chapter management | 11 |
| D — AI feedback keystroke | 3 |
| E — Command bar shortcut | 2 |
| F — AI feedback step-by-step | 12 |
| G — Chapter navigation | 5 |
| H — Reading aloud | 9 |
| I — Insert response | 12 |
| J — Export | 6 |
| K — Prompt browse | 7 |
| L — Prompt read category | 10 |
| M — Use prompt | 61 |
| N — Response cards | 19 |
| O — Feedback library | 10 |
| P — Settings | 10 |
| Q — Close app | 3 |
| R — Tutorial | 6 |
| **Total** | **205** |

---

## 8. Configuring JSay

No JSay configuration is required. VoiceBook Studio detects JSay at startup and reports it in the startup announcement. JSay reads all controls, labels, and live regions through the Windows UIA accessibility layer automatically.

If JSay is not reading the app correctly, check:
1. JSay is set to use UIA (User Interface Automation) mode — this is the default in current versions
2. The Windows accessibility settings have UIA enabled (it is on by default)

---

## 9. Windows System Settings

### Audio

VoiceBook Studio and JAWS must both output to the same Windows audio device.

Check in: **Settings → System → Sound → Output**

Set the default output device to whichever device the headset or speakers use — whether that is the built-in audio, a USB headset, or an external audio interface. Then confirm JAWS is also set to that device (JAWS Options → Sound).

### Display Scaling

VoiceBook Studio is designed for any DPI setting. If text appears cut off in dialogs at high DPI:

1. Right-click `VoiceBookStudio.exe` → Properties
2. Compatibility tab → Change high DPI settings
3. Set Override high DPI scaling behavior to: **Application**

### Windows Speech Recognition

VoiceBook Studio's built-in voice recogniser uses Windows Speech Recognition. If Dragon is running, this is automatically disabled and you do not need to configure it. If you want to use the built-in recogniser without Dragon:

1. Go to **Settings → Time & Language → Speech**
2. Ensure a microphone is set up and tested
3. The built-in recogniser starts automatically when VoiceBook launches without Dragon running

### Firewall

The AI features connect to Anthropic's servers over HTTPS on port 443. This is standard web traffic and should not require any firewall exceptions. If AI requests time out, verify that outbound HTTPS is not blocked by antivirus or enterprise firewall software.

### Startup

To launch VoiceBook Studio at Windows startup (optional):

1. Press Win+R, type `shell:startup`, press Enter
2. Copy the VoiceBook Studio shortcut from the Desktop into this folder

The app will launch automatically when Windows starts.

---

## 10. Verifying Everything Works

Run through this checklist after setup:

### JAWS Check

- [ ] Launch VoiceBook Studio with JAWS running
- [ ] JAWS reads the startup announcements
- [ ] JAWS reads the Welcome dialog controls
- [ ] JAWS reads the chapter list items (navigate with arrow keys)
- [ ] JAWS reads the AI response box when Claude responds
- [ ] App voice is silenced (JAWS handles readback, no double-speech except critical events)

### Dragon Check

- [ ] Launch VoiceBook Studio with Dragon running
- [ ] Startup says "Dragon detected"
- [ ] Dictation works in the writing editor (create a chapter, move to editor, dictate a sentence)
- [ ] "Correct that" and "Scratch that" work in the editor
- [ ] Say "Save VoiceBook" — project saves, you hear "Project saved"
- [ ] Say "Panel two" — focus moves to the editor
- [ ] Say "VoiceBook command", then dictate "read chapter", then say "Press Enter" — the chapter is read aloud

### Dragon MyCommands Check (after setup in Section 7)

- [ ] Say "Go to chapters" — Panel 1 gets focus
- [ ] Say "Add chapter" — a new chapter is created
- [ ] Say "Save VoiceBook" — project saves
- [ ] Say "Run feedback" — comprehensive feedback runs on the current chapter
- [ ] Say "Pacing feedback" — pacing analysis runs via the command bar
- [ ] Say "Close VoiceBook" — app closes with goodbye announcement

### AI Check

- [ ] Open settings, enter API key, hear "API key saved"
- [ ] Select a chapter with some content
- [ ] Say "run feedback"
- [ ] Wait for Claude to respond (typically 5–20 seconds)
- [ ] Hear the warm tone and "Claude responded"
- [ ] Say "read response" to hear the feedback

### JSay Check

- [ ] Launch VoiceBook Studio with JSay running
- [ ] JSay reads the startup dialog
- [ ] JSay reads chapter list items and buttons

---

## 11. Troubleshooting

### App does not speak on startup

Check that the Windows default audio output device has working speakers or headphones. If JAWS is running, JAWS may be suppressing the startup speech — this is expected for some events but the first announcement "JAWS detected" should still play through SAPI.

### Dragon does not dictate in the editor

Make sure Dragon is in Normal mode (green mic icon, not Command mode). If dictation still does not work, try: Dragon menu → Tools → Options → Miscellaneous → Use the Full Text Control — enable this option.

### Dragon MyCommands fire in other apps

All commands must have the Application field set to VoiceBook Studio. Open Command Browser, find each affected command, and verify the application scope.

### Step-by-Step command types the text instead of running it

The command bar did not open in time. In the Step-by-Step editor, increase the Wait step from 300ms to 500ms or 700ms.

### VoiceBook Studio not in Dragon's Application list

VoiceBook Studio must be running when you open the Command Browser for it to appear in the dropdown. Launch VoiceBook first, then open Command Browser.

### AI requests time out or fail

1. Check your API key is entered correctly in Settings
2. Check your Anthropic account has billing set up at console.anthropic.com
3. Check your internet connection
4. Check Windows Firewall is not blocking outbound HTTPS

### JAWS reads too much or too little

Adjust JAWS verbosity settings. VoiceBook uses standard UIA patterns so standard JAWS verbosity controls apply. Reducing "list item" verbosity in Settings Center will quiet down the chapter list.

### Command conflicts with Dragon dictation

If Dragon treats a command name as dictation rather than a command, rename the command to include "VoiceBook" as a prefix (for example, change "Save" to "Save VoiceBook") to make it more distinctive.

### App does not say goodbye on close

The goodbye announcement uses synchronous SAPI speech. If another application is blocking the audio device at the moment of close (such as JAWS speaking at that instant), it may be cut off. This is rare and not harmful.

---

*For day-to-day use instructions, see the VoiceBook Studio User Manual. To re-run the guided tutorial at any time, say "start tutorial" or open Help → Welcome / Tutorial.*
