# VoiceBook Studio — JAWS Settings & Keystroke Guide

VoiceBook Studio is built on WPF with full UI Automation (UIA) support, so JAWS works with it out of the box — nothing in this guide is required. It exists for readers who want to fine-tune JAWS behavior or understand exactly what's happening under the hood. For everyday use, `Docs/User-Manual.md` Section 14 ("How JAWS Works with This App") is all you need.

---

## Recommended JAWS Settings

### Application-Specific Settings

JAWS lets you configure settings per application. With VoiceBook Studio focused:

1. Press **JAWS key + F2** to open Settings Center for VoiceBook Studio.
2. Set these options:

| Setting | Recommended value | Reason |
|---|---|---|
| Virtual PC Cursor | Off | VoiceBook is a native WPF app, not a web page |
| Application mode | PC Cursor only | Direct keyboard access works; the virtual cursor is not needed |
| Read all focus changes | On | Announces every control as you Tab through |
| Announce live regions | On | Reads status bar and other live-region updates automatically |
| Verbosity | Intermediate | Reads name + type + state; Verbose reads more than most people want |
| Announce tooltips | On | Help Text (F1) descriptions surface as tooltips |

### Global Settings That Help

| Setting | Recommended value |
|---|---|
| Speak window title changes | On |
| Announce dialog boxes | On |
| Announce menu items | On |
| Speech rate | 350–400 wpm (adjust to comfort) |

---

## App Voice vs. JAWS

VoiceBook Studio has its own built-in text-to-speech (SAPI-based), used when no screen reader is present. **You do not need to configure anything for this** — the app detects JAWS at startup and silences its own voice automatically the moment JAWS is running, so there is never any manual toggle needed to prevent overlap. The "Toggle voice" / "Voice off" command still exists, but it only affects the app's voice for the case where JAWS is *not* running (e.g. no screen reader at all) — it plays no role in JAWS coexistence.

---

## Navigating VoiceBook Studio with JAWS

### Window structure

```
VoiceBook Studio
├── Menu bar (Alt to open; arrow keys to navigate)
├── Toolbar (Tab through buttons)
├── Panel 1 — Chapter Manager (Ctrl+1 to focus)
├── Panel 2 — Writing Editor (F2 or Ctrl+2 to focus)
├── Panel 4 — Library (F11 or Ctrl+4 to focus)
│     ├── Prompts tab
│     ├── Cards tab
│     └── Feedback tab
├── Panel 3 — AI Assistant (F3 or Ctrl+3 to focus)
│     Always visible along the bottom — response text, Insert/Replace
│     buttons, and the chat input — regardless of which panel above has focus
└── Status bar (live region — JAWS announces automatically)
```

### Panel navigation

| Key | Action |
|---|---|
| Ctrl+1 | Focus Chapter Manager (only works outside the editor — see below) |
| F2 or Ctrl+2 | Focus Writing Editor |
| F3 or Ctrl+3 | Focus AI Assistant chat input |
| F11 or Ctrl+4 | Focus Library |
| Escape | Return to Chapter Manager from anywhere, including from inside the editor |
| Tab / Shift+Tab | Move between controls within a panel |
| Arrow keys | Navigate within a list (chapter list, prompts list, cards list) |
| Enter | Activate selected item or default button |
| Alt | Open the menu bar |

Note: F2, F3, F11, and Escape all work even while your cursor is inside the Writing Editor mid-dictation. Ctrl+1/2/3/4 only work when focus is outside the editor.

### Chapter list (Panel 1)

JAWS reads each item as "**[Chapter title] — [Section group]**", for example "Chapter One — Body" or "Dedication — Front Matter". Use Up/Down arrows to navigate; JAWS announces each chapter as you move. Press Enter or Space to select and load it into the editor.

### Writing Editor (Panel 2)

