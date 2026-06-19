# Dragon Professional — Custom Commands for VoiceBook Studio

These commands supplement Dragon's native interaction with VoiceBook Studio.
Add them in Dragon's **MyCommands Editor** (Tools → Add New Command → MyCommand).

---

## Setup: Opening VoiceBook Studio

| Voice phrase | Dragon command type | Action |
|---|---|---|
| "Open VoiceBook Studio" | Shell open | `C:\Program Files\VoiceBook Studio\VoiceBookStudio.exe` |

---

## Panel Navigation (map to keyboard shortcuts)

Dragon cannot run VoiceCommandRouter directly. Map these phrases to **keystrokes** in MyCommands:

| Voice phrase | Keypress |
|---|---|
| "Chapter panel" | Ctrl+1 |
| "Editor panel" | Ctrl+2 |
| "A I panel" | Ctrl+3 |
| "Writing panel" | Ctrl+2 |
| "Feedback panel" | Ctrl+3 |

**How to create a keystroke command in Dragon:**
1. Tools → Add New Command
2. Command name: the voice phrase (e.g. "Chapter panel")
3. Type: **Keystroke**
4. Keystrokes: `{Ctrl+1}`
5. Click OK

---

## File Operations

| Voice phrase | Keypress |
|---|---|
| "Save project" | Ctrl+S |
| "Save project as" | Ctrl+Shift+S |
| "New project" | Ctrl+N |
| "Open project" | Ctrl+O |
| "Import document" | Ctrl+I |

---

## Chapter Management

| Voice phrase | Keypress |
|---|---|
| "Add chapter" | Ctrl+A |
| "New chapter" | Ctrl+A |
| "Rename chapter" | Ctrl+D |
| "Delete chapter" | Ctrl+Delete |
| "Move chapter up" | Alt+Up |
| "Move chapter down" | Alt+Down |

---

## AI Feedback

| Voice phrase | Keypress |
|---|---|
| "Run feedback" | Ctrl+F |
| "Comprehensive feedback" | Ctrl+F |
| "Get feedback" | Ctrl+F |

For specific feedback types (pacing, dialogue, style, structure), use Dragon's built-in  
"Click [button name]" targeting. Examples:
- Say **"Click Pacing"** to click the Pacing button in the AI panel
- Say **"Click Dialogue"** to click the Dialogue button
- Say **"Click Style"** to click the Style button
- Say **"Click Structure"** to click the Structure button

---

## Insert AI Response

| Voice phrase | Keypress | Or say |
|---|---|---|
| N/A | N/A | "Click At Cursor" |
| N/A | N/A | "Click At Start" |
| N/A | N/A | "Click At End" |

Dragon's native **"Click [button name]"** works directly for these.

---

## Export

| Voice phrase | Action |
|---|---|
| "Export manuscript" | Say "Click Export Manuscript" (uses Dragon's button targeting) |
| "Export as PDF" | Say "Click Export as PDF" |

---

## Tips for Dragon + VoiceBook Studio

### Dictation
1. Press **Ctrl+2** or say "Editor panel" to focus the writing editor.
2. Dragon will detect focus is in the editor text field and begin dictating there.
3. The editor is a standard WPF TextBox — all Dragon commands work: correct, format, select, etc.

### Navigating Chapter List
1. Say "Ctrl+1" or "Chapter panel" to focus the chapter list.
2. Say "Press Down" or "Press Up" to navigate chapters.
3. Dragon reads the chapter name as JAWS announces it.

### Using Chat Input
1. Focus Panel 3 (say "A I panel").
2. Say "Tab" to move to the Chat input field.
3. Dictate your question directly.
4. Say "Press Enter" to send.

### Dragon Vocabulary
Add these technical terms to Dragon's vocabulary for better recognition:
- VoiceBook (one word)
- VoiceBook Studio
- front matter
- back matter
- Prologue / Epilogue / Epigraph / Foreword / Preface / Afterword

Go to **Vocabulary → Add/Delete Words** and add these terms.

### Full Text Control (FTC)
VoiceBook Studio is a WPF application. Dragon Professional uses **UI Automation (UIA)** for WPF apps.  
If Dragon does not recognise the editor field:
1. Go to Dragon settings → **Miscellaneous**
2. Ensure **"Use UI Automation"** is enabled
3. Restart Dragon after making changes

---

## Complete MyCommand List (import template)

Below is a template you can refer to when entering commands in Dragon's MyCommands Editor.

```
Command: Chapter panel      Type: Keystroke    Keys: {Ctrl+1}
Command: Editor panel       Type: Keystroke    Keys: {Ctrl+2}
Command: A I panel          Type: Keystroke    Keys: {Ctrl+3}
Command: Writing panel      Type: Keystroke    Keys: {Ctrl+2}
Command: Save project       Type: Keystroke    Keys: {Ctrl+S}
Command: New project        Type: Keystroke    Keys: {Ctrl+N}
Command: Open project       Type: Keystroke    Keys: {Ctrl+O}
Command: Import document    Type: Keystroke    Keys: {Ctrl+I}
Command: Add chapter        Type: Keystroke    Keys: {Ctrl+A}
Command: Rename chapter     Type: Keystroke    Keys: {Ctrl+D}
Command: Delete chapter     Type: Keystroke    Keys: {Ctrl+Delete}
Command: Move chapter up    Type: Keystroke    Keys: {Alt+Up}
Command: Move chapter down  Type: Keystroke    Keys: {Alt+Down}
Command: Run feedback       Type: Keystroke    Keys: {Ctrl+F}
```
