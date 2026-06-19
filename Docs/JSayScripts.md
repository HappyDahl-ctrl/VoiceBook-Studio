# JSay 23 — Script Recommendations for VoiceBook Studio

JSay 23 maps spoken phrases to keyboard shortcuts or multi-step macros.
Open **JSay Settings → Scripts** to add these entries.

---

## How JSay Scripts Work

Each script is a line in the format:

```
"spoken phrase" = {key combo}
"spoken phrase" = {key1}{delay}{key2}...
```

JSay listens continuously. When it recognises a phrase it sends the keystrokes to the active window.

---

## Recommended JSay Script File

Save the contents below as `VoiceBookStudio.jsy` and import via  
**JSay Settings → Scripts → Import**.

```
// VoiceBook Studio — JSay 23 Script
// Version 1.0

// =========================================
// Panel navigation
// =========================================
"chapter panel"    = {ctrl+1}
"chapters panel"   = {ctrl+1}
"panel one"        = {ctrl+1}
"go to panel one"  = {ctrl+1}
"panel 1"          = {ctrl+1}
"go to panel 1"    = {ctrl+1}

"editor panel"     = {ctrl+2}
"writing panel"    = {ctrl+2}
"panel two"        = {ctrl+2}
"go to panel two"  = {ctrl+2}
"panel 2"          = {ctrl+2}
"go to panel 2"    = {ctrl+2}

"AI panel"         = {ctrl+3}
"assistant panel"  = {ctrl+3}
"feedback panel"   = {ctrl+3}
"panel three"      = {ctrl+3}
"go to panel three" = {ctrl+3}
"panel 3"          = {ctrl+3}
"go to panel 3"    = {ctrl+3}

// =========================================
// File operations
// =========================================
"save project"       = {ctrl+s}
"save now"           = {ctrl+s}
"save"               = {ctrl+s}
"save as"            = {ctrl+shift+s}
"new project"        = {ctrl+n}
"open project"       = {ctrl+o}
"import document"    = {ctrl+i}

// =========================================
// Chapter management
// =========================================
"add chapter"        = {ctrl+a}
"new chapter"        = {ctrl+a}
"new section"        = {ctrl+a}
"rename chapter"     = {ctrl+d}
"rename"             = {ctrl+d}
"delete chapter"     = {ctrl+del}
"remove chapter"     = {ctrl+del}
"move up"            = {alt+up}
"move chapter up"    = {alt+up}
"move down"          = {alt+down}
"move chapter down"  = {alt+down}

// =========================================
// AI feedback
// =========================================
"run feedback"       = {ctrl+f}
"get feedback"       = {ctrl+f}
"comprehensive"      = {ctrl+f}
"comprehensive feedback" = {ctrl+f}
"feedback"           = {ctrl+f}

// =========================================
// Shortcuts dialog
// =========================================
"show shortcuts"     = {alt}hp{s}
"shortcuts"          = {alt}hp{s}
```

---

## Multi-step Macros

JSay 23 supports sequences with `{delay N}` (N = milliseconds).  
These macros navigate to a specific tab then focus the input:

```
// Switch to Chat tab and focus chat input
"open chat"   = {ctrl+3}{delay 200}{tab}{tab}{tab}{tab}{tab}{tab}
// (tab count depends on your exact UI — adjust as needed)

// Switch to Prompts tab
"open prompts" = {ctrl+3}{delay 200}{ctrl+tab}

// Switch to Cards tab
"open cards"   = {ctrl+3}{delay 200}{ctrl+tab}{ctrl+tab}
```

> **Tip:** Rather than counting tabs, use VoiceBook's voice commands.
> JSay can pass text to a running app if configured with the app's input hook.
> See section below.

---

## Connecting JSay to VoiceBook's Voice Command Router

VoiceBook Studio has a built-in voice command router. If JSay is configured to  
**send text to the active window**, you can route commands directly.

### Option A: JSay Keystroke Injection (recommended)
Use the keyboard shortcut mappings above. No additional configuration needed.

### Option B: Direct text routing (advanced)
If your JSay license supports it, configure JSay to call the VoiceBook command API.  
Currently VoiceBook listens to JSay via keyboard shortcuts only. Contact the developer  
to enable a named-pipe command channel for future integration.

---

## JSay Settings for Best Compatibility

### In JSay Settings:
| Setting | Value |
|---|---|
| Target application | VoiceBook Studio (or "All applications") |
| Recognition mode | Continuous |
| Confidence threshold | 75% (lower if commands are missed) |
| Delay after recognition | 50ms |
| Send as keystrokes | Enabled |

### Noise filter
Enable the JSay noise filter to avoid accidental command triggers while dictating  
into Dragon or the built-in Windows speech recogniser.

### Wake word (optional)
Consider setting a wake word like "VoiceBook" before commands if running Dragon  
simultaneously, to prevent command conflicts:
```
"VoiceBook save"       = {ctrl+s}
"VoiceBook chapter"    = {ctrl+1}
"VoiceBook editor"     = {ctrl+2}
```

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Commands not recognised | Check that VoiceBook Studio is the active window |
| Keystrokes not landing in right field | Use Ctrl+1/2/3 first to focus the correct panel |
| Dragon and JSay conflict | Add a JSay wake word to distinguish commands |
| Tab key count off for tab switching | Use Ctrl+Tab / Ctrl+Shift+Tab to navigate TabControl |
