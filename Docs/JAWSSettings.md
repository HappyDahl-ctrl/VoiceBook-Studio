# JAWS 25 — Settings & Tips for VoiceBook Studio

VoiceBook Studio is built on WPF with full UI Automation (UIA) support.
JAWS 25 works with it out of the box. These settings optimise the experience.

---

## Recommended JAWS Settings

### Application-Specific Settings

JAWS lets you configure settings per application. When VoiceBook Studio is focused:

1. Press **JAWS key + F2** to open Settings Center for VoiceBook Studio
2. Set these options:

| Setting | Recommended value | Reason |
|---|---|---|
| Virtual PC Cursor | **Off** | VoiceBook is a native WPF app, not a web page |
| Application mode | **PC Cursor only** | Direct keyboard access works; virtual cursor is not needed |
| Read all focus changes | **On** | Announces every control as you Tab through |
| Announce live regions | **On** | Reads status bar updates (saves, exports, AI complete) |
| Verbosity | **Intermediate** | Reads name + type + state; verbose reads too much |
| Announce tooltips | **On** | Help Text (F1) descriptions appear as tooltips |

### Global Settings that Help

| Setting | Recommended value |
|---|---|
| Speak window title changes | On |
| Announce dialog boxes | On |
| Announce menu items | On |
| Speech rate | 350–400 wpm (adjust to comfort) |

---

## App Voice vs JAWS

VoiceBook Studio has its own built-in text-to-speech (SAPI-based).  
**When using JAWS, turn off the app voice:**

1. In VoiceBook Studio: **Settings menu → App voice: On** → click to toggle to **Off**
2. Or say **"Toggle voice"** / **"Voice off"**
3. JAWS will now handle all audio announcements through its own voice pipeline

The app's voice is designed as a backup for users without a screen reader.  
JAWS is always running in parallel regardless of this setting.

---

## Navigating VoiceBook Studio with JAWS

### Window structure
```
VoiceBook Studio
├── Menu bar          (Alt to open; arrow keys to navigate)
├── Toolbar           (Tab through buttons)
├── Three-panel layout
│   ├── Panel 1 — Chapter list    (Ctrl+1 to focus)
│   ├── Panel 2 — Writing editor  (Ctrl+2 to focus)
│   └── Panel 3 — AI assistant    (Ctrl+3 to focus)
│       ├── Chat tab
│       ├── Prompts tab
│       └── Cards tab
└── Status bar        (live region — JAWS announces automatically)
```

### Panel navigation
| Key | Action |
|---|---|
| Ctrl+1 | Focus Chapter List |
| Ctrl+2 | Focus Writing Editor |
| Ctrl+3 | Focus AI Assistant Chat input |
| Tab / Shift+Tab | Move between controls within a panel |
| Arrow keys | Navigate within ListBox (chapter list, prompts list, cards list) |
| Enter | Activate selected item or default button |
| Escape | Cancel / close dialog |
| Alt | Open menu bar |

### Chapter list (Panel 1)
- JAWS reads: "[Chapter title] — [Section group]" for each item
  - Example: "Chapter One — Body"
  - Example: "[Dedication] My Dedication — Front Matter"
- Use **Up/Down arrows** to navigate; JAWS announces each chapter
- Press **Enter** or **Space** to select and load into the editor

### Writing editor (Panel 2)
- Standard text field — JAWS reads as you type and navigate
- JAWS reads each character, word, or line depending on cursor movement
- JAWS reads the word count display above the editor on each update
- Dragon NaturallySpeaking can dictate here simultaneously

### AI Assistant (Panel 3)
- Ctrl+3 focuses the Chat input field
- After AI responds, JAWS announces "AI response text" (live region: Polite)
- Use Tab to reach Insert buttons; JAWS reads "Insert at cursor position", etc.
- After inserting, JAWS announces "Inserted at cursor position" and focus returns to editor

### Dialogs
All dialogs are screen-reader accessible:
- **Input dialog**: JAWS reads the prompt text then focuses the input field
- **Section type dialog**: JAWS reads grouped list (Front Matter / Body / Back Matter)
- **Save As dialog**: Standard Windows file dialog — JAWS works natively
- **API key dialog**: JAWS reads field labels and instructions

---

## JAWS Keystrokes (reference)

| JAWS keystroke | Action |
|---|---|
| Insert+F1 | Read JAWS Help for current control |
| Insert+T | Read window title |
| Insert+B | Read all text in the current window |
| Insert+Tab | Read name and type of focused control |
| Insert+F7 | List all buttons / links in the window |
| Insert+F6 | List all headings (not applicable in WPF apps) |
| F1 | Read HelpText (AutomationProperties.HelpText) for the focused control |
| Ctrl+Home / End | Jump to beginning / end of text in editor |
| Insert+Down | Start "Say All" — reads entire editor content |
| Ctrl+Insert+Down | JAWS reads entire editor content (alternate) |
| Insert+F5 | List all form fields in the window |

---

## Live Regions

VoiceBook Studio uses UIA live regions so JAWS reads updates automatically:

| Element | Live setting | What it announces |
|---|---|---|
| Status bar | Polite | Save confirmations, word count, AI complete, chapter loaded |
| Editor word count | Polite | Word count as you type |
| Chapter list label | Polite | Which chapter is active |
| JAWS status (toolbar) | Polite | "JAWS: Running" or "JAWS: Not detected" |

Polite regions are read after the current speech finishes.  
No assertive interruptions — you will not be cut off while reading.

---

## JAWS Scripting (advanced)

JAWS 25 supports custom scripts (.jss files) for application-specific behaviour.  
For most users, no scripting is needed — the built-in UIA support covers everything.

If you want to create custom JAWS scripts for VoiceBook Studio:
1. Open JAWS Script Manager: **JAWS → Tools → Script Manager**
2. Create a new script file for VoiceBookStudio.exe
3. Useful script hooks:
   - `FocusChangedEvent()` — fires when focus moves between panels
   - `KeyPressedEvent()` — override specific keys
   - `WindowActivatedEvent()` — fires when VoiceBook window is focused

**Example script: announce panel name when Ctrl+1/2/3 pressed**
```jaws
Script PanelFocus1()
  SayString("Chapters panel")
  TypeKey("ctrl+1")
EndScript

Script PanelFocus2()
  SayString("Editor panel")
  TypeKey("ctrl+2")
EndScript

Script PanelFocus3()
  SayString("AI panel")
  TypeKey("ctrl+3")
EndScript
```
Assign these scripts to Ctrl+1, Ctrl+2, Ctrl+3 in the JAWS keyboard manager.

---

## Known Compatibility Notes

| Situation | Notes |
|---|---|
| Virtual PC Cursor auto-activates | Turn it off in JAWS app settings for VoiceBook Studio |
| JAWS reads too much in editor | Set verbosity to Intermediate; turn off "Read all" in WPF app settings |
| SAPI voice and JAWS overlap | Set App Voice Off in VoiceBook Settings menu |
| Dialog does not get focus | Press Alt+Tab to bring focus back; all dialogs are modal |
| Tab order confusing | Use Ctrl+1/2/3 shortcuts rather than tabbing between panels |
