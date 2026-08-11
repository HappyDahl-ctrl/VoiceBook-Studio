# VoiceBook Studio — Integration Test Checklist

For a real hands-on pass with a blind, hands-free user on their own PC, running
Dragon NaturallySpeaking and/or JAWS and/or JSay. Run through each section in
order — it follows the actual journey a new user takes: launch, tutorial,
import or write, get AI help, save, reopen. Mark each item PASS / FAIL / SKIP
and record failures in the Notes section at the end.

This checklist was last reconciled against the app on 2026-08-11 (4-panel
layout: Chapter Manager / Writing Editor / AI Assistant / Library). If the
panel layout or command set changes again, re-check this file against
`Services/VoiceCommandRouter.cs` and `ViewModels/MainViewModel.cs` before
trusting it.

---

## 0. Before You Start

- [ ] .NET 8 build completes with 0 errors (this checklist assumes a real
      Windows build — it cannot be verified from a Linux sandbox)
- [ ] An Anthropic API key is available for the AI-dependent sections
- [ ] Have a `.docx` manuscript ready for the import section — ideally one
      with Word Heading styles and, separately, one without any headings
- [ ] Note which assistive tech combination you're testing this pass with:
      JAWS only / Dragon only / Dragon + JAWS / Dragon + JSay / none (app's
      own mic + voice) — repeat the whole checklist once per combination if
      time allows, since several sections behave differently per combination

---

## 1. First Launch — Startup Sequence & Tutorial

> Prereq: fresh install, or Tutorial Reset from the Help menu, so
> `FirstLaunchComplete` is false.

- [ ] On launch, before the window appears, the AT-status announcement is
      spoken (or read by JAWS) — states whether JAWS/Dragon/JSay was detected
      and whether the built-in mic or Dragon owns the microphone
- [ ] Window appears and focus lands on the Chapter Manager list (Panel 1)
- [ ] **Welcome dialog** appears automatically (no click needed)
- [ ] Welcome dialog offers **Start Tutorial** and **Skip Tour**
- [ ] Choosing **Skip Tour** leaves the app usable and does NOT mark the
      tutorial complete — relaunching shows the Welcome dialog again
- [ ] Choosing **Start Tutorial** opens the non-modal Tutorial dialog and
      Step 1 (Audio Check) is spoken

**Walk every tutorial step** (22 total) — for each interactive one, actually
perform the action rather than clicking Skip, so the detection itself gets
exercised:

- [ ] Step 1–2: Audio/mic check — audio is audible, mic confirmation detected
      (say "Hello", or click Confirm Audio, or via Dragon per the on-screen
      instructions)
- [ ] Step 3–5: Orientation steps read correctly, mention the right AT setup
      for this machine (Dragon-specific vs. no-Dragon wording)
- [ ] "About VoiceBook Studio" describes **4 panels** including Library —
      confirm it does NOT say "three panels"
- [ ] Step 6: navigation overview mentions Panel 4 / F11 / Ctrl+4
- [ ] Step 7: switching to Panel 2 (Writing Editor) is detected and advances
- [ ] Step 8: switching to Panel 3 (AI Assistant) is detected and advances
- [ ] Step 9: switching to Panel 4 (Library) is detected and advances
- [ ] Step 10: returning to Panel 1 is detected and advances
- [ ] Step 11–12: "Other Voice Commands" mentions word count commands
- [ ] Step 13–16: New Project or Import Document — do **both** across two
      test runs; the tutorial detects `projectopened` and `addchapter`
      either way and advances
- [ ] Step 17: "Getting Help From Claude" explains Insert vs. Replace
      correctly
- [ ] Step 18: asking Claude a question in the chat box and sending it is
      detected (`sendchat`) and the tutorial advances on its own
- [ ] Step 19: "The Prompt Library" step's example commands actually work
      when tried (Prompt categories / Read prompt A / Use prompt A1)
- [ ] Step 20: saving that response as a card is detected (`savecard`) —
      **watch specifically for any visual/focus conflict** between the
      non-modal Tutorial dialog and the modal Save Card dialog; this pairing
      has not been exercised before and is the one part of this pass most
      likely to surface a real WPF window-layering bug
- [ ] Step 21: Save is detected and advances
- [ ] Step 22 (final): Quick Reference is read correctly, mentions Panel 4,
      Replace, Save card, and word count commands
