# VoiceBook Studio — Integration Test Checklist

Run through each section in order. Mark each item PASS / FAIL / SKIP.

---

## 1. Voice Panel Navigation

> Prereq: App is open. Use Dragon / JSay or simulate via Ctrl+1/2/3.

- [ ] **Ctrl+1** moves focus to Chapter List (left panel)
- [ ] **Ctrl+2** moves focus to Writing Editor (centre panel)
- [ ] **Ctrl+3** moves focus to AI Assistant chat input (right panel)
- [ ] Voice command **"Panel 1"** moves focus to Chapter List
- [ ] Voice command **"Panel 2"** moves focus to Writing Editor
- [ ] Voice command **"Panel 3"** moves focus to AI Assistant
- [ ] Voice command **"Go to panel 1"** works
- [ ] Voice command **"Panel one"** works

---

## 2. JAWS Detection and Voice Separation

- [ ] With JAWS running: status bar shows **"JAWS: Running"**
- [ ] With JAWS not running: status bar shows **"JAWS: Not detected"**
- [ ] Voice command **"Voice off"** silences app TTS (JAWS continues to read)
- [ ] Voice command **"Voice on"** re-enables app TTS
- [ ] App TTS does not speak over JAWS when both are running

---

## 3. First-Run Tutorial

- [ ] First launch shows **Welcome dialog**
- [ ] Welcome dialog has a **"Don't show again"** checkbox
- [ ] Clicking **Start Tutorial** opens the Tutorial dialog
- [ ] All tutorial steps advance with **Next**
- [ ] Tutorial can go **Back** to a previous step
- [ ] **Repeat** re-reads the current step
- [ ] **Exit** closes the tutorial early
- [ ] Tutorial announces **"Tutorial complete"** at the end
- [ ] With "Don't show again" checked: relaunch skips Welcome dialog

---

## 4. Project Selection on Startup

- [ ] On launch, **"Which project would you like to work on today?"** is spoken
- [ ] **Project Selection dialog** appears
- [ ] Can **create a new project** from the dialog
- [ ] Can **open an existing project** from the dialog
- [ ] Selected project loads correctly into main window
- [ ] Voice command **"New project"** works from main window

---

## 5. Prompt Library

- [ ] **Prompts tab** is visible in the AI panel (right panel, tab 2)
- [ ] Category dropdown is populated (at least one category)
- [ ] Selecting a category filters the prompt list
- [ ] Selecting a prompt shows its preview text
- [ ] **"Use Prompt"** button loads prompt into chat input and switches to Chat tab
- [ ] Voice command **"Open prompts"** / **"Show prompts"** switches to Prompts tab
- [ ] Voice command **"Use prompt F3"** (or similar ID) loads that prompt

---

## 6. Response Cards

- [ ] **Cards tab** is visible in the AI panel (right panel, tab 3)
- [ ] **"Save card"** / **"Save response card"** voice command saves the current AI response
- [ ] Saved card appears in the Cards list
- [ ] **"Insert card 1"** voice command inserts card 1 into the editor at cursor
- [ ] **"Delete card 1"** voice command removes card 1
- [ ] **"Show Fiction cards"** (or any category) filters the card list

---

## 7. AI Chapter Detection on Import

> Prereq: Have a .docx file ready — one with Heading 1 styles, one without.

**Heuristic path (Heading styles present):**
- [ ] Import a .docx with Word Heading 1 / Heading 2 styles
- [ ] Chapter detection runs **without** calling Claude (fast, no API delay)
- [ ] Status bar shows **"Detected N chapters from headings"**
- [ ] Chapter Confirmation dialog shows correct chapter titles
- [ ] Accepting imports all chapters with **full content** (not truncated to 120 chars)

**AI fallback path (no headings):**
- [ ] Import a .docx with no heading styles (plain paragraphs)
- [ ] Status bar shows **"Asking Claude to detect chapter breaks…"**
- [ ] Chapter Confirmation dialog appears with Claude-suggested titles
- [ ] Each chapter has **full content** (verify by checking imported text length)
- [ ] Cancelling imports as single chapter

---

## 8. No WSR References

- [ ] Keyboard Shortcuts dialog contains no mention of "Windows Speech Recognition"
- [ ] About dialog contains no mention of "Windows Speech Recognition"
- [ ] Settings / menus contain no mention of "Windows Speech Recognition" or "WSR"

---

## 9. General Smoke Tests

- [ ] **Ctrl+N** creates a new project
- [ ] **Ctrl+S** saves the project
- [ ] **Ctrl+I** opens the Import dialog
- [ ] **Ctrl+A** adds a new chapter/section
- [ ] **Ctrl+D** renames selected chapter
- [ ] **Ctrl+Delete** deletes selected chapter
- [ ] **Alt+Up / Alt+Down** reorders chapters
- [ ] **Ctrl+F** triggers Comprehensive Feedback AI call
- [ ] AI chat **Send** button / Enter key sends message
- [ ] **"Insert at cursor"** inserts AI response at caret position
- [ ] Export Word (.docx) produces a valid file
- [ ] Export PDF produces a valid file
- [ ] **F1** help text is announced by JAWS on focused controls

---

## Notes / Failures

Record any failures here with steps to reproduce:

```
[Item]:
[Steps]:
[Expected]:
[Actual]:
```