Standard text field behavior — JAWS reads as you type and navigate. Dragon NaturallySpeaking can dictate here simultaneously. The word count above the editor deliberately does **not** announce on every keystroke (it used to, and that was a real usability problem) — it's announced automatically right after you select a chapter or Whole Book, and on request via the "word count" family of voice commands. See `Docs/User-Manual.md` Section 11.

### AI Assistant (Panel 3)

F3 or Ctrl+3 focuses the chat input field. After Claude responds, JAWS announces the response text via a Polite live region. Tab to reach the Insert/Replace buttons — JAWS reads "Insert at cursor position," "Replace…," etc. After inserting or replacing, JAWS announces the result and focus returns to the editor.

### Dialogs

All dialogs are screen-reader accessible:
- **Input dialogs** — JAWS reads the prompt text then focuses the input field
- **Section type dialog** — JAWS reads the grouped list (Front Matter / Body / Back Matter)
- **Save As dialog** — standard Windows file dialog, works natively with JAWS
- **API key dialog** — JAWS reads field labels and instructions
- **Save Card dialog** — JAWS reads the title/category fields

---

## JAWS Keystrokes (Reference)

These are standard JAWS keystrokes, not VoiceBook-specific, but especially useful in this app:

| JAWS keystroke | Action |
|---|---|
| Insert+F1 | Read JAWS Help for the current control |
| Insert+T | Read window title |
| Insert+B | Read all text in the current window |
| Insert+Tab | Read name and type of the focused control |
| Insert+F7 | List all buttons/links in the window |
| F1 | Read HelpText (`AutomationProperties.HelpText`) for the focused control |
| Ctrl+Home / Ctrl+End | Jump to beginning / end of text in the editor |
| Insert+Down | Start "Say All" — reads the entire editor content |
| Insert+F5 | List all form fields in the window |

---

## Live Regions

VoiceBook Studio uses UIA live regions so JAWS reads updates automatically without you navigating to them:

| Element | Live setting | What it announces |
|---|---|---|
| Status bar | Polite | Save confirmations, AI complete, chapter loaded, errors |
| Chapter list label | Polite | Which chapter is active |
| AI response text | Polite | Claude's reply, as it arrives |
| JAWS status (toolbar) | Polite | "JAWS: Running" or "JAWS: Not detected" |
| Word count | *(intentionally not live)* | See above — spoken deliberately, not on every keystroke |

Polite regions are read after the current speech finishes — you will not be cut off mid-sentence while JAWS is reading something else.

---

## JAWS Scripting (Advanced, Optional)

JAWS supports custom scripts (`.jss` files) for application-specific behavior. For most users, no scripting is needed — the built-in UIA support covers everything above. If you want to go further:

1. Open **JAWS → Tools → Script Manager**.
2. Create a new script file for `VoiceBookStudio.exe`.
3. Useful script hooks:
   - `FocusChangedEvent()` — fires when focus moves between panels
   - `KeyPressedEvent()` — override specific keys
   - `WindowActivatedEvent()` — fires when the VoiceBook window is focused

Example — announce a custom panel name when a shortcut is pressed:

```
Script PanelFocus1()
   SayString("Chapters panel")
   TypeKey("ctrl+1")
EndScript

Script PanelFocus2()
   SayString("Editor panel")
   TypeKey("f2")
EndScript
```

Assign scripts like these to your preferred keys in the JAWS keyboard manager if you want spoken feedback beyond what the app already provides.

---

## Known Compatibility Notes

| Situation | Notes |
|---|---|
| Virtual PC Cursor auto-activates | Turn it off in JAWS's application-specific settings for VoiceBook Studio |
| JAWS reads too much in the editor | Set verbosity to Intermediate; turn off "Read all" if it feels excessive |
| Dialog does not get focus | Press Alt+Tab to bring focus back; all dialogs are modal |
| Tab order feels slow | Use F2/F3/F11/Ctrl+1 shortcuts rather than tabbing between panels |