- [ ] **Repeat** (R key or "repeat") re-reads the current step without
      advancing
- [ ] **Previous** (P key) goes back a step
- [ ] **Exit tutorial** closes the dialog early and still marks the tutorial
      complete (relaunch does NOT show Welcome dialog again)
- [ ] "Start tutorial" voice command re-opens the tutorial at any time later
      in the session

---

## 2. JAWS Detection and Voice Separation

- [ ] With JAWS running: status bar / AT summary shows JAWS detected
- [ ] With JAWS running: VoiceBook's own SAPI voice stays completely silent
      (no overlap with JAWS at any point, including tutorial steps and AI
      responses)
- [ ] With JAWS NOT running: VoiceBook's own voice reads status changes,
      tutorial steps, and announcements aloud
- [ ] "Voice off" / "Voice on" toggles VoiceBook's own TTS (only meaningful
      when JAWS is not running)
- [ ] Word count is **not** announced automatically while typing (this was a
      real bug — confirm it's actually fixed on real JAWS, not just in code)
- [ ] Word count IS announced immediately after selecting a chapter, right
      after the chapter title
- [ ] Word count IS announced immediately after selecting Whole Book, right
      after "Whole Book. Read only."
- [ ] "Word count" command speaks the right total for whichever is open
      (chapter vs. Whole Book)
- [ ] "Chapter word count" and "Book word count" each report the correct
      scope regardless of what's currently selected
- [ ] Tabbing through each panel: JAWS announces a sensible Name for every
      control (no unlabeled "button" or "edit" announcements)
- [ ] Status bar changes (save confirmations, errors) are announced via the
      live region without needing to manually navigate to the status bar

---

## 3. Dragon-Specific Checks

> Skip this section entirely if Dragon is not installed for this pass.

- [ ] Dragon NaturallySpeaking dictates into the Writing Editor exactly as
      it would into Microsoft Word — dictation, "Correct That", Dragon's own
      navigation/selection commands
- [ ] With Dragon running, the built-in VoiceBook mic is OFF (mic status
      shows Dragon owns the mic, no contention for the audio device)
- [ ] ScrollLock toggles between Dragon dictation and the app's own command
      listening, as described in the tutorial and Dragon Commands guide
- [ ] Typing/dictating a command into the Chat input box and pressing Enter
      runs it via `VoiceCommandRouter` — confirm with at least: "panel 1",
      "save", "insert at cursor", "word count", "replace"
- [ ] With Dragon MyCommands configured per the Dragon Commands Setup Guide:
      "Click `<button name>`" and "Select `<item name>`" work against real
      buttons/list items — this is the core claim that Dragon acts on the
      same UI Automation Names JAWS reads, and it needs a live check since
      it can't be verified from source alone
- [ ] Spot-check a handful of `AutomationProperties.Name` values Dragon
      should be able to target: "Save as Card", "Replace...", "Insert at
      Cursor", a chapter list item, a Library tab

---

## 4. JSay-Specific Checks

> Skip this section entirely if JSay is not installed for this pass.

- [ ] JSay-prefixed commands ("j-say help", etc.) do not conflict with app
      commands lacking that prefix
- [ ] Multi-step JSay scripts from `Docs/JSay_Scripts_VoiceBook.txt` that
      combine a keypress + TypeText into the Chat box + Enter work as
      documented (e.g. a "replace in chapter" JSay macro)

---

## 5. Project Lifecycle — Create, Save, Reopen

- [ ] "New project" / Ctrl+N creates a project and prompts for a title
- [ ] "Save" / Ctrl+S saves, confirms with a sound and a status message, and
      fires the tutorial's `save` signal if the tutorial is open
- [ ] Auto-save (every 30s when dirty) does not interrupt dictation or speak
      unexpectedly
- [ ] Closing the app and reopening it (or "Open project") reloads the same
      project with all chapters, content, word counts, saved cards, and
      saved feedback intact
- [ ] Whole Book word count is correct after reopening (sum of all chapters)

---

## 6. Importing a Manuscript

> Prereq: two `.docx` files — one with Word Heading 1/2 styles, one without.

**Heuristic path (headings present):**
- [ ] Import runs without an AI call (fast — no API delay, no "Asking
      Claude…" status)
- [ ] Status bar reports the number of chapters detected from headings
- [ ] Chapter Confirmation dialog shows correct titles
- [ ] Accepting imports full chapter content, not truncated

**AI fallback path (no headings):**
- [ ] Status bar shows Claude is being asked to detect chapter breaks
- [ ] Chapter Confirmation dialog shows Claude-suggested titles
- [ ] Each chapter has full content after accepting
- [ ] Cancelling falls back to a single chapter with all the text

---

## 7. Writing & Chapter Management

- [ ] Add / rename / delete / reorder chapters via keyboard, voice, and
      (if Dragon) MyCommands — status and live-region announcements match
      what happened
- [ ] Per-chapter word count updates as you type, visible on screen, but is
      **not** spoken on every keystroke (see Section 2)
- [ ] "Read chapter title" / "current chapter" announces title + word count
      together, correctly, for a real chapter
- [ ] Same command while Whole Book is selected announces "Whole Book. N
      words." instead of an error
- [ ] "Read chapter" / "read paragraph" read actual chapter content aloud

---

## 8. AI Assistant — Chat, Feedback, Insert, Replace

- [ ] Comprehensive / pacing / dialogue / style / structure feedback each
      run and return a real Claude response
- [ ] Chat: asking a question and sending it (voice or typed) returns a
      response with no filler ("Sure, here's your rewrite:") when asking for
      a rewrite — confirm the "return only the content" system prompt
      instruction actually holds up against a real model response, not just
      in theory
- [ ] Insert at Cursor / At Start / At End each place the response correctly
- [ ] **Replace**: ask for a specific, locatable rewrite ("rewrite paragraph
      2"), click/say Replace, confirm the *correct* original passage is
      swapped out (not the wrong paragraph, not appended)
- [ ] Replace on an ambiguous or non-locatable request reports it can't find
      a single passage, rather than silently replacing the wrong text
- [ ] Save response as a card; confirm it appears in the Library panel's
      Cards tab with the right title/category
- [ ] Feedback runs auto-save to the Feedback library tab (if this session's
      build includes that wiring — cross-check against the "PR3" backlog
      item if it seems to be missing)

---

## 9. Library Panel — Prompts, Cards, Feedback

- [ ] Panel 4 / F11 / Ctrl+4 / "Go to library" all focus the Library tab
      control directly (JAWS should announce the change — this was a real
      bug for the *voice-triggered* tab switches specifically; confirm both
      the direct panel-focus path and the "open prompt library" /
      "show response cards" voice-command path actually move focus)
- [ ] Prompt categories / Read prompt categories reads all categories with
      letters and counts
- [ ] Read prompt `<letter>` reads every prompt in that category
- [ ] Use prompt `<id>` sends that prompt to Claude
- [ ] Card categories / Read card categories works the same way for cards
- [ ] Insert card `<number>` and Insert/Use card `<letter><number>` both
      insert the right card into the chapter
- [ ] Delete card removes the selected card
- [ ] Feedback library / "what's in my feedback library" reads saved
      feedback entries; delete feedback entry works

---

## 10. Export

- [ ] Export Word produces a valid, openable `.docx`
- [ ] Export PDF produces a valid, openable PDF
- [ ] Both commands are reachable by voice and keyboard

---

## 11. General Smoke Tests

- [ ] Ctrl+1/2/3/4 and F2/F3/F11 all move focus to the right panel from
      anywhere, including from inside the editor while dictating
- [ ] F1 does nothing app-side (reserved for JAWS's own contextual help)
- [ ] "What can I say here" gives a correct, panel-specific answer in all
      **four** panels — Panel 4 previously fell back to the generic global
      list silently; confirm it now gives Library-specific commands
- [ ] Help dialog's Voice Commands Reference section lists all four panels
      and the word count / Replace commands

---

## Known Limitations of This Checklist

This checklist was authored and last reconciled by a coding agent working in
a Linux sandbox with no Windows, no .NET SDK, no Dragon/JAWS/JSay, and no
speakers/microphone — every item above is inferred from reading the source,
not observed running. Treat every box as unverified until a human checks it
on real hardware, and please update this file (and flag it back) if any item
turns out to be wrong or the behavior has since changed.

---

## Notes / Failures

Record any failures here with steps to reproduce:

```
[Item]:
[Steps]:
[Expected]:
[Actual]:
```
