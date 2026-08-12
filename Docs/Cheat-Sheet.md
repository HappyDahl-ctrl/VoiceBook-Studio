# VoiceBook Studio — Cheat Sheet

One-page quick reference. For full explanations see `User-Manual.md`; for the
complete voice command list see Section 16 there. This sheet only lists the
version that works everywhere — the underlying commands often have several
synonyms ("panel one" / "go to panel 1" / "panel 1" all work the same).

---

## Panels

| # | Panel | Key | Voice |
|---|---|---|---|
| 1 | Chapter Manager | Ctrl+1 (Escape from inside the editor) | "panel one" |
| 2 | Writing Editor | F2 or Ctrl+2 | "panel two" |
| 3 | AI Assistant (chat, always visible at the bottom) | F3 or Ctrl+3 | "panel three" |
| 4 | Library — Prompts / Cards / Feedback | F11 or Ctrl+4 | "panel four" / "go to library" |

**Only F2 / F3 / F11 / Escape work while actively dictating in the editor.**
Ctrl+1–4 only work when focus is outside the editor.

---

## Keyboard Shortcuts

| Keys | Action |
|---|---|
| Ctrl+N | New project |
| Ctrl+O | Open project |
| Ctrl+S | Save |
| Ctrl+Shift+S | Save As |
| Ctrl+I | Import Word document |
| Ctrl+A | Add chapter |
| Ctrl+D | Rename chapter |
| Ctrl+Delete | Delete chapter |
| Alt+Up / Alt+Down | Move chapter up / down |
| F6 / F7 | Next / previous chapter |
| Ctrl+F | Comprehensive AI feedback |
| F4 | Read current paragraph |
| Ctrl+F4 | Stop reading |
| F5 | Chapter title + word count |
| F8 | Read full chapter aloud |
| F9 | Application status |
| ScrollLock | Toggle app mic on/off (mutes/unmutes Dragon) |
| Ctrl+Shift+Space | Open command bar (focus chat input) |
| Escape | Return to Chapter Manager |

F1 and F10 are deliberately unbound (F1 = JAWS's own help; F10 = WPF menu bar).

---

## Voice Commands — Most Used

**Project**
"new project" · "open project" · "save" · "save as" · "import document" ·
"export word" · "export pdf"

**Chapters**
"add chapter" · "rename chapter" · "delete chapter" · "move up" / "move down" ·
"next chapter" / "previous chapter" · "change type"

**Reading & word count**
"read chapter" · "read paragraph" · "read chapter title" · "word count" ·
"chapter word count" · "book word count" · "stop"

**AI feedback**
"feedback" (comprehensive) · "pacing" · "dialogue" · "style" · "structure" ·
"book analysis"

**Chat**
"send" · "ask assistant [your question]"

**Using a response**
"read response" (or press Space) · "stop" / "pause" · "resume reading" ·
"insert at cursor" · "insert at start" · "insert at end" ·
"replace" — swaps a rewrite straight into the passage it replaces, no
selecting or copy/paste · "save card" · "discard response"

Reading a response speaks even with JAWS running — a deliberate, narrow
exception (see User-Manual.md Section 7) with a "Reading aloud" lead-in so
it's always clear which voice is talking. A slow AI request gets a periodic
"Still working" cue so it never reads as frozen.

**Prompt Library** (Panel 4)
"open prompt library" · "prompt categories" · "read prompt A" ·
"use prompt A1"

**Response Cards** (Panel 4)
"open response cards" · "card categories" · "insert card 1" ·
"insert card A1" · "delete card 1"

**Feedback Library** (Panel 4)
"open feedback library" · "feedback categories" · "read my pacing feedback" ·
"delete feedback entry"

**Anywhere**
"what can I say here" — hear commands for the current panel ·
"application status" · "start tutorial"

---

## Dragon NaturallySpeaking

**Dictation** — works immediately in the Writing Editor, no setup, identical
to Microsoft Word: dictate normally, "Correct that," "Scratch that,"
"Select [phrase]," full Dragon cursor navigation.

**Giving app commands while Dragon owns the mic** — three options:

1. **ScrollLock** (fastest, no setup) — press once, Dragon mutes and the app
   mic takes over; say a command; press ScrollLock again to give the mic
   back to Dragon.
2. **Command bar** (no setup) — Ctrl+Shift+Space to focus the chat input,
   dictate the command, press Enter. Works for every command above.
3. **Dragon MyCommands** (one-time setup) — map spoken phrases straight to
   keyboard shortcuts or command-bar sequences, so you never need
   ScrollLock. Full list and setup steps in `Dragon-Commands-Setup-Guide.md`
   and `Docs/Dragon_Commands_VoiceBook.xml`.

**Clicking buttons by name** — every button has a UI Automation Name Dragon
can target directly ("Click Save as Card," "Click Replace…"). If a click
command doesn't register, fall back to a MyCommands macro for that button —
see the setup guide.

**Example — get pacing feedback without breaking dictation flow:**
ScrollLock → say "pacing feedback" → ScrollLock → keep dictating.

---

## JAWS

- JAWS is the only voice when it's running — the app stays silent, no
  double-reading.
- No JAWS configuration needed; works out of the box.
- Word count is **not** read on every keystroke — only right after you
  select a chapter or Whole Book, or when you ask for it (see above).
- Voice-triggered panel/tab switches (e.g. "open prompt library") move
  keyboard focus along with them, so JAWS follows correctly.

---

## JSay

- JSay commands use a "j-say" spoken prefix and don't conflict with the
  commands above.
- Multi-step JSay scripts (keypress + typed command + Enter) are in
  `Docs/JSay_Scripts_VoiceBook.txt`.

---

*Full detail on every one of these lives in `User-Manual.md`. This sheet is
meant to be printed or kept open on a second screen — it intentionally
leaves out the long tail of synonyms and edge cases.*
